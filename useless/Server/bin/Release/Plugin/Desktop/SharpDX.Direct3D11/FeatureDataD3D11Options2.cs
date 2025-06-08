using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct FeatureDataD3D11Options2
{
	public RawBool PSSpecifiedStencilRefSupported;

	public RawBool TypedUAVLoadAdditionalFormats;

	public RawBool ROVsSupported;

	public ConservativeRasterizationTier ConservativeRasterizationTier;

	public TiledResourcesTier TiledResourcesTier;

	public RawBool MapOnDefaultTextures;

	public RawBool StandardSwizzle;

	public RawBool UnifiedMemoryArchitecture;
}
