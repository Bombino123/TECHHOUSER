using System;

namespace SharpDX.Direct3D11;

[Flags]
public enum TileCopyFlags
{
	NoOverwrite = 1,
	LinearBufferToSwizzledTiledResource = 2,
	SwizzledTiledResourceToLinearBuffer = 4,
	None = 0
}
