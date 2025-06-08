using System;

namespace dnlib.DotNet;

[Flags]
public enum ParamAttributes : ushort
{
	In = 1,
	Out = 2,
	Lcid = 4,
	Retval = 8,
	Optional = 0x10,
	HasDefault = 0x1000,
	HasFieldMarshal = 0x2000
}
