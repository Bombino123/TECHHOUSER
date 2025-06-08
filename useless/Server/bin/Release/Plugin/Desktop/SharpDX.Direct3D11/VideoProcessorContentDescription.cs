using System.Runtime.InteropServices;
using SharpDX.DXGI;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct VideoProcessorContentDescription
{
	public VideoFrameFormat InputFrameFormat;

	public Rational InputFrameRate;

	public int InputWidth;

	public int InputHeight;

	public Rational OutputFrameRate;

	public int OutputWidth;

	public int OutputHeight;

	public VideoUsage Usage;
}
