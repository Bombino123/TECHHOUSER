using System.Runtime.InteropServices;
using SharpDX.DXGI;
using SharpDX.Direct3D;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
public struct ShaderResourceViewDescription1
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct Texture2DResource1
	{
		public int MostDetailedMip;

		public int MipLevels;

		public int PlaneSlice;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct Texture2DArrayResource1
	{
		public int MostDetailedMip;

		public int MipLevels;

		public int FirstArraySlice;

		public int ArraySize;

		public int PlaneSlice;
	}

	[FieldOffset(0)]
	public Format Format;

	[FieldOffset(4)]
	public ShaderResourceViewDimension Dimension;

	[FieldOffset(8)]
	public ShaderResourceViewDescription.BufferResource Buffer;

	[FieldOffset(8)]
	public ShaderResourceViewDescription.Texture1DResource Texture1D;

	[FieldOffset(8)]
	public ShaderResourceViewDescription.Texture1DArrayResource Texture1DArray;

	[FieldOffset(8)]
	public Texture2DResource1 Texture2D;

	[FieldOffset(8)]
	public Texture2DArrayResource1 Texture2DArray;

	[FieldOffset(8)]
	public ShaderResourceViewDescription.Texture2DMultisampledResource Texture2DMS;

	[FieldOffset(8)]
	public ShaderResourceViewDescription.Texture2DMultisampledArrayResource Texture2DMSArray;

	[FieldOffset(8)]
	public ShaderResourceViewDescription.Texture3DResource Texture3D;

	[FieldOffset(8)]
	public ShaderResourceViewDescription.TextureCubeResource TextureCube;

	[FieldOffset(8)]
	public ShaderResourceViewDescription.TextureCubeArrayResource TextureCubeArray;

	[FieldOffset(8)]
	public ShaderResourceViewDescription.ExtendedBufferResource BufferEx;
}
