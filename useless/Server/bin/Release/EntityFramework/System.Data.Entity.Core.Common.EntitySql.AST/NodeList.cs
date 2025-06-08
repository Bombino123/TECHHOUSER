using System.Collections;
using System.Collections.Generic;

namespace System.Data.Entity.Core.Common.EntitySql.AST;

internal sealed class NodeList<T> : Node, IEnumerable<T>, IEnumerable where T : Node
{
	private readonly List<T> _list = new List<T>();

	internal int Count => _list.Count;

	internal T this[int index] => _list[index];

	internal NodeList()
	{
	}

	internal NodeList(T item)
	{
		_list.Add(item);
	}

	internal NodeList<T> Add(T item)
	{
		_list.Add(item);
		return this;
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return _list.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _list.GetEnumerator();
	}
}
