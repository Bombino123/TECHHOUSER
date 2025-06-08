using System.Runtime.InteropServices;
using SharpDX.DXGI;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct FeatureDataFormatSupport
{
	public Format InFormat;

	public FormatSupport OutFormatSupport;
}
