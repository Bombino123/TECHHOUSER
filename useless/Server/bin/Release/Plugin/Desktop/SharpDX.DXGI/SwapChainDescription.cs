using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DXGI;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct SwapChainDescription
{
	public ModeDescription ModeDescription;

	public SampleDescription SampleDescription;

	public Usage Usage;

	public int BufferCount;

	public IntPtr OutputHandle;

	public RawBool IsWindowed;

	public SwapEffect SwapEffect;

	public SwapChainFlags Flags;
}
