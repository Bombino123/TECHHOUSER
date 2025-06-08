using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DXGI;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct SwapChainDescription1
{
	public int Width;

	public int Height;

	public Format Format;

	public RawBool Stereo;

	public SampleDescription SampleDescription;

	public Usage Usage;

	public int BufferCount;

	public Scaling Scaling;

	public SwapEffect SwapEffect;

	public AlphaMode AlphaMode;

	public SwapChainFlags Flags;
}
