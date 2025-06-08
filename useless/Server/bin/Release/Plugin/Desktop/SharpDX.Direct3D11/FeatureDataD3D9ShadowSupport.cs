using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct FeatureDataD3D9ShadowSupport
{
	public RawBool SupportsDepthAsTextureWithLessEqualComparisonFilter;
}
