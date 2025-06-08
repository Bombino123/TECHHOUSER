using System;

namespace SharpDX.Direct3D11;

[Flags]
public enum TileRangeFlags
{
	Null = 1,
	Skip = 2,
	ReuseSingleTile = 4,
	None = 0
}
