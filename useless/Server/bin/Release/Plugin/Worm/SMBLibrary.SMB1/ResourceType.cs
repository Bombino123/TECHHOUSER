using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public enum ResourceType : ushort
{
	FileTypeDisk = 0,
	FileTypeByteModePipe = 1,
	FileTypeMessageModePipe = 2,
	FileTypePrinter = 3,
	FileTypeCommDevice = 4,
	FileTypeUnknown = ushort.MaxValue
}
