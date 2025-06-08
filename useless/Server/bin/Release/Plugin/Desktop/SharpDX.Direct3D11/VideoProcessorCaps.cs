using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct VideoProcessorCaps
{
	public int DeviceCaps;

	public int FeatureCaps;

	public int FilterCaps;

	public int InputFormatCaps;

	public int AutoStreamCaps;

	public int StereoCaps;

	public int RateConversionCapsCount;

	public int MaxInputStreams;

	public int MaxStreamStates;
}
