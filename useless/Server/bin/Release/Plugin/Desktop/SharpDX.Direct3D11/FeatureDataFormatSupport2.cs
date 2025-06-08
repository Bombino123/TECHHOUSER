using System.Runtime.InteropServices;
using SharpDX.DXGI;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct FeatureDataFormatSupport2
{
	public Format InFormat;

	public ComputeShaderFormatSupport OutFormatSupport2;
}
