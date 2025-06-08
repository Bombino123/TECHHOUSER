using System.Diagnostics;

namespace SharpDX.Mathematics.Interop;

[DebuggerDisplay("X: {X}, Y: {Y}")]
public struct RawPoint
{
	public int X;

	public int Y;

	public RawPoint(int x, int y)
	{
		X = x;
		Y = y;
	}
}
