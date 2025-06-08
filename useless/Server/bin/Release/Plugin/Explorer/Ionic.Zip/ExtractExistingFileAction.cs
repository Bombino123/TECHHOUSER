using System.Runtime.InteropServices;

namespace Ionic.Zip;

[ComVisible(true)]
public enum ExtractExistingFileAction
{
	Throw,
	OverwriteSilently,
	DoNotOverwrite,
	InvokeExtractProgressEvent
}
