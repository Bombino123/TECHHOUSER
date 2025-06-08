using System;
using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[Flags]
[ComVisible(true)]
public enum SMBFileAttributes : ushort
{
	Normal = 0,
	ReadOnly = 1,
	Hidden = 2,
	System = 4,
	Volume = 8,
	Directory = 0x10,
	Archive = 0x20,
	SearchReadOnly = 0x100,
	SearchHidden = 0x200,
	SearchSystem = 0x400,
	SearchDirectory = 0x1000,
	SearchArchive = 0x2000
}
