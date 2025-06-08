using System;

namespace dnlib.IO;

public sealed class NativeMemoryDataReaderFactory : DataReaderFactory
{
	private DataStream stream;

	private string filename;

	private uint length;

	public override string Filename => filename;

	public override uint Length => length;

	private unsafe NativeMemoryDataReaderFactory(byte* data, uint length, string filename)
	{
		this.filename = filename;
		this.length = length;
		stream = DataStreamFactory.Create(data);
	}

	internal void SetLength(uint length)
	{
		this.length = length;
	}

	public unsafe static NativeMemoryDataReaderFactory Create(byte* data, uint length, string filename)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		return new NativeMemoryDataReaderFactory(data, length, filename);
	}

	public override DataReader CreateReader(uint offset, uint length)
	{
		return CreateReader(stream, offset, length);
	}

	public override void Dispose()
	{
		stream = EmptyDataStream.Instance;
		length = 0u;
		filename = null;
	}
}
