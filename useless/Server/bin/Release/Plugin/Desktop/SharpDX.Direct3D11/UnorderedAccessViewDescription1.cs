using System.Runtime.InteropServices;
using SharpDX.DXGI;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
public struct UnorderedAccessViewDescription1
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct Texture2DResource
	{
		public int MipSlice;

		public int PlaneSlice;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct Texture2DArrayResource
	{
		public int MipSlice;

		public int FirstArraySlice;

		public int ArraySize;

		public int PlaneSlice;
	}

	[FieldOffset(0)]
	public Format Format;

	[FieldOffset(4)]
	public UnorderedAccessViewDimension Dimension;

	[FieldOffset(8)]
	public UnorderedAccessViewDescription.BufferResource Buffer;

	[FieldOffset(8)]
	public UnorderedAccessViewDescription.Texture1DResource Texture1D;

	[FieldOffset(8)]
	public UnorderedAccessViewDescription.Texture1DArrayResource Texture1DArray;

	[FieldOffset(8)]
	public Texture2DResource Texture2D;

	[FieldOffset(8)]
	public Texture2DArrayResource Texture2DArray;

	[FieldOffset(8)]
	public UnorderedAccessViewDescription.Texture3DResource Texture3D;
}
