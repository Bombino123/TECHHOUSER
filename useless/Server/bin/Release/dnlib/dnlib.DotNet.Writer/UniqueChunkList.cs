using System;
using System.Collections.Generic;
using dnlib.IO;
using dnlib.PE;

namespace dnlib.DotNet.Writer;

public sealed class UniqueChunkList<T> : ChunkListBase<T> where T : class, IChunk
{
	private sealed class DescendingStableComparer : IComparer<KeyValuePair<int, Elem>>
	{
		internal static readonly DescendingStableComparer Instance = new DescendingStableComparer();

		public int Compare(KeyValuePair<int, Elem> x, KeyValuePair<int, Elem> y)
		{
			uint alignment = x.Value.alignment;
			int num = -alignment.CompareTo(y.Value.alignment);
			if (num != 0)
			{
				return num;
			}
			return x.Key.CompareTo(y.Key);
		}
	}

	private Dictionary<Elem, Elem> dict;

	public UniqueChunkList()
		: this((IEqualityComparer<T>)EqualityComparer<T>.Default)
	{
	}

	public UniqueChunkList(IEqualityComparer<T> chunkComparer)
	{
		chunks = new List<Elem>();
		dict = new Dictionary<Elem, Elem>(new ElemEqualityComparer(chunkComparer));
	}

	public override void SetOffset(FileOffset offset, RVA rva)
	{
		dict = null;
		base.SetOffset(offset, rva);
	}

	public T Add(T chunk, uint alignment)
	{
		if (setOffsetCalled)
		{
			throw new InvalidOperationException("SetOffset() has already been called");
		}
		if (chunk == null)
		{
			return null;
		}
		Elem elem = new Elem(chunk, alignment);
		if (dict.TryGetValue(elem, out var value))
		{
			return value.chunk;
		}
		dict[elem] = elem;
		chunks.Add(elem);
		return elem.chunk;
	}

	public override uint CalculateAlignment()
	{
		uint result = base.CalculateAlignment();
		KeyValuePair<int, Elem>[] array = new KeyValuePair<int, Elem>[chunks.Count];
		for (int i = 0; i < chunks.Count; i++)
		{
			array[i] = new KeyValuePair<int, Elem>(i, chunks[i]);
		}
		Array.Sort(array, DescendingStableComparer.Instance);
		for (int j = 0; j < array.Length; j++)
		{
			chunks[j] = array[j].Value;
		}
		return result;
	}
}
