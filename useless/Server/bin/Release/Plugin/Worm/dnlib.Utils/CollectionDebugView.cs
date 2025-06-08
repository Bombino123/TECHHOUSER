using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace dnlib.Utils;

internal class CollectionDebugView<TValue>
{
	private readonly ICollection<TValue> list;

	[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
	public TValue[] Items
	{
		get
		{
			TValue[] array = new TValue[list.Count];
			list.CopyTo(array, 0);
			return array;
		}
	}

	public CollectionDebugView(ICollection<TValue> list)
	{
		this.list = list ?? throw new ArgumentNullException("list");
	}
}
internal class CollectionDebugView<TValue, TOther> : CollectionDebugView<TValue>
{
	public CollectionDebugView(ICollection<TValue> list)
		: base(list)
	{
	}
}
