using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct VideoColorRgba
{
	public float R;

	public float G;

	public float B;

	public float A;
}
