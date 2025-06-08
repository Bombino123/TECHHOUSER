using System;
using System.Runtime.InteropServices;

namespace SMBLibrary.SMB2;

[Flags]
[ComVisible(true)]
public enum LockFlags : uint
{
	SharedLock = 1u,
	ExclusiveLock = 2u,
	Unlock = 4u,
	FailImmediately = 8u
}
