using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct SamplerStateDescription
{
	public Filter Filter;

	public TextureAddressMode AddressU;

	public TextureAddressMode AddressV;

	public TextureAddressMode AddressW;

	public float MipLodBias;

	public int MaximumAnisotropy;

	public Comparison ComparisonFunction;

	public RawColor4 BorderColor;

	public float MinimumLod;

	public float MaximumLod;

	public static SamplerStateDescription Default()
	{
		SamplerStateDescription result = default(SamplerStateDescription);
		result.Filter = Filter.MinMagMipLinear;
		result.AddressU = TextureAddressMode.Clamp;
		result.AddressV = TextureAddressMode.Clamp;
		result.AddressW = TextureAddressMode.Clamp;
		result.MinimumLod = float.MinValue;
		result.MaximumLod = float.MaxValue;
		result.MipLodBias = 0f;
		result.MaximumAnisotropy = 16;
		result.ComparisonFunction = Comparison.Never;
		result.BorderColor = default(RawColor4);
		return result;
	}
}
