using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SharpDX.Mathematics.Interop;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
[DebuggerDisplay("Left: {Left}, Top: {Top}, Right: {Right}, Bottom: {Bottom}")]
public struct RawRectangle
{
	public int Left;

	public int Top;

	public int Right;

	public int Bottom;

	public bool IsEmpty
	{
		get
		{
			if (Left == 0 && Top == 0 && Right == 0)
			{
				return Bottom == 0;
			}
			return false;
		}
	}

	public RawRectangle(int left, int top, int right, int bottom)
	{
		Left = left;
		Top = top;
		Right = right;
		Bottom = bottom;
	}
}
