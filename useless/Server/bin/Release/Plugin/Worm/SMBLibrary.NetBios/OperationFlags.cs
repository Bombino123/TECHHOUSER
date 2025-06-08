using System;
using System.Runtime.InteropServices;

namespace SMBLibrary.NetBios;

[Flags]
[ComVisible(true)]
public enum OperationFlags : byte
{
	Broadcast = 1,
	RecursionAvailable = 8,
	RecursionDesired = 0x10,
	Truncated = 0x20,
	AuthoritativeAnswer = 0x40
}
