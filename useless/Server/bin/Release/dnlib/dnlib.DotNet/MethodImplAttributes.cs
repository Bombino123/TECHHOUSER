using System;

namespace dnlib.DotNet;

[Flags]
public enum MethodImplAttributes : ushort
{
	CodeTypeMask = 3,
	IL = 0,
	Native = 1,
	OPTIL = 2,
	Runtime = 3,
	ManagedMask = 4,
	Unmanaged = 4,
	Managed = 0,
	ForwardRef = 0x10,
	PreserveSig = 0x80,
	InternalCall = 0x1000,
	Synchronized = 0x20,
	NoInlining = 8,
	AggressiveInlining = 0x100,
	NoOptimization = 0x40,
	AggressiveOptimization = 0x200,
	SecurityMitigations = 0x400
}
