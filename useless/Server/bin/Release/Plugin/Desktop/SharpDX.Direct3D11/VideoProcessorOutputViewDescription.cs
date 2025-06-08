using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
public struct VideoProcessorOutputViewDescription
{
	[FieldOffset(0)]
	public VpovDimension Dimension;

	[FieldOffset(4)]
	public Texture2DVpov Texture2D;

	[FieldOffset(4)]
	public Texture2DArrayVpov Texture2DArray;
}
