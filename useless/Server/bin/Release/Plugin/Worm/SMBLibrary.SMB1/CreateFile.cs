using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public enum CreateFile : byte
{
	ReturnErrorIfNotExist,
	CreateIfNotExist
}
