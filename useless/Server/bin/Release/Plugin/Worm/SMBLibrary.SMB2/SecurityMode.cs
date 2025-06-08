using System;
using System.Runtime.InteropServices;

namespace SMBLibrary.SMB2;

[Flags]
[ComVisible(true)]
public enum SecurityMode : ushort
{
	SigningEnabled = 1,
	SigningRequired = 2
}
