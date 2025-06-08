using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public enum SharingMode : byte
{
	Compatibility,
	DenyReadWriteExecute,
	DenyWrite,
	DenyReadExecute,
	DenyNothing
}
