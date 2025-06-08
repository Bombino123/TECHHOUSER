namespace SharpDX.Direct3D11;

public enum VideoProcessorAutoStreamCaps
{
	Denoise = 1,
	Deringing = 2,
	EdgeEnhancement = 4,
	ColorCorrection = 8,
	FleshToneMapping = 0x10,
	ImageStabilization = 0x20,
	SuperResolution = 0x40,
	AnamorphicScaling = 0x80
}
