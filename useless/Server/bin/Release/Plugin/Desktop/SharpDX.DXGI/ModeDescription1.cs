using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DXGI;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct ModeDescription1
{
	public int Width;

	public int Height;

	public Rational RefreshRate;

	public Format Format;

	public DisplayModeScanlineOrder ScanlineOrdering;

	public DisplayModeScaling Scaling;

	public RawBool Stereo;
}
