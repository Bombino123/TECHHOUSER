using System;
using System.Threading;
using System.Windows.Forms;
using Plugin.Helper.GDI;

namespace Plugin.Handler.GDI;

internal class HandleSmelt
{
	public static CancellationTokenSource ctsStop;

	public static void Start()
	{
		if (ctsStop == null || ctsStop.IsCancellationRequested)
		{
			ctsStop = new CancellationTokenSource();
			int width = Screen.PrimaryScreen.Bounds.Width;
			int height = Screen.PrimaryScreen.Bounds.Height;
			Random random = new Random();
			while (!ctsStop.IsCancellationRequested)
			{
				IntPtr dC = NativeMethods.GetDC(IntPtr.Zero);
				int num = random.Next(width);
				NativeMethods.BitBlt(dC, num, 10, 30, height, dC, num, 0, 13369376u);
				Thread.Sleep(1);
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
