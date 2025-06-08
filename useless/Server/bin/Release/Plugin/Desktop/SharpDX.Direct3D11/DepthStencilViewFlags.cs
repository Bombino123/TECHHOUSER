using System;

namespace SharpDX.Direct3D11;

[Flags]
public enum DepthStencilViewFlags
{
	ReadOnlyDepth = 1,
	ReadOnlyStencil = 2,
	None = 0
}
