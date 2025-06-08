using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct Texture2DArrayVpov
{
	public int MipSlice;

	public int FirstArraySlice;

	public int ArraySize;
}
