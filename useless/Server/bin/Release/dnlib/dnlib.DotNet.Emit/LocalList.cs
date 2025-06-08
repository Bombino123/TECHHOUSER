using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using dnlib.Utils;

namespace dnlib.DotNet.Emit;

[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(LocalList_CollectionDebugView))]
public sealed class LocalList : IListListener<Local>, IList<Local>, ICollection<Local>, IEnumerable<Local>, IEnumerable
{
	private readonly LazyList<Local> locals;

	public int Count => locals.Count;

	public IList<Local> Locals => locals;

	public Local this[int index]
	{
		get
		{
			return locals[index];
		}
		set
		{
			locals[index] = value;
		}
	}

	public bool IsReadOnly => false;

	public LocalList()
	{
		locals = new LazyList<Local>(this);
	}

	public LocalList(IList<Local> locals)
	{
		this.locals = new LazyList<Local>(this);
		for (int i = 0; i < locals.Count; i++)
		{
			this.locals.Add(locals[i]);
		}
	}

	public Local Add(Local local)
	{
		locals.Add(local);
		return local;
	}

	void IListListener<Local>.OnLazyAdd(int index, ref Local value)
	{
	}

	void IListListener<Local>.OnAdd(int index, Local value)
	{
		value.Index = index;
	}

	void IListListener<Local>.OnRemove(int index, Local value)
	{
		value.Index = -1;
	}

	void IListListener<Local>.OnResize(int index)
	{
		for (int i = index; i < locals.Count_NoLock; i++)
		{
			locals.Get_NoLock(i).Index = i;
		}
	}

	void IListListener<Local>.OnClear()
	{
		foreach (Local item in locals.GetEnumerable_NoLock())
		{
			item.Index = -1;
		}
	}

	public int IndexOf(Local item)
	{
		return locals.IndexOf(item);
	}

	public void Insert(int index, Local item)
	{
		locals.Insert(index, item);
	}

	public void RemoveAt(int index)
	{
		locals.RemoveAt(index);
	}

	void ICollection<Local>.Add(Local item)
	{
		locals.Add(item);
	}

	public void Clear()
	{
		locals.Clear();
	}

	public bool Contains(Local item)
	{
		return locals.Contains(item);
	}

	public void CopyTo(Local[] array, int arrayIndex)
	{
		locals.CopyTo(array, arrayIndex);
	}

	public bool Remove(Local item)
	{
		return locals.Remove(item);
	}

	public LazyList<Local>.Enumerator GetEnumerator()
	{
		return locals.GetEnumerator();
	}

	IEnumerator<Local> IEnumerable<Local>.GetEnumerator()
	{
		return locals.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable<Local>)this).GetEnumerator();
	}
}
