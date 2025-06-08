using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Plugin.Helper.GDI;

internal class NativeMethods
{
	public struct RECT
	{
		public int left;

		public int top;

		public int right;

		public int bottom;
	}

	public struct POINT
	{
		public int X;

		public int Y;

		public POINT(int x, int y)
		{
			X = x;
			Y = y;
		}

		public static implicit operator Point(POINT p)
		{
			return new Point(p.X, p.Y);
		}

		public static implicit operator POINT(Point p)
		{
			return new POINT(p.X, p.Y);
		}
	}

	public const int PS_SOLID = 0;

	[DllImport("gdi32.dll")]
	public static extern IntPtr CreateEllipticRgn(int x1, int y1, int x2, int y2);

	[DllImport("gdi32.dll")]
	public static extern int SelectClipRgn(IntPtr hdc, IntPtr hrgn);

	[DllImport("gdi32.dll")]
	public static extern int SetPixel(IntPtr hdc, int x, int y, int color);

	[DllImport("gdi32.dll")]
	public static extern bool PlgBlt(IntPtr hdcDest, POINT[] lpPoint, IntPtr hdcSrc, int nXSrc, int nYSrc, int nWidth, int nHeight, IntPtr hbmMask, int xMask, int yMask);

	[DllImport("gdi32.dll", CharSet = CharSet.Auto, ExactSpelling = true, SetLastError = true)]
	public static extern bool BitBlt(IntPtr hDestDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, uint dwRop);

	[DllImport("gdi32.dll")]
	public static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int wDest, int hDest, IntPtr hdcSource, int xSrc, int ySrc, long RasterOp);

	[DllImport("gdi32.dll", CharSet = CharSet.Auto, ExactSpelling = true, SetLastError = true)]
	public static extern bool StretchBlt(IntPtr hDestDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int nSrcWidth, int nSrcHeight, uint dwRop);

	[DllImport("gdi32.dll")]
	public static extern IntPtr CreatePen(int style, int width, uint color);

	[DllImport("gdi32.dll")]
	public static extern IntPtr SelectPen(IntPtr hdc, IntPtr hPen);

	[DllImport("gdi32.dll")]
	public static extern IntPtr CreateSolidBrush(uint color);

	[DllImport("gdi32.dll")]
	public static extern IntPtr SelectBrush(IntPtr hdc, IntPtr hBrush);

	[DllImport("gdi32.dll")]
	public static extern bool Polygon(IntPtr hdc, Point[] lpPoints, int nCount);

	[DllImport("gdi32.dll")]
	public static extern IntPtr CreateBitmap(int nWidth, int nHeight, uint cPlanes, uint cBitsPerPel, IntPtr lpvBits);

	[DllImport("gdi32.dll")]
	public static extern bool GetBitmapBits(IntPtr hbmp, int cbBuffer, IntPtr lpvBits);

	[DllImport("gdi32.dll")]
	public static extern bool SetBitmapBits(IntPtr hbmp, int cbBuffer, IntPtr lpvBits);

	[DllImport("gdi32.dll")]
	public static extern bool PatBlt(IntPtr hdc, int nXLeft, int nYLeft, int nWidth, int nHeight, uint dwRop);

	[DllImport("gdi32.dll")]
	public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hObject);

	[DllImport("gdi32.dll")]
	public static extern bool DeleteObject(IntPtr hObject);

	[DllImport("gdi32.dll")]
	public static extern bool DeleteDC(IntPtr hdc);

	[DllImport("user32.dll")]
	public static extern IntPtr GetDC(IntPtr hwnd);

	[DllImport("user32.dll")]
	public static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

	[DllImport("user32.dll")]
	public static extern IntPtr GetDesktopWindow();

	[DllImport("user32.dll")]
	public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

	[DllImport("user32.dll")]
	public static extern int GetSystemMetrics(int nIndex);

	[DllImport("kernel32.dll")]
	public static extern IntPtr VirtualAlloc(IntPtr lpAddress, int dwSize, int flAllocationType, int flProtect);

	public static uint RGB(byte r, byte g, byte b)
	{
		return (uint)(r | (g << 8) | (b << 16));
	}

	public static int RGB(int r, int g, int b)
	{
		return r | (g << 8) | (b << 16);
	}
}
