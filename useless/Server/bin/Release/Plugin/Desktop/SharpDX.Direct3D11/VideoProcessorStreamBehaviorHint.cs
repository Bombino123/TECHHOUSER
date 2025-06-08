using System.Runtime.InteropServices;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct VideoProcessorStreamBehaviorHint
{
	public RawBool Enable;

	public int Width;

	public int Height;

	public Format Format;
}
