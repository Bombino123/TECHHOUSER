using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public enum QueryFSInformationLevel : ushort
{
	SMB_INFO_ALLOCATION = 1,
	SMB_INFO_VOLUME = 2,
	SMB_QUERY_FS_VOLUME_INFO = 258,
	SMB_QUERY_FS_SIZE_INFO = 259,
	SMB_QUERY_FS_DEVICE_INFO = 260,
	SMB_QUERY_FS_ATTRIBUTE_INFO = 261
}
