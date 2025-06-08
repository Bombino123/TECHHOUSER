using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct SubResourceTiling
{
	public int WidthInTiles;

	public short HeightInTiles;

	public short DepthInTiles;

	public int StartTileIndexInOverallResource;
}
