using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[Flags]
[ComVisible(true)]
public enum EventAttributes : ushort
{
	SpecialName = 0x200,
	RTSpecialName = 0x400
}
