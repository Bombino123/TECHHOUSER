using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
public struct VideoColor
{
	[FieldOffset(0)]
	public VideoColorYCbCrA YCbCr;

	[FieldOffset(0)]
	public VideoColorRgba Rgba;
}
