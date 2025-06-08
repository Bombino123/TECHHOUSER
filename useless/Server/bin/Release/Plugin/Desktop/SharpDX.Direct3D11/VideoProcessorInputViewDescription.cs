using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct VideoProcessorInputViewDescription
{
	public int FourCC;

	public VpivDimension Dimension;

	public Texture2DVpiv Texture2D;
}
