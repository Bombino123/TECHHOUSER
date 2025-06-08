namespace SharpDX.Direct3D11;

public enum VideoDecoderCaps
{
	Downsample = 1,
	NonRealTime = 2,
	DownsampleDynamic = 4,
	DownsampleRequired = 8,
	Unsupported = 0x10
}
