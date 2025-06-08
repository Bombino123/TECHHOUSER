using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct VideoProcessorRateConversionCaps
{
	public int PastFrames;

	public int FutureFrames;

	public int ProcessorCaps;

	public int ITelecineCaps;

	public int CustomRateCount;
}
