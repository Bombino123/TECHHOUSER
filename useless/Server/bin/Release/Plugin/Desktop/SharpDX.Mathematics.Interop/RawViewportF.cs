using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SharpDX.Mathematics.Interop;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
[DebuggerDisplay("X: {X}, Y: {Y}, Width: {Width}, Height: {Height}, MinDepth: {MinDepth}, MaxDepth: {MaxDepth}")]
public struct RawViewportF
{
	public float X;

	public float Y;

	public float Width;

	public float Height;

	public float MinDepth;

	public float MaxDepth;
}
