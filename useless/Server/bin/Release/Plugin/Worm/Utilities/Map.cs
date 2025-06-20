using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Utilities;

[ComVisible(true)]
public class Map<T1, T2>
{
	private Dictionary<T1, T2> m_forward = new Dictionary<T1, T2>();

	private Dictionary<T2, T1> m_reverse = new Dictionary<T2, T1>();

	public T2 this[T1 key] => m_forward[key];

	public Map()
	{
		m_forward = new Dictionary<T1, T2>();
		m_reverse = new Dictionary<T2, T1>();
	}

	public void Add(T1 key, T2 value)
	{
		m_forward.Add(key, value);
		m_reverse.Add(value, key);
	}

	public bool ContainsKey(T1 key)
	{
		return m_forward.ContainsKey(key);
	}

	public bool ContainsValue(T2 value)
	{
		return m_reverse.ContainsKey(value);
	}

	public bool TryGetKey(T2 value, out T1 key)
	{
		return m_reverse.TryGetValue(value, out key);
	}

	public bool TryGetValue(T1 key, out T2 value)
	{
		return m_forward.TryGetValue(key, out value);
	}

	public void RemoveKey(T1 key)
	{
		if (m_forward.TryGetValue(key, out var value))
		{
			m_forward.Remove(key);
			m_reverse.Remove(value);
		}
	}

	public void RemoveValue(T2 value)
	{
		if (m_reverse.TryGetValue(value, out var value2))
		{
			m_forward.Remove(value2);
			m_reverse.Remove(value);
		}
	}

	public T1 GetKey(T2 value)
	{
		return m_reverse[value];
	}
}
