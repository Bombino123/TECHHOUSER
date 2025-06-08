using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DXGI;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct OutputDuplicatePointerShapeInformation
{
	public int Type;

	public int Width;

	public int Height;

	public int Pitch;

	public RawPoint HotSpot;
}
