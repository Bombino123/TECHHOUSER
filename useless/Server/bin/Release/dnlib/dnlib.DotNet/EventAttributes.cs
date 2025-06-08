using System;

namespace dnlib.DotNet;

[Flags]
public enum EventAttributes : ushort
{
	SpecialName = 0x200,
	RTSpecialName = 0x400
}
