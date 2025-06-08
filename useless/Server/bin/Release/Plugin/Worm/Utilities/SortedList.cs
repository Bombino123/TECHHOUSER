using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Utilities;

[ComVisible(true)]
public class SortedList<T> : ICollection<T>, IEnumerable<T>, IEnumerable
{
	private List<T> m_innerList;

	private Comparer<T> m_comparer;

	public T this[int index] => m_innerList[index];

	public int Count => m_innerList.Count;

	public bool IsReadOnly => false;

	public SortedList()
		: this(Comparer<T>.Default)
	{
	}

	public SortedList(Comparer<T> comparer)
	{
		m_innerList = new List<T>();
		m_comparer = comparer;
	}

	public void Add(T item)
	{
		int index = FindIndexForSortedInsert(m_innerList, m_comparer, item);
		m_innerList.Insert(index, item);
	}

	public bool Contains(T item)
	{
		return IndexOf(item) != -1;
	}

	public int IndexOf(T item)
	{
		return FirstIndexOf(m_innerList, m_comparer, item);
	}

	public bool Remove(T item)
	{
		int num = IndexOf(item);
		if (num >= 0)
		{
			m_innerList.RemoveAt(num);
			return true;
		}
		return false;
	}

	public void RemoveAt(int index)
	{
		m_innerList.RemoveAt(index);
	}

	public void CopyTo(T[] array)
	{
		m_innerList.CopyTo(array);
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		m_innerList.CopyTo(array, arrayIndex);
	}

	public void Clear()
	{
		m_innerList.Clear();
	}

	public IEnumerator<T> GetEnumerator()
	{
		return m_innerList.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return m_innerList.GetEnumerator();
	}

	public static int FirstIndexOf(List<T> list, Comparer<T> comparer, T item)
	{
		return FirstIndexOf(list, comparer.Compare, item);
	}

	public static int FindIndexForSortedInsert(List<T> list, Comparer<T> comparer, T item)
	{
		return FindIndexForSortedInsert(list, comparer.Compare, item);
	}

	public static int FirstIndexOf(List<T> list, Comparison<T> compare, T item)
	{
		int num = FindIndexForSortedInsert(list, compare, item);
		if (num == list.Count)
		{
			return -1;
		}
		if (compare(item, list[num]) == 0)
		{
			int num2 = num;
			while (num2 > 0 && compare(item, list[num2 - 1]) == 0)
			{
				num2--;
			}
			return num2;
		}
		return -1;
	}

	public static int FindIndexForSortedInsert(List<T> list, Comparison<T> compare, T item)
	{
		if (list.Count == 0)
		{
			return 0;
		}
		int num = 0;
		int num2 = list.Count - 1;
		int num4;
		while (num < num2)
		{
			int num3 = (num + num2) / 2;
			T x = list[num3];
			num4 = compare(x, item);
			if (num4 == 0)
			{
				return num3;
			}
			if (num4 > 0)
			{
				num2 = num3 - 1;
			}
			else
			{
				num = num3 + 1;
			}
		}
		num4 = compare(list[num], item);
		if (num4 < 0)
		{
			return num + 1;
		}
		return num;
	}
}
