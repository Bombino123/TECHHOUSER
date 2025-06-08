using System.Runtime.InteropServices;

namespace SMBLibrary;

[ComVisible(true)]
public enum CreateDisposition : uint
{
	FILE_SUPERSEDE,
	FILE_OPEN,
	FILE_CREATE,
	FILE_OPEN_IF,
	FILE_OVERWRITE,
	FILE_OVERWRITE_IF
}
