using System;
using System.Runtime.InteropServices;
using dnlib.IO;

namespace dnlib.DotNet.MD;

[ComVisible(true)]
public sealed class USStream : HeapStream
{
	public USStream()
	{
	}

	public USStream(DataReaderFactory mdReaderFactory, uint metadataBaseOffset, StreamHeader streamHeader)
		: base(mdReaderFactory, metadataBaseOffset, streamHeader)
	{
	}

	public string Read(uint offset)
	{
		if (offset == 0)
		{
			return string.Empty;
		}
		if (!IsValidOffset(offset))
		{
			return null;
		}
		DataReader dataReader = base.dataReader;
		dataReader.Position = offset;
		if (!dataReader.TryReadCompressedUInt32(out var value))
		{
			return null;
		}
		if (!dataReader.CanRead(value))
		{
			return null;
		}
		try
		{
			return dataReader.ReadUtf16String((int)(value / 2));
		}
		catch (OutOfMemoryException)
		{
			throw;
		}
		catch
		{
			return string.Empty;
		}
	}

	public string ReadNoNull(uint offset)
	{
		return Read(offset) ?? string.Empty;
	}
}
