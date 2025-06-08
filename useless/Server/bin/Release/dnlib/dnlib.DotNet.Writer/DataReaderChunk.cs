using System;
using dnlib.IO;
using dnlib.PE;

namespace dnlib.DotNet.Writer;

public class DataReaderChunk : IChunk
{
	private FileOffset offset;

	private RVA rva;

	private DataReader data;

	private readonly uint virtualSize;

	private bool setOffsetCalled;

	public FileOffset FileOffset => offset;

	public RVA RVA => rva;

	public DataReaderChunk(DataReader data)
		: this(ref data)
	{
	}

	public DataReaderChunk(DataReader data, uint virtualSize)
		: this(ref data, virtualSize)
	{
	}

	internal DataReaderChunk(ref DataReader data)
		: this(ref data, data.Length)
	{
	}

	internal DataReaderChunk(ref DataReader data, uint virtualSize)
	{
		this.data = data;
		this.virtualSize = virtualSize;
	}

	public DataReader CreateReader()
	{
		return data;
	}

	public void SetData(DataReader newData)
	{
		if (setOffsetCalled && newData.Length != data.Length)
		{
			throw new InvalidOperationException("New data must be the same size as the old data after SetOffset() has been called");
		}
		data = newData;
	}

	public void SetOffset(FileOffset offset, RVA rva)
	{
		this.offset = offset;
		this.rva = rva;
		setOffsetCalled = true;
	}

	public uint GetFileLength()
	{
		return data.Length;
	}

	public uint GetVirtualSize()
	{
		return virtualSize;
	}

	public uint CalculateAlignment()
	{
		return 0u;
	}

	public void WriteTo(DataWriter writer)
	{
		data.Position = 0u;
		data.CopyTo(writer);
	}
}
