using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct FeatureDataShaderCache
{
	public int SupportFlags;
}
