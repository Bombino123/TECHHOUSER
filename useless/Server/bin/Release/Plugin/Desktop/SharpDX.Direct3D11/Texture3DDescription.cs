using System.Runtime.InteropServices;
using SharpDX.DXGI;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct Texture3DDescription
{
	public int Width;

	public int Height;

	public int Depth;

	public int MipLevels;

	public Format Format;

	public ResourceUsage Usage;

	public BindFlags BindFlags;

	public CpuAccessFlags CpuAccessFlags;

	public ResourceOptionFlags OptionFlags;
}
