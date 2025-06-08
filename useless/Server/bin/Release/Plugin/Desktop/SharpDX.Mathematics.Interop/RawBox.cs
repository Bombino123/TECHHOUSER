using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SharpDX.Mathematics.Interop;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
[DebuggerDisplay("X: {X}, Y: {Y}, Width: {Width}, Height: {Height}")]
public struct RawBox
{
	public int X;

	public int Y;

	public int Width;

	public int Height;

	public RawBox(int x, int y, int width, int height)
	{
		X = x;
		Y = y;
		Width = width;
		Height = height;
	}
}
