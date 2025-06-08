using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SharpDX.Mathematics.Interop;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
[DebuggerDisplay("X: {X}, Y: {Y}")]
public struct RawVector2
{
	public float X;

	public float Y;

	public RawVector2(float x, float y)
	{
		X = x;
		Y = y;
	}
}
