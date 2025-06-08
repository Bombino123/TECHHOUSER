using System.Runtime.InteropServices;
using SharpDX.DXGI;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
public struct DepthStencilViewDescription
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct Texture1DResource
	{
		public int MipSlice;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct Texture1DArrayResource
	{
		public int MipSlice;

		public int FirstArraySlice;

		public int ArraySize;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct Texture2DResource
	{
		public int MipSlice;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct Texture2DArrayResource
	{
		public int MipSlice;

		public int FirstArraySlice;

		public int ArraySize;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct Texture2DMultisampledResource
	{
		public int UnusedFieldNothingToDefine;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct Texture2DMultisampledArrayResource
	{
		public int FirstArraySlice;

		public int ArraySize;
	}

	[FieldOffset(0)]
	public Format Format;

	[FieldOffset(4)]
	public DepthStencilViewDimension Dimension;

	[FieldOffset(8)]
	public DepthStencilViewFlags Flags;

	[FieldOffset(12)]
	public Texture1DResource Texture1D;

	[FieldOffset(12)]
	public Texture1DArrayResource Texture1DArray;

	[FieldOffset(12)]
	public Texture2DResource Texture2D;

	[FieldOffset(12)]
	public Texture2DArrayResource Texture2DArray;

	[FieldOffset(12)]
	public Texture2DMultisampledResource Texture2DMS;

	[FieldOffset(12)]
	public Texture2DMultisampledArrayResource Texture2DMSArray;
}
