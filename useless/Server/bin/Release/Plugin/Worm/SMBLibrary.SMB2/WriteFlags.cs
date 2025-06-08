using System.Runtime.InteropServices;

namespace SMBLibrary.SMB2;

[ComVisible(true)]
public enum WriteFlags : uint
{
	WriteThrough = 1u,
	Unbuffered
}
