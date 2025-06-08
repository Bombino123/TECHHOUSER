using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.EntitySql;
using System.Data.Entity.Core.Common.QueryCache;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Mapping.Update.Internal;
using System.Data.Entity.Core.Mapping.ViewGeneration;
using System.Data.Entity.Core.Metadata.Edm.Provider;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Core.Objects.ELinq;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace System.Data.Entity.Core.Metadata.Edm;

public class MetadataWorkspace
{
	private Lazy<EdmItemCollection> _itemsCSpace;

	private Lazy<StoreItemCollection> _itemsSSpace;

	private Lazy<ObjectItemCollection> _itemsOSpace;

	private Lazy<StorageMappingItemCollection> _itemsCSSpace;

	private Lazy<DefaultObjectMappingItemCollection> _itemsOCSpace;

	private bool _foundAssemblyWithAttribute;

	private double _schemaVersion;

	private readonly object _schemaVersionLock = new object();

	private readonly Guid _metadataWorkspaceId = Guid.NewGuid();

	internal readonly MetadataOptimization MetadataOptimization;

	private static readonly double _maximumEdmVersionSupported = SupportedEdmVersions.Last();

	private static IEnumerable<double> SupportedEdmVersions
	{
		get
		{
			yield return 0.0;
			yield return 1.0;
			yield return 2.0;
			yield return 3.0;
		}
	}

	public static double MaximumEdmVersionSupported => _maximumEdmVersionSupported;

	internal virtual Guid MetadataWorkspaceId => _metadataWorkspaceId;

	public MetadataWorkspace()
	{
		_itemsOSpace = new Lazy<ObjectItemCollection>(() => new ObjectItemCollection(), isThreadSafe: true);
		MetadataOptimization = new MetadataOptimization(this);
	}

	public MetadataWorkspace(Func<EdmItemCollection> cSpaceLoader, Func<StoreItemCollection> sSpaceLoader, Func<StorageMappingItemCollection> csMappingLoader, Func<ObjectItemCollection> oSpaceLoader)
	{
		MetadataWorkspace metadataWorkspace = this;
		Check.NotNull(cSpaceLoader, "cSpaceLoader");
		Check.NotNull(sSpaceLoader, "sSpaceLoader");
		Check.NotNull(csMappingLoader, "csMappingLoader");
		Check.NotNull(oSpaceLoader, "oSpaceLoader");
		_itemsCSpace = new Lazy<EdmItemCollection>(() => metadataWorkspace.LoadAndCheckItemCollection(cSpaceLoader), isThreadSafe: true);
		_itemsSSpace = new Lazy<StoreItemCollection>(() => metadataWorkspace.LoadAndCheckItemCollection(sSpaceLoader), isThreadSafe: true);
		_itemsOSpace = new Lazy<ObjectItemCollection>(oSpaceLoader, isThreadSafe: true);
		_itemsCSSpace = new Lazy<StorageMappingItemCollection>(() => metadataWorkspace.LoadAndCheckItemCollection(csMappingLoader), isThreadSafe: true);
		_itemsOCSpace = new Lazy<DefaultObjectMappingItemCollection>(() => new DefaultObjectMappingItemCollection(metadataWorkspace._itemsCSpace.Value, metadataWorkspace._itemsOSpace.Value), isThreadSafe: true);
		MetadataOptimization = new MetadataOptimization(this);
	}

	public MetadataWorkspace(Func<EdmItemCollection> cSpaceLoader, Func<StoreItemCollection> sSpaceLoader, Func<StorageMappingItemCollection> csMappingLoader)
	{
		MetadataWorkspace metadataWorkspace = this;
		Check.NotNull(cSpaceLoader, "cSpaceLoader");
		Check.NotNull(sSpaceLoader, "sSpaceLoader");
		Check.NotNull(csMappingLoader, "csMappingLoader");
		_itemsCSpace = new Lazy<EdmItemCollection>(() => metadataWorkspace.LoadAndCheckItemCollection(cSpaceLoader), isThreadSafe: true);
		_itemsSSpace = new Lazy<StoreItemCollection>(() => metadataWorkspace.LoadAndCheckItemCollection(sSpaceLoader), isThreadSafe: true);
		_itemsOSpace = new Lazy<ObjectItemCollection>(() => new ObjectItemCollection(), isThreadSafe: true);
		_itemsCSSpace = new Lazy<StorageMappingItemCollection>(() => metadataWorkspace.LoadAndCheckItemCollection(csMappingLoader), isThreadSafe: true);
		_itemsOCSpace = new Lazy<DefaultObjectMappingItemCollection>(() => new DefaultObjectMappingItemCollection(metadataWorkspace._itemsCSpace.Value, metadataWorkspace._itemsOSpace.Value), isThreadSafe: true);
		MetadataOptimization = new MetadataOptimization(this);
	}

	public MetadataWorkspace(IEnumerable<string> paths, IEnumerable<Assembly> assembliesToConsider)
	{
		Check.NotNull(paths, "paths");
		Check.NotNull(assembliesToConsider, "assembliesToConsider");
		EntityUtil.CheckArgumentContainsNull(ref paths, "paths");
		EntityUtil.CheckArgumentContainsNull(ref assembliesToConsider, "assembliesToConsider");
		Func<AssemblyName, Assembly> resolveReference = delegate(AssemblyName referenceName)
		{
			foreach (Assembly item in assembliesToConsider)
			{
				if (AssemblyName.ReferenceMatchesDefinition(referenceName, new AssemblyName(item.FullName)))
				{
					return item;
				}
			}
			throw new ArgumentException(Strings.AssemblyMissingFromAssembliesToConsider(referenceName.FullName), "assembliesToConsider");
		};
		CreateMetadataWorkspaceWithResolver(paths, () => assembliesToConsider, resolveReference);
		MetadataOptimization = new MetadataOptimization(this);
	}

	private void CreateMetadataWorkspaceWithResolver(IEnumerable<string> paths, Func<IEnumerable<Assembly>> wildcardAssemblies, Func<AssemblyName, Assembly> resolveReference)
	{
		MetadataArtifactLoader metadataArtifactLoader = MetadataArtifactLoader.CreateCompositeFromFilePaths(paths.ToArray(), "", new CustomAssemblyResolver(wildcardAssemblies, resolveReference));
		_itemsOSpace = new Lazy<ObjectItemCollection>(() => new ObjectItemCollection(), isThreadSafe: true);
		using (DisposableCollectionWrapper<XmlReader> disposableCollectionWrapper = new DisposableCollectionWrapper<XmlReader>(metadataArtifactLoader.CreateReaders(DataSpace.CSpace)))
		{
			if (disposableCollectionWrapper.Any())
			{
				EdmItemCollection itemCollection2 = new EdmItemCollection(disposableCollectionWrapper, metadataArtifactLoader.GetPaths(DataSpace.CSpace));
				_itemsCSpace = new Lazy<EdmItemCollection>(() => itemCollection2, isThreadSafe: true);
				_itemsOCSpace = new Lazy<DefaultObjectMappingItemCollection>(() => new DefaultObjectMappingItemCollection(itemCollection2, _itemsOSpace.Value), isThreadSafe: true);
			}
		}
		using (DisposableCollectionWrapper<XmlReader> disposableCollectionWrapper2 = new DisposableCollectionWrapper<XmlReader>(metadataArtifactLoader.CreateReaders(DataSpace.SSpace)))
		{
			if (disposableCollectionWrapper2.Any())
			{
				StoreItemCollection itemCollection = new StoreItemCollection(disposableCollectionWrapper2, metadataArtifactLoader.GetPaths(DataSpace.SSpace));
				_itemsSSpace = new Lazy<StoreItemCollection>(() => itemCollection, isThreadSafe: true);
			}
		}
		using DisposableCollectionWrapper<XmlReader> disposableCollectionWrapper3 = new DisposableCollectionWrapper<XmlReader>(metadataArtifactLoader.CreateReaders(DataSpace.CSSpace));
		if (disposableCollectionWrapper3.Any() && _itemsCSpace != null && _itemsSSpace != null)
		{
			StorageMappingItemCollection mapping = new StorageMappingItemCollection(_itemsCSpace.Value, _itemsSSpace.Value, disposableCollectionWrapper3, metadataArtifactLoader.GetPaths(DataSpace.CSSpace));
			_itemsCSSpace = new Lazy<StorageMappingItemCollection>(() => mapping, isThreadSafe: true);
		}
	}

	public virtual EntitySqlParser CreateEntitySqlParser()
	{
		return new EntitySqlParser(new ModelPerspective(this));
	}

	public virtual DbQueryCommandTree CreateQueryCommandTree(DbExpression query)
	{
		return new DbQueryCommandTree(this, DataSpace.CSpace, query);
	}

	public virtual ItemCollection GetItemCollection(DataSpace dataSpace)
	{
		return GetItemCollection(dataSpace, required: true);
	}

	[Obsolete("Construct MetadataWorkspace using constructor that accepts metadata loading delegates.")]
	public virtual void RegisterItemCollection(ItemCollection collection)
	{
		Check.NotNull(collection, "collection");
		try
		{
			switch (collection.DataSpace)
			{
			case DataSpace.CSpace:
			{
				EdmItemCollection edmCollection = (EdmItemCollection)collection;
				if (!SupportedEdmVersions.Contains(edmCollection.EdmVersion))
				{
					throw new InvalidOperationException(Strings.EdmVersionNotSupportedByRuntime(edmCollection.EdmVersion, Helper.GetCommaDelimitedString(from e in SupportedEdmVersions
						where e != 0.0
						select e.ToString(CultureInfo.InvariantCulture))));
				}
				CheckAndSetItemCollectionVersionInWorkSpace(collection);
				_itemsCSpace = new Lazy<EdmItemCollection>(() => edmCollection, isThreadSafe: true);
				if (_itemsOCSpace == null)
				{
					_itemsOCSpace = new Lazy<DefaultObjectMappingItemCollection>(() => new DefaultObjectMappingItemCollection(edmCollection, _itemsOSpace.Value));
				}
				break;
			}
			case DataSpace.SSpace:
				CheckAndSetItemCollectionVersionInWorkSpace(collection);
				_itemsSSpace = new Lazy<StoreItemCollection>(() => (StoreItemCollection)collection, isThreadSafe: true);
				break;
			case DataSpace.OSpace:
				_itemsOSpace = new Lazy<ObjectItemCollection>(() => (ObjectItemCollection)collection, isThreadSafe: true);
				if (_itemsOCSpace == null && _itemsCSpace != null)
				{
					_itemsOCSpace = new Lazy<DefaultObjectMappingItemCollection>(() => new DefaultObjectMappingItemCollection(_itemsCSpace.Value, _itemsOSpace.Value));
				}
				break;
			case DataSpace.CSSpace:
				CheckAndSetItemCollectionVersionInWorkSpace(collection);
				_itemsCSSpace = new Lazy<StorageMappingItemCollection>(() => (StorageMappingItemCollection)collection, isThreadSafe: true);
				break;
			default:
				_itemsOCSpace = new Lazy<DefaultObjectMappingItemCollection>(() => (DefaultObjectMappingItemCollection)collection, isThreadSafe: true);
				break;
			}
		}
		catch (InvalidCastException)
		{
			throw new MetadataException(Strings.InvalidCollectionForMapping(collection.DataSpace.ToString()));
		}
	}

	private T LoadAndCheckItemCollection<T>(Func<T> itemCollectionLoader) where T : ItemCollection
	{
		T val = itemCollectionLoader();
		if (val != null)
		{
			CheckAndSetItemCollectionVersionInWorkSpace(val);
		}
		return val;
	}

	private void CheckAndSetItemCollectionVersionInWorkSpace(ItemCollection itemCollectionToRegister)
	{
		double num = 0.0;
		string p = null;
		switch (itemCollectionToRegister.DataSpace)
		{
		case DataSpace.CSpace:
			num = ((EdmItemCollection)itemCollectionToRegister).EdmVersion;
			p = "EdmItemCollection";
			break;
		case DataSpace.SSpace:
			num = ((StoreItemCollection)itemCollectionToRegister).StoreSchemaVersion;
			p = "StoreItemCollection";
			break;
		case DataSpace.CSSpace:
			num = ((StorageMappingItemCollection)itemCollectionToRegister).MappingVersion;
			p = "StorageMappingItemCollection";
			break;
		}
		lock (_schemaVersionLock)
		{
			if (num != _schemaVersion && num != 0.0 && _schemaVersion != 0.0)
			{
				throw new MetadataException(Strings.DifferentSchemaVersionInCollection(p, num, _schemaVersion));
			}
			_schemaVersion = num;
		}
	}

	public virtual void LoadFromAssembly(Assembly assembly)
	{
		LoadFromAssembly(assembly, null);
	}

	public virtual void LoadFromAssembly(Assembly assembly, Action<string> logLoadMessage)
	{
		Check.NotNull(assembly, "assembly");
		ObjectItemCollection collection = (ObjectItemCollection)GetItemCollection(DataSpace.OSpace);
		ExplicitLoadFromAssembly(assembly, collection, logLoadMessage);
	}

	private void ExplicitLoadFromAssembly(Assembly assembly, ObjectItemCollection collection, Action<string> logLoadMessage)
	{
		if (!TryGetItemCollection(DataSpace.CSpace, out var collection2))
		{
			collection2 = null;
		}
		collection.ExplicitLoadFromAssembly(assembly, (EdmItemCollection)collection2, logLoadMessage);
	}

	private void ImplicitLoadFromAssembly(Assembly assembly, ObjectItemCollection collection)
	{
		if (!MetadataAssemblyHelper.ShouldFilterAssembly(assembly))
		{
			ExplicitLoadFromAssembly(assembly, collection, null);
		}
	}

	internal virtual void ImplicitLoadAssemblyForType(Type type, Assembly callingAssembly)
	{
		if (!TryGetItemCollection(DataSpace.OSpace, out var collection))
		{
			return;
		}
		ObjectItemCollection objectItemCollection = (ObjectItemCollection)collection;
		TryGetItemCollection(DataSpace.CSpace, out var collection2);
		EdmItemCollection edmItemCollection = (EdmItemCollection)collection2;
		if (!objectItemCollection.ImplicitLoadAssemblyForType(type, edmItemCollection) && null != callingAssembly)
		{
			if (ObjectItemAttributeAssemblyLoader.IsSchemaAttributePresent(callingAssembly) || _foundAssemblyWithAttribute || MetadataAssemblyHelper.GetNonSystemReferencedAssemblies(callingAssembly).Any(ObjectItemAttributeAssemblyLoader.IsSchemaAttributePresent))
			{
				_foundAssemblyWithAttribute = true;
				objectItemCollection.ImplicitLoadAllReferencedAssemblies(callingAssembly, edmItemCollection);
			}
			else
			{
				ImplicitLoadFromAssembly(callingAssembly, objectItemCollection);
			}
		}
	}

	internal virtual void ImplicitLoadFromEntityType(EntityType type, Assembly callingAssembly)
	{
		if (!TryGetMap(type, DataSpace.OCSpace, out var _))
		{
			ImplicitLoadAssemblyForType(typeof(IEntityWithKey), callingAssembly);
			if (!(GetItemCollection(DataSpace.OSpace) is ObjectItemCollection objectItemCollection) || !objectItemCollection.TryGetOSpaceType(type, out var _))
			{
				throw new InvalidOperationException(Strings.Mapping_Object_InvalidType(type.Identity));
			}
		}
	}

	public virtual T GetItem<T>(string identity, DataSpace dataSpace) where T : GlobalItem
	{
		return GetItemCollection(dataSpace, required: true).GetItem<T>(identity, ignoreCase: false);
	}

	public virtual bool TryGetItem<T>(string identity, DataSpace space, out T item) where T : GlobalItem
	{
		item = null;
		return GetItemCollection(space, required: false)?.TryGetItem<T>(identity, ignoreCase: false, out item) ?? false;
	}

	public virtual T GetItem<T>(string identity, bool ignoreCase, DataSpace dataSpace) where T : GlobalItem
	{
		return GetItemCollection(dataSpace, required: true).GetItem<T>(identity, ignoreCase);
	}

	public virtual bool TryGetItem<T>(string identity, bool ignoreCase, DataSpace dataSpace, out T item) where T : GlobalItem
	{
		item = null;
		return GetItemCollection(dataSpace, required: false)?.TryGetItem<T>(identity, ignoreCase, out item) ?? false;
	}

	public virtual ReadOnlyCollection<T> GetItems<T>(DataSpace dataSpace) where T : GlobalItem
	{
		return GetItemCollection(dataSpace, required: true).GetItems<T>();
	}

	public virtual EdmType GetType(string name, string namespaceName, DataSpace dataSpace)
	{
		return GetItemCollection(dataSpace, required: true).GetType(name, namespaceName, ignoreCase: false);
	}

	public virtual bool TryGetType(string name, string namespaceName, DataSpace dataSpace, out EdmType type)
	{
		type = null;
		return GetItemCollection(dataSpace, required: false)?.TryGetType(name, namespaceName, ignoreCase: false, out type) ?? false;
	}

	public virtual EdmType GetType(string name, string namespaceName, bool ignoreCase, DataSpace dataSpace)
	{
		return GetItemCollection(dataSpace, required: true).GetType(name, namespaceName, ignoreCase);
	}

	public virtual bool TryGetType(string name, string namespaceName, bool ignoreCase, DataSpace dataSpace, out EdmType type)
	{
		type = null;
		return GetItemCollection(dataSpace, required: false)?.TryGetType(name, namespaceName, ignoreCase, out type) ?? false;
	}

	public virtual EntityContainer GetEntityContainer(string name, DataSpace dataSpace)
	{
		return GetItemCollection(dataSpace, required: true).GetEntityContainer(name);
	}

	public virtual bool TryGetEntityContainer(string name, DataSpace dataSpace, out EntityContainer entityContainer)
	{
		entityContainer = null;
		Check.NotNull(name, "name");
		return GetItemCollection(dataSpace, required: false)?.TryGetEntityContainer(name, out entityContainer) ?? false;
	}

	public virtual EntityContainer GetEntityContainer(string name, bool ignoreCase, DataSpace dataSpace)
	{
		return GetItemCollection(dataSpace, required: true).GetEntityContainer(name, ignoreCase);
	}

	public virtual bool TryGetEntityContainer(string name, bool ignoreCase, DataSpace dataSpace, out EntityContainer entityContainer)
	{
		entityContainer = null;
		Check.NotNull(name, "name");
		return GetItemCollection(dataSpace, required: false)?.TryGetEntityContainer(name, ignoreCase, out entityContainer) ?? false;
	}

	public virtual ReadOnlyCollection<EdmFunction> GetFunctions(string name, string namespaceName, DataSpace dataSpace)
	{
		return GetFunctions(name, namespaceName, dataSpace, ignoreCase: false);
	}

	public virtual ReadOnlyCollection<EdmFunction> GetFunctions(string name, string namespaceName, DataSpace dataSpace, bool ignoreCase)
	{
		Check.NotEmpty(name, "name");
		Check.NotEmpty(namespaceName, "namespaceName");
		return GetItemCollection(dataSpace, required: true).GetFunctions(namespaceName + "." + name, ignoreCase);
	}

	internal virtual bool TryGetFunction(string name, string namespaceName, TypeUsage[] parameterTypes, bool ignoreCase, DataSpace dataSpace, out EdmFunction function)
	{
		function = null;
		Check.NotNull(name, "name");
		Check.NotNull(namespaceName, "namespaceName");
		return GetItemCollection(dataSpace, required: false)?.TryGetFunction(namespaceName + "." + name, parameterTypes, ignoreCase, out function) ?? false;
	}

	public virtual ReadOnlyCollection<PrimitiveType> GetPrimitiveTypes(DataSpace dataSpace)
	{
		return GetItemCollection(dataSpace, required: true).GetItems<PrimitiveType>();
	}

	public virtual ReadOnlyCollection<GlobalItem> GetItems(DataSpace dataSpace)
	{
		return GetItemCollection(dataSpace, required: true).GetItems<GlobalItem>();
	}

	internal virtual PrimitiveType GetMappedPrimitiveType(PrimitiveTypeKind primitiveTypeKind, DataSpace dataSpace)
	{
		return GetItemCollection(dataSpace, required: true).GetMappedPrimitiveType(primitiveTypeKind);
	}

	internal virtual bool TryGetMap(string typeIdentity, DataSpace typeSpace, bool ignoreCase, DataSpace mappingSpace, out MappingBase map)
	{
		map = null;
		ItemCollection itemCollection = GetItemCollection(mappingSpace, required: false);
		if (itemCollection != null)
		{
			return ((MappingItemCollection)itemCollection).TryGetMap(typeIdentity, typeSpace, ignoreCase, out map);
		}
		return false;
	}

	internal virtual MappingBase GetMap(string identity, DataSpace typeSpace, DataSpace dataSpace)
	{
		return ((MappingItemCollection)GetItemCollection(dataSpace, required: true)).GetMap(identity, typeSpace);
	}

	internal virtual MappingBase GetMap(GlobalItem item, DataSpace dataSpace)
	{
		return ((MappingItemCollection)GetItemCollection(dataSpace, required: true)).GetMap(item);
	}

	internal virtual bool TryGetMap(GlobalItem item, DataSpace dataSpace, out MappingBase map)
	{
		map = null;
		ItemCollection itemCollection = GetItemCollection(dataSpace, required: false);
		if (itemCollection != null)
		{
			return ((MappingItemCollection)itemCollection).TryGetMap(item, out map);
		}
		return false;
	}

	public virtual bool TryGetItemCollection(DataSpace dataSpace, out ItemCollection collection)
	{
		collection = GetItemCollection(dataSpace, required: false);
		return collection != null;
	}

	internal virtual ItemCollection GetItemCollection(DataSpace dataSpace, bool required)
	{
		ItemCollection itemCollection = dataSpace switch
		{
			DataSpace.CSpace => (_itemsCSpace == null) ? null : _itemsCSpace.Value, 
			DataSpace.OSpace => _itemsOSpace.Value, 
			DataSpace.OCSpace => (_itemsOCSpace == null) ? null : _itemsOCSpace.Value, 
			DataSpace.CSSpace => (_itemsCSSpace == null) ? null : _itemsCSSpace.Value, 
			DataSpace.SSpace => (_itemsSSpace == null) ? null : _itemsSSpace.Value, 
			_ => null, 
		};
		if (required && itemCollection == null)
		{
			throw new InvalidOperationException(Strings.NoCollectionForSpace(dataSpace.ToString()));
		}
		return itemCollection;
	}

	public virtual StructuralType GetObjectSpaceType(StructuralType edmSpaceType)
	{
		return GetObjectSpaceType<StructuralType>(edmSpaceType);
	}

	public virtual bool TryGetObjectSpaceType(StructuralType edmSpaceType, out StructuralType objectSpaceType)
	{
		return TryGetObjectSpaceType<StructuralType>(edmSpaceType, out objectSpaceType);
	}

	public virtual EnumType GetObjectSpaceType(EnumType edmSpaceType)
	{
		return GetObjectSpaceType<EnumType>(edmSpaceType);
	}

	public virtual bool TryGetObjectSpaceType(EnumType edmSpaceType, out EnumType objectSpaceType)
	{
		return TryGetObjectSpaceType<EnumType>(edmSpaceType, out objectSpaceType);
	}

	private T GetObjectSpaceType<T>(T edmSpaceType) where T : EdmType
	{
		if (!TryGetObjectSpaceType(edmSpaceType, out var objectSpaceType))
		{
			throw new ArgumentException(Strings.FailedToFindOSpaceTypeMapping(edmSpaceType.Identity));
		}
		return objectSpaceType;
	}

	private bool TryGetObjectSpaceType<T>(T edmSpaceType, out T objectSpaceType) where T : EdmType
	{
		if (edmSpaceType.DataSpace != DataSpace.CSpace)
		{
			throw new ArgumentException(Strings.ArgumentMustBeCSpaceType, "edmSpaceType");
		}
		objectSpaceType = null;
		if (TryGetMap(edmSpaceType, DataSpace.OCSpace, out var map) && map is ObjectTypeMapping objectTypeMapping)
		{
			objectSpaceType = (T)objectTypeMapping.ClrType;
		}
		return objectSpaceType != null;
	}

	public virtual StructuralType GetEdmSpaceType(StructuralType objectSpaceType)
	{
		return GetEdmSpaceType<StructuralType>(objectSpaceType);
	}

	public virtual bool TryGetEdmSpaceType(StructuralType objectSpaceType, out StructuralType edmSpaceType)
	{
		return TryGetEdmSpaceType<StructuralType>(objectSpaceType, out edmSpaceType);
	}

	public virtual EnumType GetEdmSpaceType(EnumType objectSpaceType)
	{
		return GetEdmSpaceType<EnumType>(objectSpaceType);
	}

	public virtual bool TryGetEdmSpaceType(EnumType objectSpaceType, out EnumType edmSpaceType)
	{
		return TryGetEdmSpaceType<EnumType>(objectSpaceType, out edmSpaceType);
	}

	private T GetEdmSpaceType<T>(T objectSpaceType) where T : EdmType
	{
		if (!TryGetEdmSpaceType(objectSpaceType, out var edmSpaceType))
		{
			throw new ArgumentException(Strings.FailedToFindCSpaceTypeMapping(objectSpaceType.Identity));
		}
		return edmSpaceType;
	}

	private bool TryGetEdmSpaceType<T>(T objectSpaceType, out T edmSpaceType) where T : EdmType
	{
		if (objectSpaceType.DataSpace != 0)
		{
			throw new ArgumentException(Strings.ArgumentMustBeOSpaceType, "objectSpaceType");
		}
		edmSpaceType = null;
		if (TryGetMap(objectSpaceType, DataSpace.OCSpace, out var map) && map is ObjectTypeMapping objectTypeMapping)
		{
			edmSpaceType = (T)objectTypeMapping.EdmType;
		}
		return edmSpaceType != null;
	}

	internal virtual DbQueryCommandTree GetCqtView(EntitySetBase extent)
	{
		return GetGeneratedView(extent).GetCommandTree();
	}

	internal virtual GeneratedView GetGeneratedView(EntitySetBase extent)
	{
		return ((StorageMappingItemCollection)GetItemCollection(DataSpace.CSSpace, required: true)).GetGeneratedView(extent, this);
	}

	internal virtual bool TryGetGeneratedViewOfType(EntitySetBase extent, EntityTypeBase type, bool includeSubtypes, out GeneratedView generatedView)
	{
		return ((StorageMappingItemCollection)GetItemCollection(DataSpace.CSSpace, required: true)).TryGetGeneratedViewOfType(extent, type, includeSubtypes, out generatedView);
	}

	internal virtual DbLambda GetGeneratedFunctionDefinition(EdmFunction function)
	{
		return ((EdmItemCollection)GetItemCollection(DataSpace.CSpace, required: true)).GetGeneratedFunctionDefinition(function);
	}

	internal virtual bool TryGetFunctionImportMapping(EdmFunction functionImport, out FunctionImportMapping targetFunctionMapping)
	{
		foreach (EntityContainerMapping item in GetItems<EntityContainerMapping>(DataSpace.CSSpace))
		{
			if (item.TryGetFunctionImportMapping(functionImport, out targetFunctionMapping))
			{
				return true;
			}
		}
		targetFunctionMapping = null;
		return false;
	}

	internal virtual ViewLoader GetUpdateViewLoader()
	{
		if (_itemsCSSpace == null || _itemsCSSpace.Value == null)
		{
			return null;
		}
		return _itemsCSSpace.Value.GetUpdateViewLoader();
	}

	internal virtual TypeUsage GetOSpaceTypeUsage(TypeUsage edmSpaceTypeUsage)
	{
		EdmType edmType = null;
		edmType = ((!Helper.IsPrimitiveType(edmSpaceTypeUsage.EdmType)) ? ((ObjectTypeMapping)((DefaultObjectMappingItemCollection)GetItemCollection(DataSpace.OCSpace, required: true)).GetMap(edmSpaceTypeUsage.EdmType)).ClrType : GetItemCollection(DataSpace.OSpace, required: true).GetMappedPrimitiveType(((PrimitiveType)edmSpaceTypeUsage.EdmType).PrimitiveTypeKind));
		return TypeUsage.Create(edmType, edmSpaceTypeUsage.Facets);
	}

	internal virtual bool IsItemCollectionAlreadyRegistered(DataSpace dataSpace)
	{
		ItemCollection collection;
		return TryGetItemCollection(dataSpace, out collection);
	}

	internal virtual bool IsMetadataWorkspaceCSCompatible(MetadataWorkspace other)
	{
		return GetItemCollection(DataSpace.CSSpace, required: false).MetadataEquals(other.GetItemCollection(DataSpace.CSSpace, required: false));
	}

	public static void ClearCache()
	{
		MetadataCache.Instance.Clear();
		using LockedAssemblyCache lockedAssemblyCache = AssemblyCache.AcquireLockedAssemblyCache();
		lockedAssemblyCache.Clear();
	}

	internal static TypeUsage GetCanonicalModelTypeUsage(PrimitiveTypeKind primitiveTypeKind)
	{
		return EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(primitiveTypeKind);
	}

	internal static PrimitiveType GetModelPrimitiveType(PrimitiveTypeKind primitiveTypeKind)
	{
		return EdmProviderManifest.Instance.GetPrimitiveType(primitiveTypeKind);
	}

	[Obsolete("Use MetadataWorkspace.GetRelevantMembersForUpdate(EntitySetBase, EntityTypeBase, bool) instead")]
	public virtual IEnumerable<EdmMember> GetRequiredOriginalValueMembers(EntitySetBase entitySet, EntityTypeBase entityType)
	{
		return GetInterestingMembers(entitySet, entityType, StorageMappingItemCollection.InterestingMembersKind.RequiredOriginalValueMembers);
	}

	public virtual ReadOnlyCollection<EdmMember> GetRelevantMembersForUpdate(EntitySetBase entitySet, EntityTypeBase entityType, bool partialUpdateSupported)
	{
		return GetInterestingMembers(entitySet, entityType, (!partialUpdateSupported) ? StorageMappingItemCollection.InterestingMembersKind.FullUpdate : StorageMappingItemCollection.InterestingMembersKind.PartialUpdate);
	}

	private ReadOnlyCollection<EdmMember> GetInterestingMembers(EntitySetBase entitySet, EntityTypeBase entityType, StorageMappingItemCollection.InterestingMembersKind interestingMembersKind)
	{
		AssociationSet associationSet = entitySet as AssociationSet;
		if (entitySet.EntityContainer.DataSpace != DataSpace.CSpace)
		{
			throw new ArgumentException(Strings.EntitySetNotInCSPace(entitySet.Name));
		}
		if (!entitySet.ElementType.IsAssignableFrom(entityType))
		{
			if (associationSet != null)
			{
				throw new ArgumentException(Strings.TypeNotInAssociationSet(entityType.FullName, entitySet.ElementType.FullName, entitySet.Name));
			}
			throw new ArgumentException(Strings.TypeNotInEntitySet(entityType.FullName, entitySet.ElementType.FullName, entitySet.Name));
		}
		return ((StorageMappingItemCollection)GetItemCollection(DataSpace.CSSpace, required: true)).GetInterestingMembers(entitySet, entityType, interestingMembersKind);
	}

	internal virtual QueryCacheManager GetQueryCacheManager()
	{
		return _itemsSSpace.Value.QueryCacheManager;
	}

	internal bool TryDetermineCSpaceModelType<T>(out EdmType modelEdmType)
	{
		return TryDetermineCSpaceModelType(typeof(T), out modelEdmType);
	}

	internal virtual bool TryDetermineCSpaceModelType(Type type, out EdmType modelEdmType)
	{
		Type nonNullableType = TypeSystem.GetNonNullableType(type);
		ImplicitLoadAssemblyForType(nonNullableType, Assembly.GetCallingAssembly());
		if (((ObjectItemCollection)GetItemCollection(DataSpace.OSpace)).TryGetItem<EdmType>(nonNullableType.FullNameWithNesting(), out var item) && TryGetMap(item, DataSpace.OCSpace, out var map))
		{
			ObjectTypeMapping objectTypeMapping = (ObjectTypeMapping)map;
			modelEdmType = objectTypeMapping.EdmType;
			return true;
		}
		modelEdmType = null;
		return false;
	}
}
