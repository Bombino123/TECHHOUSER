using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SharpDX.Mathematics.Interop;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
[DebuggerDisplay("X: {X}, Y: {Y}, Z: {Z}, W: {W}")]
public struct RawBool4
{
	public int X;

	public int Y;

	public int Z;

	public int W;
}
