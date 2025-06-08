using System;
using dnlib.IO;

namespace dnlib.DotNet.MD;

public sealed class BlobStream : HeapStream
{
	public BlobStream()
	{
	}

	public BlobStream(DataReaderFactory mdReaderFactory, uint metadataBaseOffset, StreamHeader streamHeader)
		: base(mdReaderFactory, metadataBaseOffset, streamHeader)
	{
	}

	public byte[] Read(uint offset)
	{
		if (offset == 0)
		{
			return Array2.Empty<byte>();
		}
		if (!TryCreateReader(offset, out var reader))
		{
			return null;
		}
		return reader.ToArray();
	}

	public byte[] ReadNoNull(uint offset)
	{
		return Read(offset) ?? Array2.Empty<byte>();
	}

	public DataReader CreateReader(uint offset)
	{
		if (TryCreateReader(offset, out var reader))
		{
			return reader;
		}
		return default(DataReader);
	}

	public bool TryCreateReader(uint offset, out DataReader reader)
	{
		reader = dataReader;
		if (!IsValidOffset(offset))
		{
			return false;
		}
		reader.Position = offset;
		if (!reader.TryReadCompressedUInt32(out var value))
		{
			return false;
		}
		if (!reader.CanRead(value))
		{
			return false;
		}
		reader = reader.Slice(reader.Position, value);
		return true;
	}
}
