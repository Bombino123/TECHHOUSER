using System.Runtime.InteropServices;
using dnlib.IO;

namespace dnlib.DotNet.MD;

[ComVisible(true)]
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
