using System.Runtime.InteropServices;

namespace dnlib.IO;

[ComVisible(true)]
public interface IFileSection
{
	FileOffset StartOffset { get; }

	FileOffset EndOffset { get; }
}
