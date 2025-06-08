using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct RenderTargetBlendDescription1
{
	public RawBool IsBlendEnabled;

	public RawBool IsLogicOperationEnabled;

	public BlendOption SourceBlend;

	public BlendOption DestinationBlend;

	public BlendOperation BlendOperation;

	public BlendOption SourceAlphaBlend;

	public BlendOption DestinationAlphaBlend;

	public BlendOperation AlphaBlendOperation;

	public LogicOperation LogicOperation;

	public ColorWriteMaskFlags RenderTargetWriteMask;
}
