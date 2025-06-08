using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DXGI;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct SwapChainFullScreenDescription
{
	public Rational RefreshRate;

	public DisplayModeScanlineOrder ScanlineOrdering;

	public DisplayModeScaling Scaling;

	public RawBool Windowed;
}
