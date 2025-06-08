using System;
using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[Flags]
[ComVisible(true)]
public enum LockType : byte
{
	READ_WRITE_LOCK = 0,
	SHARED_LOCK = 1,
	OPLOCK_RELEASE = 2,
	CHANGE_LOCKTYPE = 4,
	CANCEL_LOCK = 8,
	LARGE_FILES = 0x10
}
