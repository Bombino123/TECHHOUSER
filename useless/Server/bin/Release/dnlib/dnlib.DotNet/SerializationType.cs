namespace dnlib.DotNet;

internal enum SerializationType : byte
{
	Undefined = 0,
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
	SZArray = 29,
	Type = 80,
	TaggedObject = 81,
	Field = 83,
	Property = 84,
	Enum = 85
}
