using System;
using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[Flags]
[ComVisible(true)]
public enum OpenFlags : ushort
{
	REQ_ATTRIB = 1,
	REQ_OPLOCK = 2,
	REQ_OPLOCK_BATCH = 4,
	SMB_OPEN_EXTENDED_RESPONSE = 0x10
}
