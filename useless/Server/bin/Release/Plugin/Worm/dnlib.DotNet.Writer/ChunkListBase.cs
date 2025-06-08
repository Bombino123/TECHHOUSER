using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using dnlib.IO;
using dnlib.PE;

namespace dnlib.DotNet.Writer;

[ComVisible(true)]
public abstract class ChunkListBase<T> : IChunk where T : IChunk
{
	protected readonly struct Elem
	{
		public readonly T chunk;

		public readonly uint alignment;

		public Elem(T chunk, uint alignment)
		{
			this.chunk = chunk;
			this.alignment = alignment;
		}
	}

	protected sealed class ElemEqualityComparer : IEqualityComparer<Elem>
	{
		private IEqualityComparer<T> chunkComparer;

		public ElemEqualityComparer(IEqualityComparer<T> chunkComparer)
		{
			this.chunkComparer = chunkComparer;
		}

		public bool Equals(Elem x, Elem y)
		{
			if (x.alignment == y.alignment)
			{
				return chunkComparer.Equals(x.chunk, y.chunk);
			}
			return false;
		}

		public int GetHashCode(Elem obj)
		{
			return (int)obj.alignment + chunkComparer.GetHashCode(obj.chunk);
		}
	}

	protected List<Elem> chunks;

	private uint length;

	private uint virtualSize;

	protected bool setOffsetCalled;

	private FileOffset offset;

	private RVA rva;

	internal bool IsEmpty => chunks.Count == 0;

	public FileOffset FileOffset => offset;

	public RVA RVA => rva;

	public virtual void SetOffset(FileOffset offset, RVA rva)
	{
		setOffsetCalled = true;
		this.offset = offset;
		this.rva = rva;
		length = 0u;
		virtualSize = 0u;
		foreach (Elem chunk in chunks)
		{
			uint num = offset.AlignUp(chunk.alignment) - offset;
			uint num2 = rva.AlignUp(chunk.alignment) - rva;
			offset += num;
			rva += num2;
			chunk.chunk.SetOffset(offset, rva);
			if (chunk.chunk.GetVirtualSize() == 0)
			{
				offset -= num;
				rva -= num2;
				continue;
			}
			uint fileLength = chunk.chunk.GetFileLength();
			uint num3 = chunk.chunk.GetVirtualSize();
			offset += fileLength;
			rva += num3;
			length += num + fileLength;
			virtualSize += num2 + num3;
		}
	}

	public uint GetFileLength()
	{
		return length;
	}

	public uint GetVirtualSize()
	{
		return virtualSize;
	}

	public void WriteTo(DataWriter writer)
	{
		FileOffset fileOffset = offset;
		foreach (Elem chunk in chunks)
		{
			if (chunk.chunk.GetVirtualSize() != 0)
			{
				int num = (int)(fileOffset.AlignUp(chunk.alignment) - fileOffset);
				writer.WriteZeroes(num);
				chunk.chunk.VerifyWriteTo(writer);
				fileOffset = (FileOffset)((uint)fileOffset + (uint)(num + (int)chunk.chunk.GetFileLength()));
			}
		}
	}

	public virtual uint CalculateAlignment()
	{
		uint num = 0u;
		for (int i = 0; i < chunks.Count; i++)
		{
			Elem elem = chunks[i];
			uint num2 = Math.Max(elem.alignment, elem.chunk.CalculateAlignment());
			chunks[i] = new Elem(elem.chunk, num2);
			num = Math.Max(num, num2);
		}
		return num;
	}
}
