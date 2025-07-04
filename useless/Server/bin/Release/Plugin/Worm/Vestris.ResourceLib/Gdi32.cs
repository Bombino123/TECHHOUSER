using System;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Vestris.ResourceLib;

[ComVisible(true)]
public abstract class Gdi32
{
	public enum BitmapCompression
	{
		BI_RGB,
		BI_RLE8,
		BI_RLE4,
		BI_BITFIELDS,
		BI_JPEG,
		BI_PNG
	}

	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public struct BITMAPINFOHEADER
	{
		public uint biSize;

		public int biWidth;

		public int biHeight;

		public ushort biPlanes;

		public ushort biBitCount;

		public uint biCompression;

		public uint biSizeImage;

		public int biXPelsPerMeter;

		public int biYPelsPerMeter;

		public uint biClrUsed;

		public uint biClrImportant;

		public BitmapCompression BitmapCompression => (BitmapCompression)biCompression;

		public PixelFormat PixelFormat => (PixelFormat)(biBitCount switch
		{
			1 => 196865, 
			4 => 197634, 
			8 => 198659, 
			16 => 135174, 
			24 => 137224, 
			32 => 2498570, 
			_ => 0, 
		});

		public string PixelFormatString
		{
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_000d: Invalid comparison between Unknown and I4
				//IL_0038: Unknown result type (might be due to invalid IL or missing references)
				//IL_003e: Invalid comparison between Unknown and I4
				//IL_0012: Unknown result type (might be due to invalid IL or missing references)
				//IL_0018: Invalid comparison between Unknown and I4
				//IL_0043: Unknown result type (might be due to invalid IL or missing references)
				//IL_0049: Invalid comparison between Unknown and I4
				//IL_001d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0023: Invalid comparison between Unknown and I4
				//IL_004e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0054: Invalid comparison between Unknown and I4
				//IL_0028: Unknown result type (might be due to invalid IL or missing references)
				//IL_002e: Invalid comparison between Unknown and I4
				PixelFormat pixelFormat = PixelFormat;
				if ((int)pixelFormat <= 196865)
				{
					if ((int)pixelFormat == 137224)
					{
						return "24-bit True Colors";
					}
					if ((int)pixelFormat == 139273)
					{
						goto IL_006a;
					}
					if ((int)pixelFormat == 196865)
					{
						return "1-bit B/W";
					}
				}
				else
				{
					if ((int)pixelFormat == 197634)
					{
						return "4-bit 16 Colors";
					}
					if ((int)pixelFormat == 198659)
					{
						return "8-bit 256 Colors";
					}
					if ((int)pixelFormat == 2498570)
					{
						goto IL_006a;
					}
				}
				return "Unknown";
				IL_006a:
				return "32-bit Alpha Channel";
			}
		}
	}

	public struct BITMAPINFO
	{
		public BITMAPINFOHEADER bmiHeader;

		public RGBQUAD bmiColors;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct RGBQUAD
	{
		public byte rgbBlue;

		public byte rgbGreen;

		public byte rgbRed;

		public byte rgbReserved;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public struct BITMAPFILEHEADER
	{
		public ushort bfType;

		public uint bfSize;

		public ushort bfReserved1;

		public ushort bfReserved2;

		public uint bfOffBits;
	}

	internal enum DIBColors
	{
		DIB_RGB_COLORS = 0,
		DIB_PAL_COLORS = 1,
		DIB_PAL_INDICES = 2,
		DIB_PAL_LOGINDICES = 4
	}

	[DllImport("gdi32.dll", SetLastError = true)]
	internal static extern int SetDIBitsToDevice(IntPtr hdc, int XDest, int YDest, uint dwWidth, uint dwHeight, int XSrc, int YSrc, uint uStartScan, uint cScanLines, byte[] lpvBits, [In] ref BITMAPINFO lpbmi, uint fuColorUse);

	[DllImport("gdi32.dll", SetLastError = true)]
	internal static extern int SetDIBitsToDevice(IntPtr hdc, int XDest, int YDest, uint dwWidth, uint dwHeight, int XSrc, int YSrc, uint uStartScan, uint cScanLines, IntPtr lpvBits, IntPtr lpbmi, uint fuColorUse);

	[DllImport("gdi32.dll", SetLastError = true)]
	internal static extern int GetDIBits(IntPtr hdc, IntPtr hbmp, uint uStartScan, uint cScanLines, [Out] byte[] lpvBits, [In] ref BITMAPINFO lpbmi, uint uUsage);

	[DllImport("gdi32.dll", SetLastError = true)]
	internal static extern IntPtr CreateDIBSection(IntPtr hdc, [In] ref BITMAPINFO pbmi, uint iUsage, out IntPtr ppvBits, IntPtr hSection, uint dwOffset);

	[DllImport("gdi32.dll", SetLastError = true)]
	internal static extern IntPtr CreateCompatibleDC(IntPtr hdc);

	[DllImport("gdi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	internal static extern IntPtr CreateDC(string lpDriverName, string lpDeviceName, string lpOutput, IntPtr lpInitData);

	[DllImport("gdi32.dll", SetLastError = true)]
	internal static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

	[DllImport("gdi32.dll", SetLastError = true)]
	internal static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

	[DllImport("gdi32.dll", SetLastError = true)]
	internal static extern int DeleteObject(IntPtr hObject);

	[DllImport("gdi32.dll", SetLastError = true)]
	internal static extern int DeleteDC(IntPtr hdc);
}
