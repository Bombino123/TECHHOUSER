using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct SurfaceDescription
{
	public int Width;

	public int Height;

	public Format Format;

	public SampleDescription SampleDescription;
}
