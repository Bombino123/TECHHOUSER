using System;
using System.Runtime.InteropServices;

namespace SMBLibrary.SMB2;

[Flags]
[ComVisible(true)]
public enum SMB2TransformHeaderFlags : ushort
{
	Encrypted = 1
}
