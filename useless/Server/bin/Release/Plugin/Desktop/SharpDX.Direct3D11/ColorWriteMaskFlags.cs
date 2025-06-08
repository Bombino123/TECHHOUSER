using System;

namespace SharpDX.Direct3D11;

[Flags]
public enum ColorWriteMaskFlags : byte
{
	Red = 1,
	Green = 2,
	Blue = 4,
	Alpha = 8,
	All = 0xF
}
