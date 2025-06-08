using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.Core.Metadata.Edm;

internal class ObjectItemLoadingSessionData
{
	private Func<Assembly, ObjectItemLoadingSessionData, ObjectItemAssemblyLoader> _loaderFactory;

	private readonly Dictionary<string, EdmType> _typesInLoading;

	private readonly LoadMessageLogger _loadMessageLogger;

	private readonly List<EdmItemError> _errors;

	private readonly Dictionary<Assembly, MutableAssemblyCacheEntry> _listOfAssembliesLoaded = new Dictionary<Assembly, MutableAssemblyCacheEntry>();

	private readonly KnownAssembliesSet _knownAssemblies;

	private readonly LockedAssemblyCache _lockedAssemblyCache;

	private readonly HashSet<ObjectItemAssemblyLoader> _loadersThatNeedLevel1PostSessionProcessing = new HashSet<ObjectItemAssemblyLoader>();

	private readonly HashSet<ObjectItemAssemblyLoader> _loadersThatNeedLevel2PostSessionProcessing = new HashSet<ObjectItemAssemblyLoader>();

	private readonly EdmItemCollection _edmItemCollection;

	private Dictionary<string, KeyValuePair<EdmType, int>> _conventionCSpaceTypeNames;

	private readonly Dictionary<EdmType, EdmType> _cspaceToOspace;

	private readonly object _originalLoaderCookie;

	internal virtual Dictionary<string, EdmType> TypesInLoading => _typesInLoading;

	internal Dictionary<Assembly, MutableAssemblyCacheEntry> AssembliesLoaded => _listOfAssembliesLoaded;

	internal virtual List<EdmItemError> EdmItemErrors => _errors;

	internal KnownAssembliesSet KnownAssemblies => _knownAssemblies;

	internal LockedAssemblyCache LockedAssemblyCache => _lockedAssemblyCache;

	internal EdmItemCollection EdmItemCollection => _edmItemCollection;

	internal virtual Dictionary<EdmType, EdmType> CspaceToOspace => _cspaceToOspace;

	internal bool ConventionBasedRelationshipsAreLoaded { get; set; }

	internal virtual LoadMessageLogger LoadMessageLogger => _loadMessageLogger;

	internal Dictionary<string, KeyValuePair<EdmType, int>> ConventionCSpaceTypeNames
	{
		get
		{
			if (_edmItemCollection != null && _conventionCSpaceTypeNames == null)
			{
				_conventionCSpaceTypeNames = new Dictionary<string, KeyValuePair<EdmType, int>>();
				foreach (EdmType item in _edmItemCollection.GetItems<EdmType>())
				{
					if ((item is StructuralType && item.BuiltInTypeKind != BuiltInTypeKind.AssociationType) || Helper.IsEnumType(item))
					{
						if (_conventionCSpaceTypeNames.TryGetValue(item.Name, out var value))
						{
							_conventionCSpaceTypeNames[item.Name] = new KeyValuePair<EdmType, int>(value.Key, value.Value + 1);
							continue;
						}
						value = new KeyValuePair<EdmType, int>(item, 1);
						_conventionCSpaceTypeNames.Add(item.Name, value);
					}
				}
			}
			return _conventionCSpaceTypeNames;
		}
	}

	internal Func<Assembly, ObjectItemLoadingSessionData, ObjectItemAssemblyLoader> ObjectItemAssemblyLoaderFactory
	{
		get
		{
			return _loaderFactory;
		}
		set
		{
			if (_loaderFactory != value)
			{
				_loaderFactory = value;
			}
		}
	}

	internal object LoaderCookie
	{
		get
		{
			if (_originalLoaderCookie != null)
			{
				return _originalLoaderCookie;
			}
			return _loaderFactory;
		}
	}

	internal ObjectItemLoadingSessionData()
	{
	}

	internal ObjectItemLoadingSessionData(KnownAssembliesSet knownAssemblies, LockedAssemblyCache lockedAssemblyCache, EdmItemCollection edmItemCollection, Action<string> logLoadMessage, object loaderCookie)
	{
		_typesInLoading = new Dictionary<string, EdmType>(StringComparer.Ordinal);
		_errors = new List<EdmItemError>();
		_knownAssemblies = knownAssemblies;
		_lockedAssemblyCache = lockedAssemblyCache;
		_edmItemCollection = edmItemCollection;
		_loadMessageLogger = new LoadMessageLogger(logLoadMessage);
		_cspaceToOspace = new Dictionary<EdmType, EdmType>();
		_loaderFactory = (Func<Assembly, ObjectItemLoadingSessionData, ObjectItemAssemblyLoader>)loaderCookie;
		_originalLoaderCookie = loaderCookie;
		if (!(_loaderFactory == new Func<Assembly, ObjectItemLoadingSessionData, ObjectItemAssemblyLoader>(ObjectItemConventionAssemblyLoader.Create)) || _edmItemCollection == null)
		{
			return;
		}
		foreach (KnownAssemblyEntry entry in _knownAssemblies.GetEntries(_loaderFactory, edmItemCollection))
		{
			foreach (EdmType item in entry.CacheEntry.TypesInAssembly.OfType<EdmType>())
			{
				if (Helper.IsEntityType(item))
				{
					ClrEntityType clrEntityType = (ClrEntityType)item;
					_cspaceToOspace.Add(_edmItemCollection.GetItem<StructuralType>(clrEntityType.CSpaceTypeName), clrEntityType);
				}
				else if (Helper.IsComplexType(item))
				{
					ClrComplexType clrComplexType = (ClrComplexType)item;
					_cspaceToOspace.Add(_edmItemCollection.GetItem<StructuralType>(clrComplexType.CSpaceTypeName), clrComplexType);
				}
				else if (Helper.IsEnumType(item))
				{
					ClrEnumType clrEnumType = (ClrEnumType)item;
					_cspaceToOspace.Add(_edmItemCollection.GetItem<EnumType>(clrEnumType.CSpaceTypeName), clrEnumType);
				}
				else
				{
					_cspaceToOspace.Add(_edmItemCollection.GetItem<StructuralType>(item.FullName), item);
				}
			}
		}
	}

	internal void RegisterForLevel1PostSessionProcessing(ObjectItemAssemblyLoader loader)
	{
		_loadersThatNeedLevel1PostSessionProcessing.Add(loader);
	}

	internal void RegisterForLevel2PostSessionProcessing(ObjectItemAssemblyLoader loader)
	{
		_loadersThatNeedLevel2PostSessionProcessing.Add(loader);
	}

	internal void CompleteSession()
	{
		foreach (ObjectItemAssemblyLoader item in _loadersThatNeedLevel1PostSessionProcessing)
		{
			item.OnLevel1SessionProcessing();
		}
		foreach (ObjectItemAssemblyLoader item2 in _loadersThatNeedLevel2PostSessionProcessing)
		{
			item2.OnLevel2SessionProcessing();
		}
	}
}
