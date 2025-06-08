using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SharpDX.Mathematics.Interop;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
[DebuggerDisplay("X: {X}, Y: {Y}, Z: {Z}, W: {W}")]
public struct RawInt4
{
	public int X;

	public int Y;

	public int Z;

	public int W;

	public RawInt4(int x, int y, int z, int w)
	{
		X = x;
		Y = y;
		Z = z;
		W = w;
	}
}
