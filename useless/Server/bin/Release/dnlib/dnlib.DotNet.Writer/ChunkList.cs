using System;
using System.Collections.Generic;

namespace dnlib.DotNet.Writer;

public class ChunkList<T> : ChunkListBase<T> where T : class, IChunk
{
	public ChunkList()
	{
		chunks = new List<Elem>();
	}

	public void Add(T chunk, uint alignment)
	{
		if (setOffsetCalled)
		{
			throw new InvalidOperationException("SetOffset() has already been called");
		}
		if (chunk != null)
		{
			chunks.Add(new Elem(chunk, alignment));
		}
	}

	public uint? Remove(T chunk)
	{
		if (setOffsetCalled)
		{
			throw new InvalidOperationException("SetOffset() has already been called");
		}
		if (chunk != null)
		{
			List<Elem> list = chunks;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].chunk == chunk)
				{
					uint alignment = list[i].alignment;
					list.RemoveAt(i);
					return alignment;
				}
			}
		}
		return null;
	}
}
