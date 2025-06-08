namespace SharpDX.Direct3D11;

public enum VideoProcessorProcessorCaps
{
	DeinterlaceBlend = 1,
	DeinterlaceBob = 2,
	DeinterlaceAdaptive = 4,
	DeinterlaceMotionCompensation = 8,
	InverseTelecine = 0x10,
	FrameRateConversion = 0x20
}
