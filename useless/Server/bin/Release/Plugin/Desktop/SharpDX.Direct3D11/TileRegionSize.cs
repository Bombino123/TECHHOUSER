using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct TileRegionSize
{
	public int TileCount;

	public RawBool BUseBox;

	public int Width;

	public short Height;

	public short Depth;
}
