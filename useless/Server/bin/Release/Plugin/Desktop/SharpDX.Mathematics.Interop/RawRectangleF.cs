using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SharpDX.Mathematics.Interop;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
[DebuggerDisplay("Left: {Left}, Top: {Top}, Right: {Right}, Bottom: {Bottom}")]
public struct RawRectangleF
{
	public float Left;

	public float Top;

	public float Right;

	public float Bottom;

	public RawRectangleF(float left, float top, float right, float bottom)
	{
		Left = left;
		Top = top;
		Right = right;
		Bottom = bottom;
	}
}
