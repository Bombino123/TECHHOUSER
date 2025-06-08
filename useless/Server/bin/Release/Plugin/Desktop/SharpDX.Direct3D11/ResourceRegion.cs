using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct ResourceRegion
{
	public int Left;

	public int Top;

	public int Front;

	public int Right;

	public int Bottom;

	public int Back;

	public ResourceRegion(int left, int top, int front, int right, int bottom, int back)
	{
		Left = left;
		Top = top;
		Front = front;
		Right = right;
		Bottom = bottom;
		Back = back;
	}
}
