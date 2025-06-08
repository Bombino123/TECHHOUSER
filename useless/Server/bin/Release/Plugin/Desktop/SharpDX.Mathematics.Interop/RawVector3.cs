using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SharpDX.Mathematics.Interop;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
[DebuggerDisplay("X: {X}, Y: {Y}, Z: {Z}")]
public struct RawVector3
{
	public float X;

	public float Y;

	public float Z;

	public RawVector3(float x, float y, float z)
	{
		X = x;
		Y = y;
		Z = z;
	}
}
