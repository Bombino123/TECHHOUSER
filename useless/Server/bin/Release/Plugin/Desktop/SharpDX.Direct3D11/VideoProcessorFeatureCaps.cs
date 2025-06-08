namespace SharpDX.Direct3D11;

public enum VideoProcessorFeatureCaps
{
	AlphaFill = 1,
	Constriction = 2,
	LumaKey = 4,
	AlphaPalette = 8,
	Legacy = 0x10,
	Stereo = 0x20,
	Rotation = 0x40,
	AlphaStream = 0x80,
	PixelAspectRatio = 0x100,
	Mirror = 0x200,
	ShaderUsage = 0x400,
	MetadataHdr10 = 0x800
}
