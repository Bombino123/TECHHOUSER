using System;
using System.Collections;
using System.Collections.Generic;

namespace AntdUI;

public class BaseCollection : IList, ICollection, IEnumerable
{
	public Action<bool>? action;

	private object[]? list;

	private int count;

	public virtual object? this[int index]
	{
		get
		{
			return get(index);
		}
		set
		{
			set(index, value);
		}
	}

	public bool IsFixedSize => false;

	public bool IsReadOnly => false;

	public int Count => count;

	public bool IsSynchronized => true;

	public object SyncRoot => this;

	private void PropertyChanged(object value)
	{
		if (value is NotifyProperty notifyProperty)
		{
			notifyProperty.PropertyChanged += delegate
			{
				action?.Invoke(obj: false);
			};
		}
	}

	private object? get(int index)
	{
		if (list == null || index < 0 || index >= count)
		{
			return null;
		}
		return list[index];
	}

	private void set(int index, object? value)
	{
		if (value != null && list != null && index >= 0 && index < count)
		{
			list[index] = value;
			PropertyChanged(value);
		}
	}

	public int Add(object? value)
	{
		if (value == null)
		{
			return -1;
		}
		EnsureSpace(1)[count++] = value;
		PropertyChanged(value);
		action?.Invoke(obj: true);
		return IndexOf(value);
	}

	public void AddRange(object[] items)
	{
		object[] array = EnsureSpace(items.Length);
		foreach (object obj in items)
		{
			array[count++] = obj;
			PropertyChanged(obj);
		}
		action?.Invoke(obj: true);
	}

	public void AddRange(IList<object> items)
	{
		object[] array = EnsureSpace(items.Count);
		foreach (object item in items)
		{
			array[count++] = item;
			PropertyChanged(item);
		}
		action?.Invoke(obj: true);
	}

	public void Clear()
	{
		count = 0;
		list = null;
		action?.Invoke(obj: true);
	}

	public bool Contains(object? value)
	{
		if (value == null)
		{
			return false;
		}
		return IndexOf(value) != -1;
	}

	public void CopyTo(Array array, int index)
	{
		if (list != null)
		{
			Array.Copy(list, 0, array, index, count);
		}
	}

	public IEnumerator GetEnumerator()
	{
		int i = 0;
		for (int Len = count; i < Len; i++)
		{
			yield return list[i];
		}
	}

	public int IndexOf(object? value)
	{
		if (list == null || value == null)
		{
			return -1;
		}
		return Array.IndexOf<object>(list, value);
	}

	public void Insert(int index, object? value)
	{
		if (value != null && index >= 0 && index < count)
		{
			object[] array = EnsureSpace(1);
			for (int num = count; num > index; num--)
			{
				array[num] = array[num - 1];
			}
			array[index] = value;
			count++;
			PropertyChanged(value);
			action?.Invoke(obj: true);
		}
	}

	public void Remove(object? value)
	{
		int num = IndexOf(value);
		if (num > -1)
		{
			RemoveAt(num);
		}
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
			action?.Invoke(obj: true);
		}
	}

	private object[] EnsureSpace(int elements)
	{
		if (list == null)
		{
			list = new object[Math.Max(elements, 4)];
		}
		else if (count + elements > list.Length)
		{
			object[] array = new object[Math.Max(count + elements, list.Length * 2)];
			list.CopyTo(array, 0);
			list = array;
		}
		return list;
	}
}
