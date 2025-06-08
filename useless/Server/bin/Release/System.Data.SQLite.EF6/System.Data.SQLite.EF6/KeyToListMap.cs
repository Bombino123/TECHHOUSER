using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime;
using System.Text;

namespace System.Data.SQLite.EF6;

internal sealed class KeyToListMap<TKey, TValue> : InternalBase
{
	private Dictionary<TKey, List<TValue>> m_map;

	internal IEnumerable<TValue> AllValues
	{
		get
		{
			foreach (TKey key in Keys)
			{
				foreach (TValue item in ListForKey(key))
				{
					yield return item;
				}
			}
		}
	}

	internal IEnumerable<TKey> Keys => m_map.Keys;

	internal IEnumerable<KeyValuePair<TKey, List<TValue>>> KeyValuePairs
	{
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		get
		{
			return m_map;
		}
	}

	internal KeyToListMap(IEqualityComparer<TKey> comparer)
	{
		m_map = new Dictionary<TKey, List<TValue>>(comparer);
	}

	internal void Add(TKey key, TValue value)
	{
		if (!m_map.TryGetValue(key, out var value2))
		{
			value2 = new List<TValue>();
			m_map[key] = value2;
		}
		value2.Add(value);
	}

	internal void AddRange(TKey key, IEnumerable<TValue> values)
	{
		foreach (TValue value in values)
		{
			Add(key, value);
		}
	}

	internal bool ContainsKey(TKey key)
	{
		return m_map.ContainsKey(key);
	}

	internal IEnumerable<TValue> EnumerateValues(TKey key)
	{
		if (!m_map.TryGetValue(key, out var value))
		{
			yield break;
		}
		foreach (TValue item in value)
		{
			yield return item;
		}
	}

	internal ReadOnlyCollection<TValue> ListForKey(TKey key)
	{
		return new ReadOnlyCollection<TValue>(m_map[key]);
	}

	internal bool RemoveKey(TKey key)
	{
		return m_map.Remove(key);
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		foreach (TKey key in Keys)
		{
			StringUtil.FormatStringBuilder(builder, "{0}", key);
			builder.Append(": ");
			IEnumerable<TValue> list = ListForKey(key);
			StringUtil.ToSeparatedString(builder, list, ",", "null");
			builder.Append("; ");
		}
	}

	internal bool TryGetListForKey(TKey key, out ReadOnlyCollection<TValue> valueCollection)
	{
		valueCollection = null;
		if (m_map.TryGetValue(key, out var value))
		{
			valueCollection = new ReadOnlyCollection<TValue>(value);
			return true;
		}
		return false;
	}
}
