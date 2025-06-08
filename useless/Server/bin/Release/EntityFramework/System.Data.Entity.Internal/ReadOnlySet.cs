using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Resources;

namespace System.Data.Entity.Internal;

internal class ReadOnlySet<T> : ISet<T>, ICollection<T>, IEnumerable<T>, IEnumerable
{
	private readonly ISet<T> _set;

	public int Count => _set.Count;

	public bool IsReadOnly => true;

	public ReadOnlySet(ISet<T> set)
	{
		_set = set;
	}

	public bool Add(T item)
	{
		throw Error.DbPropertyValues_PropertyValueNamesAreReadonly();
	}

	public void ExceptWith(IEnumerable<T> other)
	{
		_set.ExceptWith(other);
	}

	public void IntersectWith(IEnumerable<T> other)
	{
		_set.IntersectWith(other);
	}

	public bool IsProperSubsetOf(IEnumerable<T> other)
	{
		return _set.IsProperSubsetOf(other);
	}

	public bool IsProperSupersetOf(IEnumerable<T> other)
	{
		return _set.IsProperSupersetOf(other);
	}

	public bool IsSubsetOf(IEnumerable<T> other)
	{
		return _set.IsSubsetOf(other);
	}

	public bool IsSupersetOf(IEnumerable<T> other)
	{
		return _set.IsSupersetOf(other);
	}

	public bool Overlaps(IEnumerable<T> other)
	{
		return _set.Overlaps(other);
	}

	public bool SetEquals(IEnumerable<T> other)
	{
		return _set.SetEquals(other);
	}

	public void SymmetricExceptWith(IEnumerable<T> other)
	{
		_set.SymmetricExceptWith(other);
	}

	public void UnionWith(IEnumerable<T> other)
	{
		_set.UnionWith(other);
	}

	void ICollection<T>.Add(T item)
	{
		throw Error.DbPropertyValues_PropertyValueNamesAreReadonly();
	}

	public void Clear()
	{
		throw Error.DbPropertyValues_PropertyValueNamesAreReadonly();
	}

	public bool Contains(T item)
	{
		return _set.Contains(item);
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		_set.CopyTo(array, arrayIndex);
	}

	public bool Remove(T item)
	{
		throw Error.DbPropertyValues_PropertyValueNamesAreReadonly();
	}

	public IEnumerator<T> GetEnumerator()
	{
		return _set.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)_set).GetEnumerator();
	}
}
