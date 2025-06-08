using System;

namespace dnlib.PE;

[Flags]
public enum DllCharacteristics : ushort
{
	Reserved1 = 1,
	Reserved2 = 2,
	Reserved3 = 4,
	Reserved4 = 8,
	Reserved5 = 0x10,
	HighEntropyVA = 0x20,
	DynamicBase = 0x40,
	ForceIntegrity = 0x80,
	NxCompat = 0x100,
	NoIsolation = 0x200,
	NoSeh = 0x400,
	NoBind = 0x800,
	AppContainer = 0x1000,
	WdmDriver = 0x2000,
	GuardCf = 0x4000,
	TerminalServerAware = 0x8000
}
