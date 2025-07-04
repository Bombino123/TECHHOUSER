using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public enum ElementType : byte
{
	End = 0,
	Void = 1,
	Boolean = 2,
	Char = 3,
	I1 = 4,
	U1 = 5,
	I2 = 6,
	U2 = 7,
	I4 = 8,
	U4 = 9,
	I8 = 10,
	U8 = 11,
	R4 = 12,
	R8 = 13,
	String = 14,
	Ptr = 15,
	ByRef = 16,
	ValueType = 17,
	Class = 18,
	Var = 19,
	Array = 20,
	GenericInst = 21,
	TypedByRef = 22,
	ValueArray = 23,
	I = 24,
	U = 25,
	R = 26,
	FnPtr = 27,
	Object = 28,
	SZArray = 29,
	MVar = 30,
	CModReqd = 31,
	CModOpt = 32,
	Internal = 33,
	Module = 63,
	Sentinel = 65,
	Pinned = 69
}
