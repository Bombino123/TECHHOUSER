using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct TileShape
{
	public int WidthInTexels;

	public int HeightInTexels;

	public int DepthInTexels;
}
