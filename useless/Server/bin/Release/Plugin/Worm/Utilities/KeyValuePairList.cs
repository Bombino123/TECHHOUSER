using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Utilities;

[ComVisible(true)]
public class KeyValuePairList<TKey, TValue> : List<KeyValuePair<TKey, TValue>>
{
	public List<TKey> Keys
	{
		get
		{
			List<TKey> list = new List<TKey>();
			using Enumerator enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				list.Add(enumerator.Current.Key);
			}
			return list;
		}
	}

	public List<TValue> Values
	{
		get
		{
			List<TValue> list = new List<TValue>();
			using Enumerator enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				list.Add(enumerator.Current.Value);
			}
			return list;
		}
	}

	public KeyValuePairList()
	{
	}

	public KeyValuePairList(List<KeyValuePair<TKey, TValue>> collection)
		: base((IEnumerable<KeyValuePair<TKey, TValue>>)collection)
	{
	}

	public bool ContainsKey(TKey key)
	{
		return IndexOfKey(key) != -1;
	}

	public int IndexOfKey(TKey key)
	{
		for (int i = 0; i < base.Count; i++)
		{
			if (base[i].Key.Equals(key))
			{
				return i;
			}
		}
		return -1;
	}

	public TValue ValueOf(TKey key)
	{
		for (int i = 0; i < base.Count; i++)
		{
			if (base[i].Key.Equals(key))
			{
				return base[i].Value;
			}
		}
		return default(TValue);
	}

	public void Add(TKey key, TValue value)
	{
		Add(new KeyValuePair<TKey, TValue>(key, value));
	}

	public new KeyValuePairList<TKey, TValue> GetRange(int index, int count)
	{
		return new KeyValuePairList<TKey, TValue>(base.GetRange(index, count));
	}

	public new void Sort()
	{
		Sort(Comparer<TKey>.Default);
	}

	public void Sort(ListSortDirection sortDirection)
	{
		Sort(Comparer<TKey>.Default, sortDirection);
	}

	public void Sort(IComparer<TKey> comparer, ListSortDirection sortDirection)
	{
		if (sortDirection == ListSortDirection.Ascending)
		{
			Sort(comparer);
		}
		else
		{
			Sort(new ReverseComparer<TKey>(comparer));
		}
	}

	public void Sort(IComparer<TKey> comparer)
	{
		Sort((KeyValuePair<TKey, TValue> a, KeyValuePair<TKey, TValue> b) => comparer.Compare(a.Key, b.Key));
	}
}
