using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct VideoColorYCbCrA
{
	public float Y;

	public float Cb;

	public float Cr;

	public float A;
}
