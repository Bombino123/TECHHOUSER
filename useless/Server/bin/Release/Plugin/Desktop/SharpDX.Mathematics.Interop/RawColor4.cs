using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SharpDX.Mathematics.Interop;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
[DebuggerDisplay("R:{R} G:{G} B:{B} A:{A}")]
public struct RawColor4
{
	public float R;

	public float G;

	public float B;

	public float A;

	public RawColor4(float r, float g, float b, float a)
	{
		R = r;
		G = g;
		B = b;
		A = a;
	}
}
