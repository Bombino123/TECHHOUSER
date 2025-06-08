using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public enum WriteThroughMode : byte
{
	Disabled,
	WriteThrough
}
