using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public enum OpenResult : byte
{
	Reserved,
	FileExistedAndWasOpened,
	NotExistedAndWasCreated,
	FileExistedAndWasTruncated
}
