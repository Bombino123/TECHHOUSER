using System;
using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[Flags]
[ComVisible(true)]
public enum HeaderFlags : byte
{
	LockAndRead = 1,
	CaseInsensitive = 8,
	CanonicalizedPaths = 0x10,
	Oplock = 0x20,
	Reply = 0x80
}
