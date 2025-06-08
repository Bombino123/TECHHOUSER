using System;

namespace dnlib.DotNet;

[Flags]
public enum PropertyAttributes : ushort
{
	SpecialName = 0x200,
	RTSpecialName = 0x400,
	HasDefault = 0x1000
}
