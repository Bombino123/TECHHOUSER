using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct ModeDescription
{
	public int Width;

	public int Height;

	public Rational RefreshRate;

	public Format Format;

	public DisplayModeScanlineOrder ScanlineOrdering;

	public DisplayModeScaling Scaling;

	public ModeDescription(int width, int height, Rational refreshRate, Format format)
	{
		Width = width;
		Height = height;
		RefreshRate = refreshRate;
		Format = format;
		ScanlineOrdering = DisplayModeScanlineOrder.Unspecified;
		Scaling = DisplayModeScaling.Unspecified;
	}

	public ModeDescription(Format format)
	{
		this = default(ModeDescription);
		Format = format;
	}
}
