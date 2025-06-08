using System;
using System.Diagnostics;
using System.Threading;

namespace dnlib.Utils;

[DebuggerDisplay("Count = {Length}")]
internal sealed class SimpleLazyList<T> where T : class
{
	[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
	private readonly T[] elements;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly Func<uint, T> readElementByRID;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly uint length;

	public uint Length => length;

	public T this[uint index]
	{
		get
		{
			if (index >= length)
			{
				return null;
			}
			if (elements[index] == null)
			{
				Interlocked.CompareExchange(ref elements[index], readElementByRID(index + 1), null);
			}
			return elements[index];
		}
	}

	public SimpleLazyList(uint length, Func<uint, T> readElementByRID)
	{
		this.length = length;
		this.readElementByRID = readElementByRID;
		elements = new T[length];
	}
}
