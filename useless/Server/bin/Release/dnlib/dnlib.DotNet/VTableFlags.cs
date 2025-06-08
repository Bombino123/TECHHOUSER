using System;

namespace dnlib.DotNet;

[Flags]
public enum VTableFlags : ushort
{
	Bit32 = 1,
	Bit64 = 2,
	FromUnmanaged = 4,
	FromUnmanagedRetainAppDomain = 8,
	CallMostDerived = 0x10
}
