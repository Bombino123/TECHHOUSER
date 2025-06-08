using System.Diagnostics;
using System.Runtime.InteropServices;

namespace dnlib.IO;

[DebuggerDisplay("O:{startOffset} L:{size} {GetType().Name}")]
[ComVisible(true)]
public class FileSection : IFileSection
{
	protected FileOffset startOffset;

	protected uint size;

	public FileOffset StartOffset => startOffset;

	public FileOffset EndOffset => startOffset + size;

	protected void SetStartOffset(ref DataReader reader)
	{
		startOffset = (FileOffset)reader.CurrentOffset;
	}

	protected void SetEndoffset(ref DataReader reader)
	{
		size = (uint)(reader.CurrentOffset - startOffset);
	}
}
