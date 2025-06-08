using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public enum FileExistsOpts : byte
{
	ReturnError,
	Append,
	TruncateToZero
}
