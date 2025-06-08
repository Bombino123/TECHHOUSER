using System;
using System.Collections;
using System.Collections.Generic;

namespace AntdUI;

public class AntList<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
{
	public Action<string, object>? action;

	private int count;

	private T[]? list;

	public T this[int index]
	{
		get
		{
			if (list == null || index < 0 || index >= count)
			{
				throw new Exception("Null List");
			}
			return list[index];
		}
		set
		{
			if (value != null && list != null && index >= 0 && index < count)
			{
				list[index] = value;
				action?.Invoke("edit", index);
			}
		}
	}

	public int Count => count;

	public bool IsReadOnly => false;

	public AntList()
	{
	}

	public AntList(int capacity)
	{
		EnsureSpace(capacity);
	}

	public AntList(IList<T> collection)
	{
		EnsureSpace(collection.Count);
		AddRange(collection);
	}

	public void Add(T item)
	{
		if (item != null)
		{
			int num = count++;
			EnsureSpace(1)[num] = item;
			action?.Invoke("add", num);
		}
	}

	public void AddRange(T[] items)
	{
		T[] array = EnsureSpace(items.Length);
		List<int> list = new List<int>(items.Length);
		foreach (T val in items)
		{
			int num = count++;
			list.Add(num);
			array[num] = val;
		}
		action?.Invoke("add", list.ToArray());
	}

	public void AddRange(IList<T> items)
	{
		T[] array = EnsureSpace(items.Count);
		List<int> list = new List<int>(items.Count);
		foreach (T item in items)
		{
			int num = count++;
			list.Add(num);
			array[num] = item;
		}
		action?.Invoke("add", list.ToArray());
	}

	public void Clear()
	{
		count = 0;
		list = null;
		action?.Invoke("del", "all");
	}

	public bool Contains(T item)
	{
		if (item == null)
		{
			return false;
		}
		return IndexOf(item) != -1;
	}

	public void CopyTo(T[] array, int index)
	{
		if (list != null)
		{
			Array.Copy(list, 0, array, index, count);
		}
	}

	public int IndexOf(T item)
	{
		if (list == null || item == null)
		{
			return -1;
		}
		return Array.IndexOf(list, item);
	}

	public void Insert(int index, T item)
	{
		if (item != null && index >= 0 && (index < count || count <= 0))
		{
			T[] array = EnsureSpace(1);
			for (int num = count; num > index; num--)
			{
				array[num] = array[num - 1];
			}
			array[index] = item;
			count++;
			action?.Invoke("add", index);
		}
	}

	public bool Remove(T item)
	{
		int num = IndexOf(item);
		if (num > -1)
		{
			RemoveAt(num);
			return true;
		}
		return false;
	}

	public void RemoveAt(int index)
	{
		if (list != null && index >= 0 && index < count)
		{
			count--;
			int i = index;
			for (int num = count; i < num; i++)
			{
				list[i] = list[i + 1];
			}
			action?.Invoke("del", index);
		}
	}

	public IEnumerator<T> GetEnumerator()
	{
		int i = 0;
		for (int Len = count; i < Len; i++)
		{
			yield return list[i];
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		int i = 0;
		for (int Len = count; i < Len; i++)
		{
			yield return list[i];
		}
	}

	private T[] EnsureSpace(int elements)
	{
		if (list == null)
		{
			list = new T[Math.Max(elements, 4)];
		}
		else if (count + elements > list.Length)
		{
			T[] array = new T[Math.Max(count + elements, list.Length * 2)];
			list.CopyTo(array, 0);
			list = array;
		}
		return list;
	}
}
