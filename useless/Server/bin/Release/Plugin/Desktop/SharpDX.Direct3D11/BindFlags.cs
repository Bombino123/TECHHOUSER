using System;

namespace SharpDX.Direct3D11;

[Flags]
public enum BindFlags
{
	VertexBuffer = 1,
	IndexBuffer = 2,
	ConstantBuffer = 4,
	ShaderResource = 8,
	StreamOutput = 0x10,
	RenderTarget = 0x20,
	DepthStencil = 0x40,
	UnorderedAccess = 0x80,
	Decoder = 0x200,
	VideoEncoder = 0x400,
	None = 0
}
