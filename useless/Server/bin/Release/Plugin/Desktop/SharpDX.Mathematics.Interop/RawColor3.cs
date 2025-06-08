using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SharpDX.Mathematics.Interop;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
[DebuggerDisplay("R: {R}, G: {G}, B: {B}")]
public struct RawColor3
{
	public float R;

	public float G;

	public float B;

	public RawColor3(float r, float g, float b)
	{
		R = r;
		G = g;
		B = b;
	}
}
