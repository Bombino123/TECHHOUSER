using System.Runtime.InteropServices;

namespace SMBLibrary;

[ComVisible(true)]
public enum FileStatus : uint
{
	FILE_SUPERSEDED,
	FILE_OPENED,
	FILE_CREATED,
	FILE_OVERWRITTEN,
	FILE_EXISTS,
	FILE_DOES_NOT_EXIST
}
