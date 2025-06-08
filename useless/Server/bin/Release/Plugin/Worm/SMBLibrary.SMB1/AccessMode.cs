using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public enum AccessMode : byte
{
	Read,
	Write,
	ReadWrite,
	Execute
}
