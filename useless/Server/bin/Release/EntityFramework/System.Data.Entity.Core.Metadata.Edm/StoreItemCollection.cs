using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.QueryCache;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm.Provider;
using System.Data.Entity.Core.SchemaObjectModel;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Text;
using System.Xml;

namespace System.Data.Entity.Core.Metadata.Edm;

public class StoreItemCollection : ItemCollection
{
	private class Loader
	{
		private string _provider;

		private string _providerManifestToken;

		private DbProviderManifest _providerManifest;

		private DbProviderFactory _providerFactory;

		private IList<EdmSchemaError> _errors;

		private IList<Schema> _schemas;

		private readonly bool _throwOnError;

		private readonly IDbDependencyResolver _resolver;

		public IList<EdmSchemaError> Errors => _errors;

		public IList<Schema> Schemas => _schemas;

		public DbProviderManifest ProviderManifest => _providerManifest;

		public DbProviderFactory ProviderFactory => _providerFactory;

		public string ProviderManifestToken => _providerManifestToken;

		public string ProviderInvariantName => _provider;

		public bool HasNonWarningErrors => !MetadataHelper.CheckIfAllErrorsAreWarnings(_errors);

		public Loader(IEnumerable<XmlReader> xmlReaders, IEnumerable<string> sourceFilePaths, bool throwOnError, IDbDependencyResolver resolver)
		{
			_throwOnError = throwOnError;
			IDbDependencyResolver resolver2;
			if (resolver != null)
			{
				IDbDependencyResolver dbDependencyResolver = new CompositeResolver<IDbDependencyResolver, IDbDependencyResolver>(resolver, DbConfiguration.DependencyResolver);
				resolver2 = dbDependencyResolver;
			}
			else
			{
				resolver2 = DbConfiguration.DependencyResolver;
			}
			_resolver = resolver2;
			LoadItems(xmlReaders, sourceFilePaths);
		}

		private void LoadItems(IEnumerable<XmlReader> xmlReaders, IEnumerable<string> sourceFilePaths)
		{
			_errors = SchemaManager.ParseAndValidate(xmlReaders, sourceFilePaths, SchemaDataModelOption.ProviderDataModel, OnProviderNotification, OnProviderManifestTokenNotification, OnProviderManifestNeeded, out _schemas);
			if (_throwOnError)
			{
				ThrowOnNonWarningErrors();
			}
		}

		internal void ThrowOnNonWarningErrors()
		{
			if (!MetadataHelper.CheckIfAllErrorsAreWarnings(_errors))
			{
				throw EntityUtil.InvalidSchemaEncountered(Helper.CombineErrorMessage(_errors));
			}
		}

		private void OnProviderNotification(string provider, Action<string, ErrorCode, EdmSchemaErrorSeverity> addError)
		{
			string provider2 = _provider;
			if (_provider == null)
			{
				_provider = provider;
				InitializeProviderManifest(addError);
			}
			else if (!(_provider == provider))
			{
				addError(Strings.AllArtifactsMustTargetSameProvider_InvariantName(provider2, _provider), ErrorCode.InconsistentProvider, EdmSchemaErrorSeverity.Error);
			}
		}

		private void InitializeProviderManifest(Action<string, ErrorCode, EdmSchemaErrorSeverity> addError)
		{
			if (_providerManifest != null || _providerManifestToken == null || _provider == null)
			{
				return;
			}
			DbProviderFactory dbProviderFactory = null;
			try
			{
				dbProviderFactory = DbConfiguration.DependencyResolver.GetService<DbProviderFactory>(_provider);
			}
			catch (ArgumentException ex)
			{
				addError(ex.Message, ErrorCode.InvalidProvider, EdmSchemaErrorSeverity.Error);
				return;
			}
			try
			{
				DbProviderServices service = _resolver.GetService<DbProviderServices>(_provider);
				_providerManifest = service.GetProviderManifest(_providerManifestToken);
				_providerFactory = dbProviderFactory;
				if (_providerManifest is EdmProviderManifest)
				{
					if (_throwOnError)
					{
						throw new NotSupportedException(Strings.OnlyStoreConnectionsSupported);
					}
					addError(Strings.OnlyStoreConnectionsSupported, ErrorCode.InvalidProvider, EdmSchemaErrorSeverity.Error);
				}
			}
			catch (ProviderIncompatibleException provEx)
			{
				if (_throwOnError)
				{
					throw;
				}
				AddProviderIncompatibleError(provEx, addError);
			}
		}

		private void OnProviderManifestTokenNotification(string token, Action<string, ErrorCode, EdmSchemaErrorSeverity> addError)
		{
			if (_providerManifestToken == null)
			{
				_providerManifestToken = token;
				InitializeProviderManifest(addError);
			}
			else if (_providerManifestToken != token)
			{
				addError(Strings.AllArtifactsMustTargetSameProvider_ManifestToken(token, _providerManifestToken), ErrorCode.ProviderManifestTokenMismatch, EdmSchemaErrorSeverity.Error);
			}
		}

		private DbProviderManifest OnProviderManifestNeeded(Action<string, ErrorCode, EdmSchemaErrorSeverity> addError)
		{
			if (_providerManifest == null)
			{
				addError(Strings.ProviderManifestTokenNotFound, ErrorCode.ProviderManifestTokenNotFound, EdmSchemaErrorSeverity.Error);
			}
			return _providerManifest;
		}

		private static void AddProviderIncompatibleError(ProviderIncompatibleException provEx, Action<string, ErrorCode, EdmSchemaErrorSeverity> addError)
		{
			StringBuilder stringBuilder = new StringBuilder(provEx.Message);
			if (provEx.InnerException != null && !string.IsNullOrEmpty(provEx.InnerException.Message))
			{
				stringBuilder.AppendFormat(" {0}", provEx.InnerException.Message);
			}
			addError(stringBuilder.ToString(), ErrorCode.FailedToRetrieveProviderManifest, EdmSchemaErrorSeverity.Error);
		}
	}

	private double _schemaVersion;

	private readonly CacheForPrimitiveTypes _primitiveTypeMaps = new CacheForPrimitiveTypes();

	private readonly Memoizer<EdmFunction, EdmFunction> _cachedCTypeFunction;

	private readonly DbProviderManifest _providerManifest;

	private readonly string _providerInvariantName;

	private readonly string _providerManifestToken;

	private readonly DbProviderFactory _providerFactory;

	private readonly QueryCacheManager _queryCacheManager = QueryCacheManager.Create();

	internal QueryCacheManager QueryCacheManager => _queryCacheManager;

	public virtual DbProviderFactory ProviderFactory => _providerFactory;

	public virtual DbProviderManifest ProviderManifest => _providerManifest;

	public virtual string ProviderManifestToken => _providerManifestToken;

	public virtual string ProviderInvariantName => _providerInvariantName;

	public double StoreSchemaVersion
	{
		get
		{
			return _schemaVersion;
		}
		internal set
		{
			_schemaVersion = value;
		}
	}

	internal StoreItemCollection()
		: base(DataSpace.SSpace)
	{
	}

	internal StoreItemCollection(DbProviderFactory factory, DbProviderManifest manifest, string providerInvariantName, string providerManifestToken)
		: base(DataSpace.SSpace)
	{
		_providerFactory = factory;
		_providerManifest = manifest;
		_providerInvariantName = providerInvariantName;
		_providerManifestToken = providerManifestToken;
		_cachedCTypeFunction = new Memoizer<EdmFunction, EdmFunction>(ConvertFunctionSignatureToCType, null);
		LoadProviderManifest(_providerManifest);
	}

	private StoreItemCollection(IEnumerable<XmlReader> xmlReaders, ReadOnlyCollection<string> filePaths, IDbDependencyResolver resolver, out IList<EdmSchemaError> errors)
		: base(DataSpace.SSpace)
	{
		errors = Init(xmlReaders, filePaths, throwOnError: false, resolver, out _providerManifest, out _providerFactory, out _providerInvariantName, out _providerManifestToken, out _cachedCTypeFunction);
	}

	internal StoreItemCollection(IEnumerable<XmlReader> xmlReaders, IEnumerable<string> filePaths)
		: base(DataSpace.SSpace)
	{
		EntityUtil.CheckArgumentEmpty(ref xmlReaders, Strings.StoreItemCollectionMustHaveOneArtifact, "xmlReader");
		Init(xmlReaders, filePaths, throwOnError: true, null, out _providerManifest, out _providerFactory, out _providerInvariantName, out _providerManifestToken, out _cachedCTypeFunction);
	}

	public StoreItemCollection(IEnumerable<XmlReader> xmlReaders)
		: base(DataSpace.SSpace)
	{
		Check.NotNull(xmlReaders, "xmlReaders");
		EntityUtil.CheckArgumentEmpty(ref xmlReaders, Strings.StoreItemCollectionMustHaveOneArtifact, "xmlReader");
		MetadataArtifactLoader metadataArtifactLoader = MetadataArtifactLoader.CreateCompositeFromXmlReaders(xmlReaders);
		Init(metadataArtifactLoader.GetReaders(), metadataArtifactLoader.GetPaths(), throwOnError: true, null, out _providerManifest, out _providerFactory, out _providerInvariantName, out _providerManifestToken, out _cachedCTypeFunction);
	}

	public StoreItemCollection(EdmModel model)
		: base(DataSpace.SSpace)
	{
		Check.NotNull(model, "model");
		_providerManifest = model.ProviderManifest;
		_providerInvariantName = model.ProviderInfo.ProviderInvariantName;
		_providerFactory = DbConfiguration.DependencyResolver.GetService<DbProviderFactory>(_providerInvariantName);
		_providerManifestToken = model.ProviderInfo.ProviderManifestToken;
		_cachedCTypeFunction = new Memoizer<EdmFunction, EdmFunction>(ConvertFunctionSignatureToCType, null);
		LoadProviderManifest(_providerManifest);
		_schemaVersion = model.SchemaVersion;
		model.Validate();
		foreach (GlobalItem globalItem in model.GlobalItems)
		{
			globalItem.SetReadOnly();
			AddInternal(globalItem);
		}
	}

	public StoreItemCollection(params string[] filePaths)
		: base(DataSpace.SSpace)
	{
		Check.NotNull(filePaths, "filePaths");
		IEnumerable<string> enumerableArgument = filePaths;
		EntityUtil.CheckArgumentEmpty(ref enumerableArgument, Strings.StoreItemCollectionMustHaveOneArtifact, "filePaths");
		MetadataArtifactLoader metadataArtifactLoader = null;
		List<XmlReader> list = null;
		try
		{
			metadataArtifactLoader = MetadataArtifactLoader.CreateCompositeFromFilePaths(enumerableArgument, ".ssdl");
			list = metadataArtifactLoader.CreateReaders(DataSpace.SSpace);
			IEnumerable<XmlReader> enumerableArgument2 = list.AsEnumerable();
			EntityUtil.CheckArgumentEmpty(ref enumerableArgument2, Strings.StoreItemCollectionMustHaveOneArtifact, "filePaths");
			Init(list, metadataArtifactLoader.GetPaths(DataSpace.SSpace), throwOnError: true, null, out _providerManifest, out _providerFactory, out _providerInvariantName, out _providerManifestToken, out _cachedCTypeFunction);
		}
		finally
		{
			if (list != null)
			{
				Helper.DisposeXmlReaders(list);
			}
		}
	}

	private IList<EdmSchemaError> Init(IEnumerable<XmlReader> xmlReaders, IEnumerable<string> filePaths, bool throwOnError, IDbDependencyResolver resolver, out DbProviderManifest providerManifest, out DbProviderFactory providerFactory, out string providerInvariantName, out string providerManifestToken, out Memoizer<EdmFunction, EdmFunction> cachedCTypeFunction)
	{
		cachedCTypeFunction = new Memoizer<EdmFunction, EdmFunction>(ConvertFunctionSignatureToCType, null);
		Loader loader = new Loader(xmlReaders, filePaths, throwOnError, resolver);
		providerFactory = loader.ProviderFactory;
		providerManifest = loader.ProviderManifest;
		providerManifestToken = loader.ProviderManifestToken;
		providerInvariantName = loader.ProviderInvariantName;
		if (!loader.HasNonWarningErrors)
		{
			LoadProviderManifest(loader.ProviderManifest);
			List<EdmSchemaError> list = EdmItemCollection.LoadItems(_providerManifest, loader.Schemas, this);
			foreach (EdmSchemaError item in list)
			{
				loader.Errors.Add(item);
			}
			if (throwOnError && list.Count != 0)
			{
				loader.ThrowOnNonWarningErrors();
			}
		}
		return loader.Errors;
	}

	public virtual ReadOnlyCollection<PrimitiveType> GetPrimitiveTypes()
	{
		return _primitiveTypeMaps.GetTypes();
	}

	internal override PrimitiveType GetMappedPrimitiveType(PrimitiveTypeKind primitiveTypeKind)
	{
		PrimitiveType type = null;
		_primitiveTypeMaps.TryGetType(primitiveTypeKind, null, out type);
		return type;
	}

	private void LoadProviderManifest(DbProviderManifest storeManifest)
	{
		foreach (PrimitiveType storeType in storeManifest.GetStoreTypes())
		{
			AddInternal(storeType);
			_primitiveTypeMaps.Add(storeType);
		}
		foreach (EdmFunction storeFunction in storeManifest.GetStoreFunctions())
		{
			AddInternal(storeFunction);
		}
	}

	internal ReadOnlyCollection<EdmFunction> GetCTypeFunctions(string functionName, bool ignoreCase)
	{
		if (base.FunctionLookUpTable.TryGetValue(functionName, out var value))
		{
			value = ConvertToCTypeFunctions(value);
			if (ignoreCase)
			{
				return value;
			}
			return ItemCollection.GetCaseSensitiveFunctions(value, functionName);
		}
		return Helper.EmptyEdmFunctionReadOnlyCollection;
	}

	private ReadOnlyCollection<EdmFunction> ConvertToCTypeFunctions(ReadOnlyCollection<EdmFunction> functionOverloads)
	{
		List<EdmFunction> list = new List<EdmFunction>();
		foreach (EdmFunction functionOverload in functionOverloads)
		{
			list.Add(ConvertToCTypeFunction(functionOverload));
		}
		return new ReadOnlyCollection<EdmFunction>(list);
	}

	internal EdmFunction ConvertToCTypeFunction(EdmFunction sTypeFunction)
	{
		return _cachedCTypeFunction.Evaluate(sTypeFunction);
	}

	internal static EdmFunction ConvertFunctionSignatureToCType(EdmFunction sTypeFunction)
	{
		if (sTypeFunction.IsFromProviderManifest)
		{
			return sTypeFunction;
		}
		FunctionParameter functionParameter = null;
		if (sTypeFunction.ReturnParameter != null)
		{
			TypeUsage typeUsage = MetadataHelper.ConvertStoreTypeUsageToEdmTypeUsage(sTypeFunction.ReturnParameter.TypeUsage);
			functionParameter = new FunctionParameter(sTypeFunction.ReturnParameter.Name, typeUsage, sTypeFunction.ReturnParameter.GetParameterMode());
		}
		List<FunctionParameter> list = new List<FunctionParameter>();
		if (sTypeFunction.Parameters.Count > 0)
		{
			foreach (FunctionParameter parameter in sTypeFunction.Parameters)
			{
				TypeUsage typeUsage2 = MetadataHelper.ConvertStoreTypeUsageToEdmTypeUsage(parameter.TypeUsage);
				FunctionParameter item = new FunctionParameter(parameter.Name, typeUsage2, parameter.GetParameterMode());
				list.Add(item);
			}
		}
		FunctionParameter[] returnParameters = ((functionParameter == null) ? new FunctionParameter[0] : new FunctionParameter[1] { functionParameter });
		EdmFunction edmFunction = new EdmFunction(sTypeFunction.Name, sTypeFunction.NamespaceName, DataSpace.CSpace, new EdmFunctionPayload
		{
			Schema = sTypeFunction.Schema,
			StoreFunctionName = sTypeFunction.StoreFunctionNameAttribute,
			CommandText = sTypeFunction.CommandTextAttribute,
			IsAggregate = sTypeFunction.AggregateAttribute,
			IsBuiltIn = sTypeFunction.BuiltInAttribute,
			IsNiladic = sTypeFunction.NiladicFunctionAttribute,
			IsComposable = sTypeFunction.IsComposableAttribute,
			IsFromProviderManifest = sTypeFunction.IsFromProviderManifest,
			IsCachedStoreFunction = true,
			IsFunctionImport = sTypeFunction.IsFunctionImport,
			ReturnParameters = returnParameters,
			Parameters = list.ToArray(),
			ParameterTypeSemantics = sTypeFunction.ParameterTypeSemanticsAttribute
		});
		edmFunction.SetReadOnly();
		return edmFunction;
	}

	public static StoreItemCollection Create(IEnumerable<XmlReader> xmlReaders, ReadOnlyCollection<string> filePaths, IDbDependencyResolver resolver, out IList<EdmSchemaError> errors)
	{
		Check.NotNull(xmlReaders, "xmlReaders");
		EntityUtil.CheckArgumentContainsNull(ref xmlReaders, "xmlReaders");
		EntityUtil.CheckArgumentEmpty(ref xmlReaders, Strings.StoreItemCollectionMustHaveOneArtifact, "xmlReaders");
		StoreItemCollection result = new StoreItemCollection(xmlReaders, filePaths, resolver, out errors);
		if (errors == null || errors.Count <= 0)
		{
			return result;
		}
		return null;
	}
}
