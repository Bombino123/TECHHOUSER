using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public enum SearchStorageType : uint
{
	FILE_DIRECTORY_FILE = 1u,
	FILE_NON_DIRECTORY_FILE = 0x40u
}
