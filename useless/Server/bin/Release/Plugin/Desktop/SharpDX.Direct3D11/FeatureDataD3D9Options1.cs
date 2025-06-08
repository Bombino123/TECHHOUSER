using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct FeatureDataD3D9Options1
{
	public RawBool FullNonPow2TextureSupported;

	public RawBool DepthAsTextureWithLessEqualComparisonFilterSupported;

	public RawBool SimpleInstancingSupported;

	public RawBool TextureCubeFaceRenderTargetWithNonCubeDepthStencilSupported;
}
