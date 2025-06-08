using System.Runtime.InteropServices;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct VideoProcessorCustomRate
{
	public Rational CustomRate;

	public int OutputFrames;

	public RawBool InputInterlaced;

	public int InputFramesOrFields;
}
