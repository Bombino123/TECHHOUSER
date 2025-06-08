using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DXGI;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct OutputDuplicateDescription
{
	public ModeDescription ModeDescription;

	public DisplayModeRotation Rotation;

	public RawBool DesktopImageInSystemMemory;
}
