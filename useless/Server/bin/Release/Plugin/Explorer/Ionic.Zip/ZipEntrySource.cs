using System.Runtime.InteropServices;

namespace Ionic.Zip;

[ComVisible(true)]
public enum ZipEntrySource
{
	None,
	FileSystem,
	Stream,
	ZipFile,
	WriteDelegate,
	JitStream,
	ZipOutputStream
}
