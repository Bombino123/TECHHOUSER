using System.Runtime.InteropServices;

namespace SMBLibrary.SMB2;

[ComVisible(true)]
public enum InfoType : byte
{
	File = 1,
	FileSystem,
	Security,
	Quota
}
