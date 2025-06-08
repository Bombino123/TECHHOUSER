using System;
using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[Flags]
[ComVisible(true)]
public enum FindFlags : ushort
{
	SMB_FIND_CLOSE_AFTER_REQUEST = 1,
	SMB_FIND_CLOSE_AT_EOS = 2,
	SMB_FIND_RETURN_RESUME_KEYS = 4,
	SMB_FIND_CONTINUE_FROM_LAST = 8,
	SMB_FIND_WITH_BACKUP_INTENT = 0x10
}
