using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct FeatureDataD3D11Options
{
	public RawBool OutputMergerLogicOp;

	public RawBool UAVOnlyRenderingForcedSampleCount;

	public RawBool DiscardAPIsSeenByDriver;

	public RawBool FlagsForUpdateAndCopySeenByDriver;

	public RawBool ClearView;

	public RawBool CopyWithOverlap;

	public RawBool ConstantBufferPartialUpdate;

	public RawBool ConstantBufferOffsetting;

	public RawBool MapNoOverwriteOnDynamicConstantBuffer;

	public RawBool MapNoOverwriteOnDynamicBufferSRV;

	public RawBool MultisampleRTVWithForcedSampleCountOne;

	public RawBool SAD4ShaderInstructions;

	public RawBool ExtendedDoublesShaderInstructions;

	public RawBool ExtendedResourceSharing;
}
