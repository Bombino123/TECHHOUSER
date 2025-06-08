using System;
using System.Runtime.InteropServices;

namespace SMBLibrary.SMB2;

[Flags]
[ComVisible(true)]
public enum SessionFlags : ushort
{
	IsGuest = 1,
	IsNull = 2,
	EncryptData = 4
}
