using dnlib.IO;

namespace dnlib.DotNet.MD;

public class CustomDotNetStream : DotNetStream
{
	public CustomDotNetStream()
	{
	}

	public CustomDotNetStream(DataReaderFactory mdReaderFactory, uint metadataBaseOffset, StreamHeader streamHeader)
		: base(mdReaderFactory, metadataBaseOffset, streamHeader)
	{
	}
}
