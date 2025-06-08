using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[Flags]
[ComVisible(true)]
public enum PropertyAttributes : ushort
{
	SpecialName = 0x200,
	RTSpecialName = 0x400,
	HasDefault = 0x1000
}
