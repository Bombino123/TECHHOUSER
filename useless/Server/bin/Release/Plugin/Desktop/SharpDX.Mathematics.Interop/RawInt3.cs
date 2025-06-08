using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SharpDX.Mathematics.Interop;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
[DebuggerDisplay("X: {X}, Y: {Y}, Z: {Z}")]
public struct RawInt3
{
	public int X;

	public int Y;

	public int Z;

	public RawInt3(int x, int y, int z)
	{
		X = x;
		Y = y;
		Z = z;
	}
}
