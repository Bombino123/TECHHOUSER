using System;
using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[Flags]
[ComVisible(true)]
public enum WriteMode : ushort
{
	WritethroughMode = 1,
	ReadBytesAvailable = 2,
	RAW_MODE = 4,
	MSG_START = 8
}
