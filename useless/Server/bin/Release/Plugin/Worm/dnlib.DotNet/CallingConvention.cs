using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[Flags]
[ComVisible(true)]
public enum CallingConvention : byte
{
	Default = 0,
	C = 1,
	StdCall = 2,
	ThisCall = 3,
	FastCall = 4,
	VarArg = 5,
	Field = 6,
	LocalSig = 7,
	Property = 8,
	Unmanaged = 9,
	GenericInst = 0xA,
	NativeVarArg = 0xB,
	Mask = 0xF,
	Generic = 0x10,
	HasThis = 0x20,
	ExplicitThis = 0x40,
	ReservedByCLR = 0x80
}
