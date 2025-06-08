using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace AntdUI;

internal class Win32
{
	private struct Win32Size
	{
		public int cx;

		public int cy;

		public Win32Size(int x, int y)
		{
			cx = x;
			cy = y;
		}
	}

	private struct Win32Point
	{
		public int x;

		public int y;

		public Win32Point(int x, int y)
		{
			this.x = x;
			this.y = y;
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	private struct BLENDFUNCTION
	{
		public byte BlendOp;

		public byte BlendFlags;

		public byte SourceConstantAlpha;

		public byte AlphaFormat;
	}

	public struct CANDIDATEFORM
	{
		public int dwIndex;

		public int dwStyle;

		public Point ptCurrentPos;

		public Rectangle rcArea;
	}

	public struct COMPOSITIONFORM
	{
		public int dwStyle;

		public Point ptCurrentPos;

		public Rectangle rcArea;
	}

	public struct LOGFONT
	{
		public int lfHeight;

		public int lfWidth;

		public int lfEscapement;

		public int lfOrientation;

		public int lfWeight;

		public byte lfItalic;

		public byte lfUnderline;

		public byte lfStrikeOut;

		public byte lfCharSet;

		public byte lfOutPrecision;

		public byte lfClipPrecision;

		public byte lfQuality;

		public byte lfPitchAndFamily;

		public string lfFaceName;
	}

	private static IntPtr screenDC;

	private static IntPtr memDc;

	private const byte AC_SRC_OVER = 0;

	private const int ULW_ALPHA = 2;

	private const byte AC_SRC_ALPHA = 1;

	public const int SRCCOPY = 13369376;

	public const int GCS_COMPSTR = 8;

	public const int GCS_RESULTSTR = 2048;

	public const int WM_GETDLGCODE = 135;

	public const int DLGC_WANTALLKEYS = 4;

	public const int DLGC_WANTARROWS = 1;

	public const int DLGC_WANTCHARS = 128;

	public const int WM_IME_REQUEST = 648;

	public const int WM_IME_COMPOSITION = 271;

	public const int WM_IME_ENDCOMPOSITION = 270;

	public const int WM_IME_STARTCOMPOSITION = 269;

	public const int CFS_DEFAULT = 0;

	public const int CFS_RECT = 1;

	public const int CFS_POINT = 2;

	public const int CFS_FORCE_POSITION = 32;

	public const int CFS_CANDIDATEPOS = 64;

	public const int CFS_EXCLUDE = 128;

	public const int WM_KEYFIRST = 256;

	public const int WM_KEYLAST = 264;

	public const int WM_IME_CHAR = 646;

	private static byte[] m_byString;

	static Win32()
	{
		m_byString = new byte[1024];
		screenDC = GetDC(IntPtr.Zero);
		memDc = CreateCompatibleDC(screenDC);
	}

	~Win32()
	{
		DeleteDC(memDc);
		ReleaseDC(IntPtr.Zero, screenDC);
	}

	public static void SetBits(Bitmap? bmp, Rectangle rect, IntPtr intPtr, byte a = byte.MaxValue)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		if (bmp == null || (int)((Image)bmp).PixelFormat == 0)
		{
			return;
		}
		IntPtr hbitmap = bmp.GetHbitmap(Color.FromArgb(0));
		IntPtr hObj = SelectObject(memDc, hbitmap);
		try
		{
			Win32Point pptSrc = new Win32Point(0, 0);
			BLENDFUNCTION bLENDFUNCTION = default(BLENDFUNCTION);
			bLENDFUNCTION.BlendOp = 0;
			bLENDFUNCTION.SourceConstantAlpha = a;
			bLENDFUNCTION.AlphaFormat = 1;
			bLENDFUNCTION.BlendFlags = 0;
			BLENDFUNCTION pblend = bLENDFUNCTION;
			Win32Point pptDst = new Win32Point(rect.X, rect.Y);
			Win32Size psize = new Win32Size(rect.Width, rect.Height);
			UpdateLayeredWindow(intPtr, screenDC, ref pptDst, ref psize, memDc, ref pptSrc, 0, ref pblend, 2);
		}
		finally
		{
			if (hbitmap != IntPtr.Zero)
			{
				SelectObject(memDc, hObj);
				DeleteObject(hbitmap);
			}
		}
	}

	[DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
	private static extern IntPtr CreateCompatibleDC(IntPtr hDC);

	[DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
	private static extern IntPtr GetDC(IntPtr hWnd);

	[DllImport("gdi32.dll", ExactSpelling = true)]
	private static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObj);

	[DllImport("user32.dll", ExactSpelling = true)]
	private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

	[DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
	private static extern int DeleteDC(IntPtr hDC);

	[DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
	private static extern int DeleteObject(IntPtr hObj);

	[DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
	private static extern int UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref Win32Point pptDst, ref Win32Size psize, IntPtr hdcSrc, ref Win32Point pptSrc, int crKey, ref BLENDFUNCTION pblend, int dwFlags);

	[DllImport("Imm32.dll")]
	public static extern IntPtr ImmGetContext(IntPtr hWnd);

	[DllImport("Imm32.dll")]
	public static extern bool ImmGetOpenStatus(IntPtr himc);

	[DllImport("Imm32.dll")]
	public static extern bool ImmReleaseContext(IntPtr hWnd, IntPtr hIMC);

	[DllImport("Imm32.dll", CharSet = CharSet.Unicode)]
	public static extern int ImmGetCompositionString(IntPtr hIMC, int dwIndex, byte[] lpBuf, int dwBufLen);

	[DllImport("Imm32.dll")]
	public static extern bool ImmSetCandidateWindow(IntPtr hImc, ref CANDIDATEFORM fuck);

	[DllImport("Imm32.dll")]
	public static extern bool ImmSetCompositionWindow(IntPtr hIMC, ref COMPOSITIONFORM lpCompForm);

	[DllImport("Imm32.dll")]
	public static extern bool ImmSetCompositionFont(IntPtr hIMC, ref LOGFONT logFont);

	public static string? ImmGetCompositionString(IntPtr hIMC, int dwIndex)
	{
		if (hIMC == IntPtr.Zero)
		{
			return null;
		}
		int count = ImmGetCompositionString(hIMC, dwIndex, m_byString, m_byString.Length);
		return Encoding.Unicode.GetString(m_byString, 0, count);
	}

	internal static string? GetClipBoardText()
	{
		IntPtr intPtr = default(IntPtr);
		IntPtr intPtr2 = default(IntPtr);
		try
		{
			if (!OpenClipboard(IntPtr.Zero))
			{
				return null;
			}
			intPtr = GetClipboardData(13u);
			if (intPtr == (IntPtr)0)
			{
				return null;
			}
			intPtr2 = GlobalLock(intPtr);
			if (intPtr2 == (IntPtr)0)
			{
				return null;
			}
			int num = GlobalSize(intPtr);
			byte[] array = new byte[num];
			Marshal.Copy(intPtr2, array, 0, num);
			return Encoding.Unicode.GetString(array).TrimEnd(new char[1]);
		}
		catch
		{
			return null;
		}
		finally
		{
			if (intPtr2 != (IntPtr)0)
			{
				GlobalUnlock(intPtr);
			}
			CloseClipboard();
		}
	}

	internal static bool SetClipBoardText(string? text)
	{
		IntPtr intPtr = default(IntPtr);
		try
		{
			if (!OpenClipboard(IntPtr.Zero))
			{
				return false;
			}
			if (!EmptyClipboard())
			{
				return false;
			}
			if (text == null)
			{
				return true;
			}
			intPtr = Marshal.AllocHGlobal((text.Length + 1) * 2);
			if (intPtr == (IntPtr)0)
			{
				return false;
			}
			IntPtr intPtr2 = GlobalLock(intPtr);
			if (intPtr2 == (IntPtr)0)
			{
				return false;
			}
			try
			{
				Marshal.Copy(text.ToCharArray(), 0, intPtr2, text.Length);
			}
			finally
			{
				GlobalUnlock(intPtr2);
			}
			if (SetClipboardData(13u, intPtr) == (IntPtr)0)
			{
				return false;
			}
			intPtr = default(IntPtr);
			return true;
		}
		catch
		{
			return false;
		}
		finally
		{
			if (intPtr != (IntPtr)0)
			{
				Marshal.FreeHGlobal(intPtr);
			}
			CloseClipboard();
		}
	}

	[DllImport("User32.dll", SetLastError = true)]
	private static extern IntPtr GetClipboardData(uint uFormat);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern IntPtr GlobalLock(IntPtr hMem);

	[DllImport("Kernel32.dll", SetLastError = true)]
	private static extern int GlobalSize(IntPtr hMem);

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GlobalUnlock(IntPtr hMem);

	[DllImport("User32.dll")]
	private static extern bool OpenClipboard(IntPtr hWndNewOwner);

	[DllImport("User32.dll")]
	private static extern bool CloseClipboard();

	[DllImport("User32.dll")]
	private static extern bool EmptyClipboard();

	[DllImport("User32.dll")]
	private static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);
}
