using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public enum OpLockLevel : byte
{
	NoOpLockGranted,
	ExclusiveOpLockGranted,
	BatchOpLockGranted,
	Level2OpLockGranted
}
