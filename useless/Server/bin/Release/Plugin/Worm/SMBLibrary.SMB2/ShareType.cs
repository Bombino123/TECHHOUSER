using System.Runtime.InteropServices;

namespace SMBLibrary.SMB2;

[ComVisible(true)]
public enum ShareType : byte
{
	Disk = 1,
	Pipe,
	Print
}
