using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SharpDX.Mathematics.Interop;

[StructLayout(LayoutKind.Sequential, Size = 4)]
[DebuggerDisplay("R:{R} G:{G} B:{B} A:{A}")]
public struct RawColorBGRA
{
	public byte B;

	public byte G;

	public byte R;

	public byte A;

	public RawColorBGRA(byte b, byte g, byte r, byte a)
	{
		B = b;
		G = g;
		R = r;
		A = a;
	}
}
