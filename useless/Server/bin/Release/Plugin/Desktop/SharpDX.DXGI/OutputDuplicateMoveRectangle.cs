using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DXGI;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct OutputDuplicateMoveRectangle
{
	public RawPoint SourcePoint;

	public RawRectangle DestinationRect;
}
