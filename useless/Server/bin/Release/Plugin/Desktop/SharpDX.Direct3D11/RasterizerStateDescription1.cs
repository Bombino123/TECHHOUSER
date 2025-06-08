using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct RasterizerStateDescription1
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

	public static RasterizerStateDescription1 Default()
	{
		RasterizerStateDescription1 result = default(RasterizerStateDescription1);
		result.FillMode = FillMode.Solid;
		result.CullMode = CullMode.Back;
		result.IsFrontCounterClockwise = false;
		result.DepthBias = 0;
		result.SlopeScaledDepthBias = 0f;
		result.DepthBiasClamp = 0f;
		result.IsDepthClipEnabled = true;
		result.IsScissorEnabled = false;
		result.IsMultisampleEnabled = false;
		result.IsAntialiasedLineEnabled = false;
		result.ForcedSampleCount = 0;
		return result;
	}
}
