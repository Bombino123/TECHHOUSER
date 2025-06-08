using System;
using System.Collections.Generic;
using dnlib.IO;
using dnlib.PE;

namespace dnlib.DotNet.Writer;

public sealed class NetResources : IReuseChunk, IChunk
{
	private readonly List<DataReaderChunk> resources = new List<DataReaderChunk>();

	private readonly uint alignment;

	private uint length;

	private bool setOffsetCalled;

	private FileOffset offset;

	private RVA rva;

	internal bool IsEmpty => resources.Count == 0;

	public FileOffset FileOffset => offset;

	public RVA RVA => rva;

	public uint NextOffset => Utils.AlignUp(length, alignment);

	public NetResources(uint alignment)
	{
		this.alignment = alignment;
	}

	public DataReaderChunk Add(DataReader reader)
	{
		if (setOffsetCalled)
		{
			throw new InvalidOperationException("SetOffset() has already been called");
		}
		length = NextOffset + 4 + reader.Length;
		DataReaderChunk dataReaderChunk = new DataReaderChunk(ref reader);
		resources.Add(dataReaderChunk);
		return dataReaderChunk;
	}

	bool IReuseChunk.CanReuse(RVA origRva, uint origSize)
	{
		return length <= origSize;
	}

	public void SetOffset(FileOffset offset, RVA rva)
	{
		setOffsetCalled = true;
		this.offset = offset;
		this.rva = rva;
		foreach (DataReaderChunk resource in resources)
		{
			offset = offset.AlignUp(alignment);
			rva = rva.AlignUp(alignment);
			resource.SetOffset(offset + 4, rva + 4);
			uint num = 4 + resource.GetFileLength();
			offset += num;
			rva += num;
		}
	}

	public uint GetFileLength()
	{
		return length;
	}

	public uint GetVirtualSize()
	{
		return GetFileLength();
	}

	public uint CalculateAlignment()
	{
		return 0u;
	}

	public void WriteTo(DataWriter writer)
	{
		RVA rVA = rva;
		foreach (DataReaderChunk resource in resources)
		{
			int num = (int)(rVA.AlignUp(alignment) - rVA);
			writer.WriteZeroes(num);
			rVA = (RVA)((uint)rVA + (uint)num);
			writer.WriteUInt32(resource.GetFileLength());
			resource.VerifyWriteTo(writer);
			rVA += 4 + resource.GetFileLength();
		}
	}
}
