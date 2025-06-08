using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using dnlib.Threading;

namespace dnlib.Utils;

[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(CollectionDebugView<>))]
[ComVisible(true)]
public class LazyList<TValue> : ILazyList<TValue>, IList<TValue>, ICollection<TValue>, IEnumerable<TValue>, IEnumerable where TValue : class
{
	private protected class Element
	{
		protected TValue value;

		public virtual bool IsInitialized_NoLock => true;

		protected Element()
		{
		}

		public Element(TValue data)
		{
			value = data;
		}

		public virtual TValue GetValue_NoLock(int index)
		{
			return value;
		}

		public virtual void SetValue_NoLock(int index, TValue value)
		{
			this.value = value;
		}

		public override string ToString()
		{
			return value?.ToString() ?? string.Empty;
		}
	}

	public struct Enumerator : IEnumerator<TValue>, IDisposable, IEnumerator
	{
		private readonly LazyList<TValue> list;

		private readonly int id;

		private int index;

		private TValue current;

		public TValue Current => current;

		object IEnumerator.Current => current;

		internal Enumerator(LazyList<TValue> list)
		{
			this.list = list;
			index = 0;
			current = null;
			list.theLock.EnterReadLock();
			try
			{
				id = list.id;
			}
			finally
			{
				list.theLock.ExitReadLock();
			}
		}

		public bool MoveNext()
		{
			list.theLock.EnterWriteLock();
			try
			{
				if (list.id == id && index < list.Count_NoLock)
				{
					current = list.list[index].GetValue_NoLock(index);
					index++;
					return true;
				}
				return MoveNextDoneOrThrow_NoLock();
			}
			finally
			{
				list.theLock.ExitWriteLock();
			}
		}

		private bool MoveNextDoneOrThrow_NoLock()
		{
			if (list.id != id)
			{
				throw new InvalidOperationException("List was modified");
			}
			current = null;
			return false;
		}

		public void Dispose()
		{
		}

		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	private protected readonly List<Element> list;

	private int id;

	private protected readonly IListListener<TValue> listener;

	private readonly Lock theLock = Lock.Create();

	public int Count
	{
		get
		{
			theLock.EnterReadLock();
			try
			{
				return Count_NoLock;
			}
			finally
			{
				theLock.ExitReadLock();
			}
		}
	}

	internal int Count_NoLock => list.Count;

	public bool IsReadOnly => false;

	public TValue this[int index]
	{
		get
		{
			theLock.EnterWriteLock();
			try
			{
				return Get_NoLock(index);
			}
			finally
			{
				theLock.ExitWriteLock();
			}
		}
		set
		{
			theLock.EnterWriteLock();
			try
			{
				Set_NoLock(index, value);
			}
			finally
			{
				theLock.ExitWriteLock();
			}
		}
	}

	internal TValue Get_NoLock(int index)
	{
		return list[index].GetValue_NoLock(index);
	}

	private void Set_NoLock(int index, TValue value)
	{
		if (listener != null)
		{
			listener.OnRemove(index, list[index].GetValue_NoLock(index));
			listener.OnAdd(index, value);
		}
		list[index].SetValue_NoLock(index, value);
		id++;
	}

	public LazyList()
		: this((IListListener<TValue>)null)
	{
	}

	public LazyList(IListListener<TValue> listener)
	{
		this.listener = listener;
		list = new List<Element>();
	}

	private protected LazyList(int length, IListListener<TValue> listener)
	{
		this.listener = listener;
		list = new List<Element>(length);
	}

	public int IndexOf(TValue item)
	{
		theLock.EnterWriteLock();
		try
		{
			return IndexOf_NoLock(item);
		}
		finally
		{
			theLock.ExitWriteLock();
		}
	}

	private int IndexOf_NoLock(TValue item)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].GetValue_NoLock(i) == item)
			{
				return i;
			}
		}
		return -1;
	}

	public void Insert(int index, TValue item)
	{
		theLock.EnterWriteLock();
		try
		{
			Insert_NoLock(index, item);
		}
		finally
		{
			theLock.ExitWriteLock();
		}
	}

	private void Insert_NoLock(int index, TValue item)
	{
		if (listener != null)
		{
			listener.OnAdd(index, item);
		}
		list.Insert(index, new Element(item));
		if (listener != null)
		{
			listener.OnResize(index);
		}
		id++;
	}

	public void RemoveAt(int index)
	{
		theLock.EnterWriteLock();
		try
		{
			RemoveAt_NoLock(index);
		}
		finally
		{
			theLock.ExitWriteLock();
		}
	}

	private void RemoveAt_NoLock(int index)
	{
		if (listener != null)
		{
			listener.OnRemove(index, list[index].GetValue_NoLock(index));
		}
		list.RemoveAt(index);
		if (listener != null)
		{
			listener.OnResize(index);
		}
		id++;
	}

	public void Add(TValue item)
	{
		theLock.EnterWriteLock();
		try
		{
			Add_NoLock(item);
		}
		finally
		{
			theLock.ExitWriteLock();
		}
	}

	private void Add_NoLock(TValue item)
	{
		int count = list.Count;
		if (listener != null)
		{
			listener.OnAdd(count, item);
		}
		list.Add(new Element(item));
		if (listener != null)
		{
			listener.OnResize(count);
		}
		id++;
	}

	public void Clear()
	{
		theLock.EnterWriteLock();
		try
		{
			Clear_NoLock();
		}
		finally
		{
			theLock.ExitWriteLock();
		}
	}

	private void Clear_NoLock()
	{
		if (listener != null)
		{
			listener.OnClear();
		}
		list.Clear();
		if (listener != null)
		{
			listener.OnResize(0);
		}
		id++;
	}

	public bool Contains(TValue item)
	{
		return IndexOf(item) >= 0;
	}

	public void CopyTo(TValue[] array, int arrayIndex)
	{
		theLock.EnterWriteLock();
		try
		{
			CopyTo_NoLock(array, arrayIndex);
		}
		finally
		{
			theLock.ExitWriteLock();
		}
	}

	private void CopyTo_NoLock(TValue[] array, int arrayIndex)
	{
		for (int i = 0; i < list.Count; i++)
		{
			array[arrayIndex + i] = list[i].GetValue_NoLock(i);
		}
	}

	public bool Remove(TValue item)
	{
		theLock.EnterWriteLock();
		try
		{
			return Remove_NoLock(item);
		}
		finally
		{
			theLock.ExitWriteLock();
		}
	}

	private bool Remove_NoLock(TValue item)
	{
		int num = IndexOf_NoLock(item);
		if (num < 0)
		{
			return false;
		}
		RemoveAt_NoLock(num);
		return true;
	}

	internal bool IsInitialized(int index)
	{
		theLock.EnterReadLock();
		try
		{
			return IsInitialized_NoLock(index);
		}
		finally
		{
			theLock.ExitReadLock();
		}
	}

	private bool IsInitialized_NoLock(int index)
	{
		if ((uint)index >= (uint)list.Count)
		{
			return false;
		}
		return list[index].IsInitialized_NoLock;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
	{
		return GetEnumerator();
	}

	internal IEnumerable<TValue> GetEnumerable_NoLock()
	{
		int id2 = id;
		for (int i = 0; i < list.Count; i++)
		{
			if (id != id2)
			{
				throw new InvalidOperationException("List was modified");
			}
			yield return list[i].GetValue_NoLock(i);
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(CollectionDebugView<, >))]
[ComVisible(true)]
public class LazyList<TValue, TContext> : LazyList<TValue>, ILazyList<TValue>, IList<TValue>, ICollection<TValue>, IEnumerable<TValue>, IEnumerable where TValue : class
{
	private sealed class LazyElement : Element
	{
		internal readonly int origIndex;

		private LazyList<TValue, TContext> lazyList;

		public override bool IsInitialized_NoLock => lazyList == null;

		public override TValue GetValue_NoLock(int index)
		{
			if (lazyList != null)
			{
				value = lazyList.ReadOriginalValue_NoLock(index, origIndex);
				lazyList = null;
			}
			return value;
		}

		public override void SetValue_NoLock(int index, TValue value)
		{
			base.value = value;
			lazyList = null;
		}

		public LazyElement(int origIndex, LazyList<TValue, TContext> lazyList)
		{
			this.origIndex = origIndex;
			this.lazyList = lazyList;
		}

		public override string ToString()
		{
			if (lazyList != null)
			{
				value = lazyList.ReadOriginalValue_NoLock(this);
				lazyList = null;
			}
			if (value != null)
			{
				return value.ToString();
			}
			return string.Empty;
		}
	}

	private TContext context;

	private readonly Func<TContext, int, TValue> readOriginalValue;

	public LazyList()
		: this((IListListener<TValue>)null)
	{
	}

	public LazyList(IListListener<TValue> listener)
		: base(listener)
	{
	}

	public LazyList(int length, TContext context, Func<TContext, int, TValue> readOriginalValue)
		: this(length, (IListListener<TValue>)null, context, readOriginalValue)
	{
	}

	public LazyList(int length, IListListener<TValue> listener, TContext context, Func<TContext, int, TValue> readOriginalValue)
		: base(length, listener)
	{
		this.context = context;
		this.readOriginalValue = readOriginalValue;
		for (int i = 0; i < length; i++)
		{
			list.Add(new LazyElement(i, this));
		}
	}

	private TValue ReadOriginalValue_NoLock(LazyElement elem)
	{
		return ReadOriginalValue_NoLock(list.IndexOf(elem), elem.origIndex);
	}

	private TValue ReadOriginalValue_NoLock(int index, int origIndex)
	{
		TValue value = readOriginalValue(context, origIndex);
		listener?.OnLazyAdd(index, ref value);
		return value;
	}
}
