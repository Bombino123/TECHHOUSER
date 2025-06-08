using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Plugin.Helper;

internal class ScreenCapture
{
	private class User32
	{
		public struct Rect
		{
			public int left;

			public int top;

			public int right;

			public int bottom;
		}

		[DllImport("user32.dll")]
		public static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

		[DllImport("user32.dll")]
		public static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, uint nFlags);
	}

	public Bitmap CaptureWindow(IntPtr handle)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		User32.Rect rect = default(User32.Rect);
		User32.GetWindowRect(handle, ref rect);
		Bitmap val = new Bitmap(rect.right - rect.left + 1, rect.bottom - rect.top + 1);
		Graphics val2 = Graphics.FromImage((Image)(object)val);
		try
		{
			IntPtr hdc = val2.GetHdc();
			if (Plugin.HigherThan81)
			{
				User32.PrintWindow(handle, hdc, 2u);
			}
			else
			{
				User32.PrintWindow(handle, hdc, 0u);
			}
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
		return val;
	}
}
