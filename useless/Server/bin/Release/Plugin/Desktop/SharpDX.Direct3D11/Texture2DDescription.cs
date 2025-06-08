using System.Runtime.InteropServices;
using SharpDX.DXGI;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct Texture2DDescription
{
	public int Width;

	public int Height;

	public int MipLevels;

	public int ArraySize;

	public Format Format;

	public SampleDescription SampleDescription;

	public ResourceUsage Usage;

	public BindFlags BindFlags;

	public CpuAccessFlags CpuAccessFlags;

	public ResourceOptionFlags OptionFlags;
}
