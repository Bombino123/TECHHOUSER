using System.Runtime.InteropServices;
using SharpDX.DXGI;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
public struct RenderTargetViewDescription
{
	[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
	public struct BufferResource
	{
		[FieldOffset(0)]
		public int FirstElement;

		[FieldOffset(0)]
		public int ElementOffset;

		[FieldOffset(4)]
		public int ElementCount;

		[FieldOffset(4)]
		public int ElementWidth;
	}

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
	public struct Texture2DMultisampledResource
	{
		public int UnusedFieldNothingToDefine;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct Texture2DArrayResource
	{
		public int MipSlice;

		public int FirstArraySlice;

		public int ArraySize;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct Texture2DMultisampledArrayResource
	{
		public int FirstArraySlice;

		public int ArraySize;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct Texture3DResource
	{
		public int MipSlice;

		public int FirstDepthSlice;

		public int DepthSliceCount;
	}

	[FieldOffset(0)]
	public Format Format;

	[FieldOffset(4)]
	public RenderTargetViewDimension Dimension;

	[FieldOffset(8)]
	public BufferResource Buffer;

	[FieldOffset(8)]
	public Texture1DResource Texture1D;

	[FieldOffset(8)]
	public Texture1DArrayResource Texture1DArray;

	[FieldOffset(8)]
	public Texture2DResource Texture2D;

	[FieldOffset(8)]
	public Texture2DArrayResource Texture2DArray;

	[FieldOffset(8)]
	public Texture2DMultisampledResource Texture2DMS;

	[FieldOffset(8)]
	public Texture2DMultisampledArrayResource Texture2DMSArray;

	[FieldOffset(8)]
	public Texture3DResource Texture3D;
}
