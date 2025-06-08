using System.Runtime.InteropServices;
using dnlib.IO;

namespace dnlib.DotNet.MD;

[ComVisible(true)]
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
