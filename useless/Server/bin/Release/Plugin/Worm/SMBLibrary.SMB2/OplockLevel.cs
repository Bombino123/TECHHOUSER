using System;
using System.Runtime.InteropServices;

namespace SMBLibrary.SMB2;

[Flags]
[ComVisible(true)]
public enum OplockLevel : byte
{
	None = 0,
	Level2 = 1,
	Exclusive = 8,
	Batch = 9,
	Lease = byte.MaxValue
}
