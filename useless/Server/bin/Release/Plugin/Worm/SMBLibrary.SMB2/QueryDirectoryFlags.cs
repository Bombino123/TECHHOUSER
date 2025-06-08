using System;
using System.Runtime.InteropServices;

namespace SMBLibrary.SMB2;

[Flags]
[ComVisible(true)]
public enum QueryDirectoryFlags : byte
{
	SMB2_RESTART_SCANS = 1,
	SMB2_RETURN_SINGLE_ENTRY = 2,
	SMB2_INDEX_SPECIFIED = 4,
	SMB2_REOPEN = 0x10
}
