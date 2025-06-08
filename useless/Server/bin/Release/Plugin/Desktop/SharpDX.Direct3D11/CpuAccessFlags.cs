using System;

namespace SharpDX.Direct3D11;

[Flags]
public enum CpuAccessFlags
{
	Write = 0x10000,
	Read = 0x20000,
	None = 0
}
