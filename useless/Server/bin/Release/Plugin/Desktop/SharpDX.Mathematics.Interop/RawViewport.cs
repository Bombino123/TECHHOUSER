using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SharpDX.Mathematics.Interop;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
[DebuggerDisplay("X: {X}, Y: {Y}, Width: {Width}, Height: {Height}, MinDepth: {MinDepth}, MaxDepth: {MaxDepth}")]
public struct RawViewport
{
	public int X;

	public int Y;

	public int Width;

	public int Height;

	public float MinDepth;

	public float MaxDepth;
}
