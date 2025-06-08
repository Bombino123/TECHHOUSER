using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct FeatureDataShaderMinimumPrecisionSupport
{
	public int PixelShaderMinPrecision;

	public int AllOtherShaderStagesMinPrecision;
}
