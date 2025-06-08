namespace SharpDX.Direct3D11;

public enum VideoProcessorFilterCaps
{
	Brightness = 1,
	Contrast = 2,
	Hue = 4,
	Saturation = 8,
	NoiseReduction = 0x10,
	EdgeEnhancement = 0x20,
	AnamorphicScaling = 0x40,
	StereoAdjustment = 0x80
}
