using dnlib.IO;

namespace dnlib.DotNet.MD;

public abstract class HeapStream : DotNetStream
{
	protected HeapStream()
	{
	}

	protected HeapStream(DataReaderFactory mdReaderFactory, uint metadataBaseOffset, StreamHeader streamHeader)
		: base(mdReaderFactory, metadataBaseOffset, streamHeader)
	{
	}
}
