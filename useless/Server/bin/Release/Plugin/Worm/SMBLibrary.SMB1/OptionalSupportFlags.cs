using System;
using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[Flags]
[ComVisible(true)]
public enum OptionalSupportFlags : ushort
{
	SMB_SUPPORT_SEARCH_BITS = 1,
	SMB_SHARE_IS_IN_DFS = 2,
	SMB_CSC_CACHE_MANUAL_REINT = 0,
	SMB_CSC_CACHE_AUTO_REINT = 4,
	SMB_CSC_CACHE_VDO = 8,
	SMB_CSC_NO_CACHING = 0xC,
	SMB_UNIQUE_FILE_NAME = 0x10,
	SMB_EXTENDED_SIGNATURES = 0x20
}
