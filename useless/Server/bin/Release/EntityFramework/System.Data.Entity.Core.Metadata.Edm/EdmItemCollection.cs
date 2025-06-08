using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.Utils;
using System.Data.Entity.Core.Metadata.Edm.Provider;
using System.Data.Entity.Core.Objects.ELinq;
using System.Data.Entity.Core.SchemaObjectModel;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;

namespace System.Data.Entity.Core.Metadata.Edm;

public sealed class EdmItemCollection : ItemCollection
{
	private readonly CacheForPrimitiveTypes _primitiveTypeMaps = new CacheForPrimitiveTypes();

	private double _edmVersion;

	private Memoizer<InitializerMetadata, InitializerMetadata> _getCanonicalInitializerMetadataMemoizer;

	private Memoizer<EdmFunction, DbLambda> _getGeneratedFunctionDefinitionsMemoizer;

	private readonly OcAssemblyCache _conventionalOcCache = new OcAssemblyCache();

	public double EdmVersion
	{
		get
		{
			return _edmVersion;
		}
		internal set
		{
			_edmVersion = value;
		}
	}

	internal OcAssemblyCache ConventionalOcCache => _conventionalOcCache;

	internal EdmItemCollection(IEnumerable<XmlReader> xmlReaders, IEnumerable<string> filePaths, bool skipInitialization = false)
		: base(DataSpace.CSpace)
	{
		if (!skipInitialization)
		{
			Init(xmlReaders, filePaths, throwOnError: true);
		}
	}

	public EdmItemCollection(IEnumerable<XmlReader> xmlReaders)
		: base(DataSpace.CSpace)
	{
		Check.NotNull(xmlReaders, "xmlReaders");
		EntityUtil.CheckArgumentContainsNull(ref xmlReaders, "xmlReaders");
		MetadataArtifactLoader metadataArtifactLoader = MetadataArtifactLoader.CreateCompositeFromXmlReaders(xmlReaders);
		Init(metadataArtifactLoader.GetReaders(), metadataArtifactLoader.GetPaths(), throwOnError: true);
	}

	public EdmItemCollection(EdmModel model)
		: base(DataSpace.CSpace)
	{
		Check.NotNull(model, "model");
		Init();
		_edmVersion = model.SchemaVersion;
		model.Validate();
		foreach (GlobalItem globalItem in model.GlobalItems)
		{
			globalItem.SetReadOnly();
			AddInternal(globalItem);
		}
	}

	public EdmItemCollection(params string[] filePaths)
		: base(DataSpace.CSpace)
	{
		Check.NotNull(filePaths, "filePaths");
		MetadataArtifactLoader metadataArtifactLoader = null;
		List<XmlReader> list = null;
		try
		{
			metadataArtifactLoader = MetadataArtifactLoader.CreateCompositeFromFilePaths(filePaths, ".csdl");
			list = metadataArtifactLoader.CreateReaders(DataSpace.CSpace);
			Init(list, metadataArtifactLoader.GetPaths(DataSpace.CSpace), throwOnError: true);
		}
		finally
		{
			if (list != null)
			{
				Helper.DisposeXmlReaders(list);
			}
		}
	}

	private EdmItemCollection(IEnumerable<XmlReader> xmlReaders, ReadOnlyCollection<string> filePaths, out IList<EdmSchemaError> errors)
		: base(DataSpace.CSpace)
	{
		errors = Init(xmlReaders, filePaths, throwOnError: false);
	}

	private void Init()
	{
		LoadEdmPrimitiveTypesAndFunctions();
	}

	private IList<EdmSchemaError> Init(IEnumerable<XmlReader> xmlReaders, IEnumerable<string> filePaths, bool throwOnError)
	{
		Init();
		return LoadItems(xmlReaders, filePaths, SchemaDataModelOption.EntityDataModel, MetadataItem.EdmProviderManifest, this, throwOnError);
	}

	internal InitializerMetadata GetCanonicalInitializerMetadata(InitializerMetadata metadata)
	{
		if (_getCanonicalInitializerMetadataMemoizer == null)
		{
			Interlocked.CompareExchange(ref _getCanonicalInitializerMetadataMemoizer, new Memoizer<InitializerMetadata, InitializerMetadata>((InitializerMetadata m) => m, EqualityComparer<InitializerMetadata>.Default), null);
		}
		return _getCanonicalInitializerMetadataMemoizer.Evaluate(metadata);
	}

	internal static bool IsSystemNamespace(DbProviderManifest manifest, string namespaceName)
	{
		if (manifest == MetadataItem.EdmProviderManifest)
		{
			if (!(namespaceName == "Transient") && !(namespaceName == "Edm"))
			{
				return namespaceName == "System";
			}
			return true;
		}
		switch (namespaceName)
		{
		default:
			if (manifest != null)
			{
				return namespaceName == manifest.NamespaceName;
			}
			return false;
		case "Transient":
		case "Edm":
		case "System":
			return true;
		}
	}

	internal static IList<EdmSchemaError> LoadItems(IEnumerable<XmlReader> xmlReaders, IEnumerable<string> sourceFilePaths, SchemaDataModelOption dataModelOption, DbProviderManifest providerManifest, ItemCollection itemCollection, bool throwOnError)
	{
		IList<Schema> schemaCollection = null;
		IList<EdmSchemaError> list = SchemaManager.ParseAndValidate(xmlReaders, sourceFilePaths, dataModelOption, providerManifest, out schemaCollection);
		if (MetadataHelper.CheckIfAllErrorsAreWarnings(list))
		{
			foreach (EdmSchemaError item in LoadItems(providerManifest, schemaCollection, itemCollection))
			{
				list.Add(item);
			}
		}
		if (!MetadataHelper.CheckIfAllErrorsAreWarnings(list) && throwOnError)
		{
			throw EntityUtil.InvalidSchemaEncountered(Helper.CombineErrorMessage(list));
		}
		return list;
	}

	internal static List<EdmSchemaError> LoadItems(DbProviderManifest manifest, IList<Schema> somSchemas, ItemCollection itemCollection)
	{
		List<EdmSchemaError> list = new List<EdmSchemaError>();
		IEnumerable<GlobalItem> enumerable = LoadSomSchema(somSchemas, manifest, itemCollection);
		List<string> list2 = new List<string>();
		foreach (GlobalItem item in enumerable)
		{
			if (item.BuiltInTypeKind == BuiltInTypeKind.EdmFunction && item.DataSpace == DataSpace.SSpace)
			{
				EdmFunction edmFunction = (EdmFunction)item;
				StringBuilder stringBuilder = new StringBuilder();
				EdmFunction.BuildIdentity(stringBuilder, edmFunction.FullName, edmFunction.Parameters, (FunctionParameter param) => MetadataHelper.ConvertStoreTypeUsageToEdmTypeUsage(param.TypeUsage), (FunctionParameter param) => param.Mode);
				string text = stringBuilder.ToString();
				if (list2.Contains(text))
				{
					list.Add(new EdmSchemaError(Strings.DuplicatedFunctionoverloads(edmFunction.FullName, text.Substring(edmFunction.FullName.Length)).Trim(), 174, EdmSchemaErrorSeverity.Error));
					continue;
				}
				list2.Add(text);
			}
			item.SetReadOnly();
			itemCollection.AddInternal(item);
		}
		return list;
	}

	internal static IEnumerable<GlobalItem> LoadSomSchema(IList<Schema> somSchemas, DbProviderManifest providerManifest, ItemCollection itemCollection)
	{
		return Converter.ConvertSchema(somSchemas, providerManifest, itemCollection);
	}

	public ReadOnlyCollection<PrimitiveType> GetPrimitiveTypes()
	{
		return _primitiveTypeMaps.GetTypes();
	}

	public ReadOnlyCollection<PrimitiveType> GetPrimitiveTypes(double edmVersion)
	{
		if (edmVersion == 1.0 || edmVersion == 1.1 || edmVersion == 2.0)
		{
			return new ReadOnlyCollection<PrimitiveType>((from type in _primitiveTypeMaps.GetTypes()
				where !Helper.IsSpatialType(type) && !Helper.IsHierarchyIdType(type)
				select type).ToList());
		}
		if (edmVersion == 3.0)
		{
			return _primitiveTypeMaps.GetTypes();
		}
		throw new ArgumentException(Strings.InvalidEDMVersion(edmVersion.ToString(CultureInfo.CurrentCulture)));
	}

	internal override PrimitiveType GetMappedPrimitiveType(PrimitiveTypeKind primitiveTypeKind)
	{
		PrimitiveType type = null;
		_primitiveTypeMaps.TryGetType(primitiveTypeKind, null, out type);
		return type;
	}

	private void LoadEdmPrimitiveTypesAndFunctions()
	{
		EdmProviderManifest instance = EdmProviderManifest.Instance;
		ReadOnlyCollection<PrimitiveType> storeTypes = instance.GetStoreTypes();
		for (int i = 0; i < storeTypes.Count; i++)
		{
			AddInternal(storeTypes[i]);
			_primitiveTypeMaps.Add(storeTypes[i]);
		}
		ReadOnlyCollection<EdmFunction> storeFunctions = instance.GetStoreFunctions();
		for (int j = 0; j < storeFunctions.Count; j++)
		{
			AddInternal(storeFunctions[j]);
		}
	}

	internal DbLambda GetGeneratedFunctionDefinition(EdmFunction function)
	{
		if (_getGeneratedFunctionDefinitionsMemoizer == null)
		{
			Interlocked.CompareExchange(ref _getGeneratedFunctionDefinitionsMemoizer, new Memoizer<EdmFunction, DbLambda>(GenerateFunctionDefinition, null), null);
		}
		return _getGeneratedFunctionDefinitionsMemoizer.Evaluate(function);
	}

	internal DbLambda GenerateFunctionDefinition(EdmFunction function)
	{
		if (!function.HasUserDefinedBody)
		{
			throw new InvalidOperationException(Strings.Cqt_UDF_FunctionHasNoDefinition(function.Identity));
		}
		DbLambda dbLambda = ExternalCalls.CompileFunctionDefinition(function.CommandTextAttribute, function.Parameters, this);
		if (!TypeSemantics.IsStructurallyEqual(function.ReturnParameter.TypeUsage, dbLambda.Body.ResultType))
		{
			throw new InvalidOperationException(Strings.Cqt_UDF_FunctionDefinitionResultTypeMismatch(function.ReturnParameter.TypeUsage.ToString(), function.FullName, dbLambda.Body.ResultType.ToString()));
		}
		return dbLambda;
	}

	public static EdmItemCollection Create(IEnumerable<XmlReader> xmlReaders, ReadOnlyCollection<string> filePaths, out IList<EdmSchemaError> errors)
	{
		Check.NotNull(xmlReaders, "xmlReaders");
		EntityUtil.CheckArgumentContainsNull(ref xmlReaders, "xmlReaders");
		EdmItemCollection result = new EdmItemCollection(xmlReaders, filePaths, out errors);
		if (errors == null || errors.Count <= 0)
		{
			return result;
		}
		return null;
	}
}
