using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public enum SetInformationLevel : ushort
{
	SMB_INFO_STANDARD = 1,
	SMB_INFO_SET_EAS = 2,
	SMB_SET_FILE_BASIC_INFO = 257,
	SMB_SET_FILE_DISPOSITION_INFO = 258,
	SMB_SET_FILE_ALLOCATION_INFO = 259,
	SMB_SET_FILE_END_OF_FILE_INFO = 260
}
