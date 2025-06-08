using System;
using System.Runtime.InteropServices;

namespace dnlib.IO;

[ComVisible(true)]
public sealed class ByteArrayDataReaderFactory : DataReaderFactory
{
	private DataStream stream;

	private string filename;

	private uint length;

	private byte[] data;

	public override string Filename => filename;

	public override uint Length => length;

	internal byte[] DataArray => data;

	internal uint DataOffset => 0u;

	private ByteArrayDataReaderFactory(byte[] data, string filename)
	{
		this.filename = filename;
		length = (uint)data.Length;
		stream = DataStreamFactory.Create(data);
		this.data = data;
	}

	public static ByteArrayDataReaderFactory Create(byte[] data, string filename)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		return new ByteArrayDataReaderFactory(data, filename);
	}

	public static DataReader CreateReader(byte[] data)
	{
		return Create(data, null).CreateReader();
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
		data = null;
	}
}
