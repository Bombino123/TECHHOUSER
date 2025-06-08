using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct RasterizerStateDescription2
{
	public FillMode FillMode;

	public CullMode CullMode;

	public RawBool IsFrontCounterClockwise;

	public int DepthBias;

	public float DepthBiasClamp;

	public float SlopeScaledDepthBias;

	public RawBool IsDepthClipEnabled;

	public RawBool IsScissorEnabled;

	public RawBool IsMultisampleEnabled;

	public RawBool IsAntialiasedLineEnabled;

	public int ForcedSampleCount;

	public ConservativeRasterizationMode ConservativeRasterizationMode;
}
