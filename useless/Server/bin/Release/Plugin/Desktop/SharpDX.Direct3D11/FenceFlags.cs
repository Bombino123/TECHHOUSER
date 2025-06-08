using System;

namespace SharpDX.Direct3D11;

[Flags]
public enum FenceFlags
{
	None = 1,
	Shared = 2,
	SharedCrossAdapter = 4
}
