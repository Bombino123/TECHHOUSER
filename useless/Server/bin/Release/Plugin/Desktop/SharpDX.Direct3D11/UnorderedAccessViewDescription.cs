using System.Runtime.InteropServices;
using SharpDX.DXGI;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
public struct UnorderedAccessViewDescription
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct BufferResource
	{
		public int FirstElement;

		public int ElementCount;

		public UnorderedAccessViewBufferFlags Flags;
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
	public struct Texture2DArrayResource
	{
		public int MipSlice;

		public int FirstArraySlice;

		public int ArraySize;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct Texture3DResource
	{
		public int MipSlice;

		public int FirstWSlice;

		public int WSize;
	}

	[FieldOffset(0)]
	public Format Format;

	[FieldOffset(4)]
	public UnorderedAccessViewDimension Dimension;

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
	public Texture3DResource Texture3D;
}
