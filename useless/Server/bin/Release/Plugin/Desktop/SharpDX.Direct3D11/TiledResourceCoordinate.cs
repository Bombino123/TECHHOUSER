using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct TiledResourceCoordinate
{
	public int X;

	public int Y;

	public int Z;

	public int Subresource;
}
