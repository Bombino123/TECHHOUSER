using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct RenderTargetBlendDescription
{
	public RawBool IsBlendEnabled;

	public BlendOption SourceBlend;

	public BlendOption DestinationBlend;

	public BlendOperation BlendOperation;

	public BlendOption SourceAlphaBlend;

	public BlendOption DestinationAlphaBlend;

	public BlendOperation AlphaBlendOperation;

	public ColorWriteMaskFlags RenderTargetWriteMask;

	public RenderTargetBlendDescription(bool isBlendEnabled, BlendOption sourceBlend, BlendOption destinationBlend, BlendOperation blendOperation, BlendOption sourceAlphaBlend, BlendOption destinationAlphaBlend, BlendOperation alphaBlendOperation, ColorWriteMaskFlags renderTargetWriteMask)
	{
		IsBlendEnabled = isBlendEnabled;
		SourceBlend = sourceBlend;
		DestinationBlend = destinationBlend;
		BlendOperation = blendOperation;
		SourceAlphaBlend = sourceAlphaBlend;
		DestinationAlphaBlend = destinationAlphaBlend;
		AlphaBlendOperation = alphaBlendOperation;
		RenderTargetWriteMask = renderTargetWriteMask;
	}

	public override string ToString()
	{
		return $"IsBlendEnabled: {IsBlendEnabled}, SourceBlend: {SourceBlend}, DestinationBlend: {DestinationBlend}, BlendOperation: {BlendOperation}, SourceAlphaBlend: {SourceAlphaBlend}, DestinationAlphaBlend: {DestinationAlphaBlend}, AlphaBlendOperation: {AlphaBlendOperation}, RenderTargetWriteMask: {RenderTargetWriteMask}";
	}
}
