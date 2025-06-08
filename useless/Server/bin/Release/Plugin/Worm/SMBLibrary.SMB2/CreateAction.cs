using System.Runtime.InteropServices;

namespace SMBLibrary.SMB2;

[ComVisible(true)]
public enum CreateAction : uint
{
	FILE_SUPERSEDED,
	FILE_OPENED,
	FILE_CREATED,
	FILE_OVERWRITTEN
}
