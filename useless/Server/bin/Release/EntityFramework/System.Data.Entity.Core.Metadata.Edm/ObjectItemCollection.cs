using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Metadata.Edm.Provider;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.Core.Metadata.Edm;

public class ObjectItemCollection : ItemCollection
{
	private readonly CacheForPrimitiveTypes _primitiveTypeMaps = new CacheForPrimitiveTypes();

	private KnownAssembliesSet _knownAssemblies = new KnownAssembliesSet();

	private readonly Dictionary<string, EdmType> _ocMapping = new Dictionary<string, EdmType>();

	private object _loaderCookie;

	private readonly object _loadAssemblyLock = new object();

	internal bool OSpaceTypesLoaded { get; set; }

	internal object LoadAssemblyLock => _loadAssemblyLock;

	public ObjectItemCollection()
		: this(null)
	{
	}

	internal ObjectItemCollection(KnownAssembliesSet knownAssembliesSet = null)
		: base(DataSpace.OSpace)
	{
		_knownAssemblies = knownAssembliesSet ?? new KnownAssembliesSet();
		foreach (PrimitiveType storeType in ClrProviderManifest.Instance.GetStoreTypes())
		{
			AddInternal(storeType);
			_primitiveTypeMaps.Add(storeType);
		}
	}

	internal void ImplicitLoadAllReferencedAssemblies(Assembly assembly, EdmItemCollection edmItemCollection)
	{
		if (!MetadataAssemblyHelper.ShouldFilterAssembly(assembly))
		{
			LoadAssemblyFromCache(assembly, loadReferencedAssemblies: true, edmItemCollection, null);
		}
	}

	public void LoadFromAssembly(Assembly assembly)
	{
		ExplicitLoadFromAssembly(assembly, null, null);
	}

	public void LoadFromAssembly(Assembly assembly, EdmItemCollection edmItemCollection, Action<string> logLoadMessage)
	{
		Check.NotNull(assembly, "assembly");
		Check.NotNull(edmItemCollection, "edmItemCollection");
		Check.NotNull(logLoadMessage, "logLoadMessage");
		ExplicitLoadFromAssembly(assembly, edmItemCollection, logLoadMessage);
	}

	public void LoadFromAssembly(Assembly assembly, EdmItemCollection edmItemCollection)
	{
		Check.NotNull(assembly, "assembly");
		Check.NotNull(edmItemCollection, "edmItemCollection");
		ExplicitLoadFromAssembly(assembly, edmItemCollection, null);
	}

	internal void ExplicitLoadFromAssembly(Assembly assembly, EdmItemCollection edmItemCollection, Action<string> logLoadMessage)
	{
		LoadAssemblyFromCache(assembly, loadReferencedAssemblies: false, edmItemCollection, logLoadMessage);
	}

	internal bool ImplicitLoadAssemblyForType(Type type, EdmItemCollection edmItemCollection)
	{
		bool flag = false;
		if (!MetadataAssemblyHelper.ShouldFilterAssembly(type.Assembly()))
		{
			flag = LoadAssemblyFromCache(type.Assembly(), loadReferencedAssemblies: false, edmItemCollection, null);
		}
		if (type.IsGenericType())
		{
			Type[] genericArguments = type.GetGenericArguments();
			foreach (Type type2 in genericArguments)
			{
				flag |= ImplicitLoadAssemblyForType(type2, edmItemCollection);
			}
		}
		return flag;
	}

	internal AssociationType GetRelationshipType(string relationshipName)
	{
		if (TryGetItem<AssociationType>(relationshipName, out var item))
		{
			return item;
		}
		return null;
	}

	private bool LoadAssemblyFromCache(Assembly assembly, bool loadReferencedAssemblies, EdmItemCollection edmItemCollection, Action<string> logLoadMessage)
	{
		if (OSpaceTypesLoaded)
		{
			return true;
		}
		if (edmItemCollection != null)
		{
			ReadOnlyCollection<EntityContainer> items = edmItemCollection.GetItems<EntityContainer>();
			if (items.Any() && items.All((EntityContainer c) => c.Annotations.Any((MetadataProperty a) => a.Name == "http://schemas.microsoft.com/ado/2013/11/edm/customannotation:UseClrTypes" && ((string)a.Value).ToUpperInvariant() == "TRUE")))
			{
				lock (LoadAssemblyLock)
				{
					if (!OSpaceTypesLoaded)
					{
						new CodeFirstOSpaceLoader().LoadTypes(edmItemCollection, this);
					}
					return true;
				}
			}
		}
		if (_knownAssemblies.TryGetKnownAssembly(assembly, _loaderCookie, edmItemCollection, out var entry))
		{
			if (!loadReferencedAssemblies)
			{
				return entry.CacheEntry.TypesInAssembly.Count != 0;
			}
			if (entry.ReferencedAssembliesAreLoaded)
			{
				return true;
			}
		}
		lock (LoadAssemblyLock)
		{
			if (_knownAssemblies.TryGetKnownAssembly(assembly, _loaderCookie, edmItemCollection, out entry) && (!loadReferencedAssemblies || entry.ReferencedAssembliesAreLoaded))
			{
				return true;
			}
			KnownAssembliesSet knownAssemblies = new KnownAssembliesSet(_knownAssemblies);
			AssemblyCache.LoadAssembly(assembly, loadReferencedAssemblies, knownAssemblies, edmItemCollection, logLoadMessage, ref _loaderCookie, out var typesInLoading, out var errors);
			if (errors.Count != 0)
			{
				throw EntityUtil.InvalidSchemaEncountered(Helper.CombineErrorMessage(errors));
			}
			if (typesInLoading.Count != 0)
			{
				AddLoadedTypes(typesInLoading);
			}
			_knownAssemblies = knownAssemblies;
			return typesInLoading.Count != 0;
		}
	}

	internal virtual void AddLoadedTypes(Dictionary<string, EdmType> typesInLoading)
	{
		List<GlobalItem> list = new List<GlobalItem>();
		foreach (EdmType value in typesInLoading.Values)
		{
			list.Add(value);
			string text = "";
			try
			{
				if (Helper.IsEntityType(value))
				{
					text = ((ClrEntityType)value).CSpaceTypeName;
					_ocMapping.Add(text, value);
				}
				else if (Helper.IsComplexType(value))
				{
					text = ((ClrComplexType)value).CSpaceTypeName;
					_ocMapping.Add(text, value);
				}
				else if (Helper.IsEnumType(value))
				{
					text = ((ClrEnumType)value).CSpaceTypeName;
					_ocMapping.Add(text, value);
				}
			}
			catch (ArgumentException innerException)
			{
				throw new MappingException(Strings.Mapping_CannotMapCLRTypeMultipleTimes(text), innerException);
			}
		}
		AddRange(list);
	}

	public IEnumerable<PrimitiveType> GetPrimitiveTypes()
	{
		return _primitiveTypeMaps.GetTypes();
	}

	public Type GetClrType(StructuralType objectSpaceType)
	{
		return GetClrType((EdmType)objectSpaceType);
	}

	public bool TryGetClrType(StructuralType objectSpaceType, out Type clrType)
	{
		return TryGetClrType((EdmType)objectSpaceType, out clrType);
	}

	public Type GetClrType(EnumType objectSpaceType)
	{
		return GetClrType((EdmType)objectSpaceType);
	}

	public bool TryGetClrType(EnumType objectSpaceType, out Type clrType)
	{
		return TryGetClrType((EdmType)objectSpaceType, out clrType);
	}

	private static Type GetClrType(EdmType objectSpaceType)
	{
		if (!TryGetClrType(objectSpaceType, out var clrType))
		{
			throw new ArgumentException(Strings.FailedToFindClrTypeMapping(objectSpaceType.Identity));
		}
		return clrType;
	}

	private static bool TryGetClrType(EdmType objectSpaceType, out Type clrType)
	{
		if (objectSpaceType.DataSpace != 0)
		{
			throw new ArgumentException(Strings.ArgumentMustBeOSpaceType, "objectSpaceType");
		}
		clrType = null;
		if (Helper.IsEntityType(objectSpaceType) || Helper.IsComplexType(objectSpaceType) || Helper.IsEnumType(objectSpaceType))
		{
			clrType = objectSpaceType.ClrType;
		}
		return clrType != null;
	}

	internal override PrimitiveType GetMappedPrimitiveType(PrimitiveTypeKind modelType)
	{
		if (Helper.IsGeometricTypeKind(modelType))
		{
			modelType = PrimitiveTypeKind.Geometry;
		}
		else if (Helper.IsGeographicTypeKind(modelType))
		{
			modelType = PrimitiveTypeKind.Geography;
		}
		PrimitiveType type = null;
		_primitiveTypeMaps.TryGetType(modelType, null, out type);
		return type;
	}

	internal bool TryGetOSpaceType(EdmType cspaceType, out EdmType edmType)
	{
		if (Helper.IsEntityType(cspaceType) || Helper.IsComplexType(cspaceType) || Helper.IsEnumType(cspaceType))
		{
			return _ocMapping.TryGetValue(cspaceType.Identity, out edmType);
		}
		return TryGetItem<EdmType>(cspaceType.Identity, out edmType);
	}

	internal static string TryGetMappingCSpaceTypeIdentity(EdmType edmType)
	{
		if (Helper.IsEntityType(edmType))
		{
			return ((ClrEntityType)edmType).CSpaceTypeName;
		}
		if (Helper.IsComplexType(edmType))
		{
			return ((ClrComplexType)edmType).CSpaceTypeName;
		}
		if (Helper.IsEnumType(edmType))
		{
			return ((ClrEnumType)edmType).CSpaceTypeName;
		}
		return edmType.Identity;
	}

	public override ReadOnlyCollection<T> GetItems<T>()
	{
		return InternalGetItems(typeof(T)) as ReadOnlyCollection<T>;
	}
}
