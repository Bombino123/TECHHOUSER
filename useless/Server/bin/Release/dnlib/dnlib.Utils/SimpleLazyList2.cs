using System;
using System.Diagnostics;
using System.Threading;
using dnlib.DotNet;

namespace dnlib.Utils;

[DebuggerDisplay("Count = {Length}")]
internal sealed class SimpleLazyList2<T> where T : class, IContainsGenericParameter2
{
	[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
	private readonly T[] elements;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly Func<uint, GenericParamContext, T> readElementByRID;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly uint length;

	public uint Length => length;

	public T this[uint index, GenericParamContext gpContext]
	{
		get
		{
			if (index >= length)
			{
				return null;
			}
			if (elements[index] == null)
			{
				T val = readElementByRID(index + 1, gpContext);
				if (val.ContainsGenericParameter)
				{
					return val;
				}
				Interlocked.CompareExchange(ref elements[index], val, null);
			}
			return elements[index];
		}
	}

	public SimpleLazyList2(uint length, Func<uint, GenericParamContext, T> readElementByRID)
	{
		this.length = length;
		this.readElementByRID = readElementByRID;
		elements = new T[length];
	}
}
