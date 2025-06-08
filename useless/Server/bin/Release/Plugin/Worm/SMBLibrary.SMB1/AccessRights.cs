using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public enum AccessRights : ushort
{
	SMB_DA_ACCESS_READ,
	SMB_DA_ACCESS_WRITE,
	SMB_DA_ACCESS_READ_WRITE
}
