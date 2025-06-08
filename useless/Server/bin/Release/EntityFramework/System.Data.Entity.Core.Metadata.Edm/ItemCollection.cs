using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Threading;

namespace System.Data.Entity.Core.Metadata.Edm;

public abstract class ItemCollection : ReadOnlyMetadataCollection<GlobalItem>
{
	private readonly DataSpace _space;

	private Dictionary<string, ReadOnlyCollection<EdmFunction>> _functionLookUpTable;

	private Memoizer<Type, ICollection> _itemsCache;

	private int _itemCount;

	public DataSpace DataSpace => _space;

	internal Dictionary<string, ReadOnlyCollection<EdmFunction>> FunctionLookUpTable
	{
		get
		{
			if (_functionLookUpTable == null)
			{
				Dictionary<string, ReadOnlyCollection<EdmFunction>> value = PopulateFunctionLookUpTable(this);
				Interlocked.CompareExchange(ref _functionLookUpTable, value, null);
			}
			return _functionLookUpTable;
		}
	}

	internal ItemCollection()
	{
	}

	internal ItemCollection(DataSpace dataspace)
		: base(new MetadataCollection<GlobalItem>())
	{
		_space = dataspace;
	}

	internal void AddInternal(GlobalItem item)
	{
		base.Source.Add(item);
	}

	internal void AddRange(List<GlobalItem> items)
	{
		base.Source.AddRange(items);
	}

	public T GetItem<T>(string identity) where T : GlobalItem
	{
		return GetItem<T>(identity, ignoreCase: false);
	}

	public bool TryGetItem<T>(string identity, out T item) where T : GlobalItem
	{
		return TryGetItem<T>(identity, ignoreCase: false, out item);
	}

	public bool TryGetItem<T>(string identity, bool ignoreCase, out T item) where T : GlobalItem
	{
		GlobalItem item2 = null;
		TryGetValue(identity, ignoreCase, out item2);
		item = item2 as T;
		return item != null;
	}

	public T GetItem<T>(string identity, bool ignoreCase) where T : GlobalItem
	{
		if (TryGetItem<T>(identity, ignoreCase, out var item))
		{
			return item;
		}
		throw new ArgumentException(Strings.ItemInvalidIdentity(identity), "identity");
	}

	public virtual ReadOnlyCollection<T> GetItems<T>() where T : GlobalItem
	{
		Memoizer<Type, ICollection> itemsCache = _itemsCache;
		if (_itemsCache == null || _itemCount != base.Count)
		{
			Memoizer<Type, ICollection> value = new Memoizer<Type, ICollection>(InternalGetItems, null);
			Interlocked.CompareExchange(ref _itemsCache, value, itemsCache);
			_itemCount = base.Count;
		}
		return _itemsCache.Evaluate(typeof(T)) as ReadOnlyCollection<T>;
	}

	internal ICollection InternalGetItems(Type type)
	{
		return typeof(ItemCollection).GetOnlyDeclaredMethod("GenericGetItems").MakeGenericMethod(type).Invoke(null, new object[1] { this }) as ICollection;
	}

	private static ReadOnlyCollection<TItem> GenericGetItems<TItem>(ItemCollection collection) where TItem : GlobalItem
	{
		List<TItem> list = new List<TItem>();
		foreach (GlobalItem item2 in collection)
		{
			if (item2 is TItem item)
			{
				list.Add(item);
			}
		}
		return new ReadOnlyCollection<TItem>(list);
	}

	public EdmType GetType(string name, string namespaceName)
	{
		return GetType(name, namespaceName, ignoreCase: false);
	}

	public bool TryGetType(string name, string namespaceName, out EdmType type)
	{
		return TryGetType(name, namespaceName, ignoreCase: false, out type);
	}

	public EdmType GetType(string name, string namespaceName, bool ignoreCase)
	{
		Check.NotNull(name, "name");
		Check.NotNull(namespaceName, "namespaceName");
		return GetItem<EdmType>(EdmType.CreateEdmTypeIdentity(namespaceName, name), ignoreCase);
	}

	public bool TryGetType(string name, string namespaceName, bool ignoreCase, out EdmType type)
	{
		Check.NotNull(name, "name");
		Check.NotNull(namespaceName, "namespaceName");
		GlobalItem item = null;
		TryGetValue(EdmType.CreateEdmTypeIdentity(namespaceName, name), ignoreCase, out item);
		type = item as EdmType;
		return type != null;
	}

	public ReadOnlyCollection<EdmFunction> GetFunctions(string functionName)
	{
		return GetFunctions(functionName, ignoreCase: false);
	}

	public ReadOnlyCollection<EdmFunction> GetFunctions(string functionName, bool ignoreCase)
	{
		return GetFunctions(FunctionLookUpTable, functionName, ignoreCase);
	}

	protected static ReadOnlyCollection<EdmFunction> GetFunctions(Dictionary<string, ReadOnlyCollection<EdmFunction>> functionCollection, string functionName, bool ignoreCase)
	{
		if (functionCollection.TryGetValue(functionName, out var value))
		{
			if (ignoreCase)
			{
				return value;
			}
			return GetCaseSensitiveFunctions(value, functionName);
		}
		return Helper.EmptyEdmFunctionReadOnlyCollection;
	}

	internal static ReadOnlyCollection<EdmFunction> GetCaseSensitiveFunctions(ReadOnlyCollection<EdmFunction> functionOverloads, string functionName)
	{
		List<EdmFunction> list = new List<EdmFunction>(functionOverloads.Count);
		for (int i = 0; i < functionOverloads.Count; i++)
		{
			if (functionOverloads[i].FullName == functionName)
			{
				list.Add(functionOverloads[i]);
			}
		}
		if (list.Count != functionOverloads.Count)
		{
			functionOverloads = new ReadOnlyCollection<EdmFunction>(list);
		}
		return functionOverloads;
	}

	internal bool TryGetFunction(string functionName, TypeUsage[] parameterTypes, bool ignoreCase, out EdmFunction function)
	{
		Check.NotNull(functionName, "functionName");
		Check.NotNull(parameterTypes, "parameterTypes");
		string identity = EdmFunction.BuildIdentity(functionName, parameterTypes);
		GlobalItem item = null;
		function = null;
		if (TryGetValue(identity, ignoreCase, out item) && Helper.IsEdmFunction(item))
		{
			function = (EdmFunction)item;
			return true;
		}
		return false;
	}

	public EntityContainer GetEntityContainer(string name)
	{
		Check.NotNull(name, "name");
		return GetEntityContainer(name, ignoreCase: false);
	}

	public bool TryGetEntityContainer(string name, out EntityContainer entityContainer)
	{
		Check.NotNull(name, "name");
		return TryGetEntityContainer(name, ignoreCase: false, out entityContainer);
	}

	public EntityContainer GetEntityContainer(string name, bool ignoreCase)
	{
		if (GetValue(name, ignoreCase) is EntityContainer result)
		{
			return result;
		}
		throw new ArgumentException(Strings.ItemInvalidIdentity(name), "name");
	}

	public bool TryGetEntityContainer(string name, bool ignoreCase, out EntityContainer entityContainer)
	{
		Check.NotNull(name, "name");
		GlobalItem item = null;
		if (TryGetValue(name, ignoreCase, out item) && Helper.IsEntityContainer(item))
		{
			entityContainer = (EntityContainer)item;
			return true;
		}
		entityContainer = null;
		return false;
	}

	internal virtual PrimitiveType GetMappedPrimitiveType(PrimitiveTypeKind primitiveTypeKind)
	{
		throw Error.NotSupported();
	}

	internal virtual bool MetadataEquals(ItemCollection other)
	{
		return this == other;
	}

	private static Dictionary<string, ReadOnlyCollection<EdmFunction>> PopulateFunctionLookUpTable(ItemCollection itemCollection)
	{
		Dictionary<string, List<EdmFunction>> dictionary = new Dictionary<string, List<EdmFunction>>(StringComparer.OrdinalIgnoreCase);
		foreach (EdmFunction item in itemCollection.GetItems<EdmFunction>())
		{
			if (!dictionary.TryGetValue(item.FullName, out var value))
			{
				value = new List<EdmFunction>();
				dictionary[item.FullName] = value;
			}
			value.Add(item);
		}
		Dictionary<string, ReadOnlyCollection<EdmFunction>> dictionary2 = new Dictionary<string, ReadOnlyCollection<EdmFunction>>(StringComparer.OrdinalIgnoreCase);
		foreach (List<EdmFunction> value2 in dictionary.Values)
		{
			dictionary2.Add(value2[0].FullName, new ReadOnlyCollection<EdmFunction>(value2.ToArray()));
		}
		return dictionary2;
	}
}
