using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct PackedMipDescription
{
	public byte StandardMipCount;

	public byte PackedMipCount;

	public int TilesForPackedMipCount;

	public int StartTileIndexInOverallResource;
}
