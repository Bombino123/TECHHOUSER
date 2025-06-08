using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct FeatureDataD3D11Options4
{
	public RawBool ExtendedNV12SharedTextureSupported;
}
