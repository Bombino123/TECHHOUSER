using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal class VarMap : IDictionary<Var, Var>, ICollection<KeyValuePair<Var, Var>>, IEnumerable<KeyValuePair<Var, Var>>, IEnumerable
{
	private Dictionary<Var, Var> map;

	private Dictionary<Var, Var> reverseMap;

	public Var this[Var key]
	{
		get
		{
			return map[key];
		}
		set
		{
			map[key] = value;
		}
	}

	public ICollection<Var> Keys => map.Keys;

	public ICollection<Var> Values => map.Values;

	public int Count => map.Count;

	public bool IsReadOnly => false;

	internal VarMap GetReverseMap()
	{
		return new VarMap(reverseMap, map);
	}

	public bool ContainsValue(Var value)
	{
		return reverseMap.ContainsKey(value);
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = string.Empty;
		foreach (Var key in map.Keys)
		{
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}({1},{2})", new object[3]
			{
				text,
				key.Id,
				this[key].Id
			});
			text = ",";
		}
		return stringBuilder.ToString();
	}

	public void Add(Var key, Var value)
	{
		if (!reverseMap.ContainsKey(value))
		{
			reverseMap.Add(value, key);
		}
		map.Add(key, value);
	}

	public void Add(KeyValuePair<Var, Var> item)
	{
		if (!reverseMap.ContainsKey(item.Value))
		{
			((ICollection<KeyValuePair<Var, Var>>)reverseMap).Add(new KeyValuePair<Var, Var>(item.Value, item.Key));
		}
		((ICollection<KeyValuePair<Var, Var>>)map).Add(item);
	}

	public void Clear()
	{
		map.Clear();
		reverseMap.Clear();
	}

	public bool Contains(KeyValuePair<Var, Var> item)
	{
		return ((ICollection<KeyValuePair<Var, Var>>)map).Contains(item);
	}

	public bool ContainsKey(Var key)
	{
		return map.ContainsKey(key);
	}

	public void CopyTo(KeyValuePair<Var, Var>[] array, int arrayIndex)
	{
		((ICollection<KeyValuePair<Var, Var>>)map).CopyTo(array, arrayIndex);
	}

	public IEnumerator<KeyValuePair<Var, Var>> GetEnumerator()
	{
		return map.GetEnumerator();
	}

	public bool Remove(Var key)
	{
		reverseMap.Remove(map[key]);
		return map.Remove(key);
	}

	public bool Remove(KeyValuePair<Var, Var> item)
	{
		reverseMap.Remove(map[item.Value]);
		return ((ICollection<KeyValuePair<Var, Var>>)map).Remove(item);
	}

	public bool TryGetValue(Var key, out Var value)
	{
		return ((IDictionary<Var, Var>)map).TryGetValue(key, out value);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return map.GetEnumerator();
	}

	public VarMap()
	{
		map = new Dictionary<Var, Var>();
		reverseMap = new Dictionary<Var, Var>();
	}

	private VarMap(Dictionary<Var, Var> map, Dictionary<Var, Var> reverseMap)
	{
		this.map = map;
		this.reverseMap = reverseMap;
	}
}
