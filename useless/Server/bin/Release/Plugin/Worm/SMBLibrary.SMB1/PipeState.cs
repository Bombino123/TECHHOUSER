using System;
using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[Flags]
[ComVisible(true)]
public enum PipeState : ushort
{
	ReadMode = 0x100,
	Nonblocking = 0x8000
}
