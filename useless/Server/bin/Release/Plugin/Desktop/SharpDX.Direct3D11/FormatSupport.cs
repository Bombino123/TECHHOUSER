using System;

namespace SharpDX.Direct3D11;

[Flags]
public enum FormatSupport
{
	Buffer = 1,
	InputAssemblyVertexBuffer = 2,
	InputAssemblyIndexBuffer = 4,
	StreamOutputBuffer = 8,
	Texture1D = 0x10,
	Texture2D = 0x20,
	Texture3D = 0x40,
	TextureCube = 0x80,
	ShaderLoad = 0x100,
	ShaderSample = 0x200,
	ShaderSampleComparison = 0x400,
	ShaderSampleMonoText = 0x800,
	Mip = 0x1000,
	MipAutogen = 0x2000,
	RenderTarget = 0x4000,
	Blendable = 0x8000,
	DepthStencil = 0x10000,
	CpuLockable = 0x20000,
	MultisampleResolve = 0x40000,
	Display = 0x80000,
	CastWithinBitLayout = 0x100000,
	MultisampleRenderTarget = 0x200000,
	MultisampleLoad = 0x400000,
	ShaderGather = 0x800000,
	BackBufferCast = 0x1000000,
	TypedUnorderedAccessView = 0x2000000,
	ShaderGatherComparison = 0x4000000,
	DecoderOutput = 0x8000000,
	VideoProcessorOutput = 0x10000000,
	VideoProcessorInput = 0x20000000,
	VideoEncoder = 0x40000000,
	None = 0
}
