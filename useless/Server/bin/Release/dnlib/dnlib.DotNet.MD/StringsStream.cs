using dnlib.IO;

namespace dnlib.DotNet.MD;

public sealed class StringsStream : HeapStream
{
	public StringsStream()
	{
	}

	public StringsStream(DataReaderFactory mdReaderFactory, uint metadataBaseOffset, StreamHeader streamHeader)
		: base(mdReaderFactory, metadataBaseOffset, streamHeader)
	{
	}

	public UTF8String Read(uint offset)
	{
		if (offset >= base.StreamLength)
		{
			return null;
		}
		DataReader dataReader = base.dataReader;
		dataReader.Position = offset;
		byte[] array = dataReader.TryReadBytesUntil(0);
		if (array == null)
		{
			return null;
		}
		return new UTF8String(array);
	}

	public UTF8String ReadNoNull(uint offset)
	{
		return Read(offset) ?? UTF8String.Empty;
	}
}
