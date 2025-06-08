using System.Runtime.InteropServices;
using SharpDX.DXGI;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct VideoSampleDescription
{
	public int Width;

	public int Height;

	public Format Format;

	public ColorSpaceType ColorSpace;
}
