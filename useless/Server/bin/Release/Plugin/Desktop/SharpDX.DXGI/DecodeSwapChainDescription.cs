using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct DecodeSwapChainDescription
{
	public int Flags;
}
