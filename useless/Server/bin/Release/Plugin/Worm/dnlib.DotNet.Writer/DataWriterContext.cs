using System.IO;

namespace dnlib.DotNet.Writer;

internal sealed class DataWriterContext
{
	public readonly MemoryStream OutStream;

	public readonly DataWriter Writer;

	public DataWriterContext()
	{
		OutStream = new MemoryStream();
		Writer = new DataWriter(OutStream);
	}
}
