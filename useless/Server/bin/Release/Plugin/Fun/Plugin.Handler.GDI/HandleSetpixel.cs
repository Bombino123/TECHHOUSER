using System;
using System.Threading;
using Plugin.Helper.GDI;

namespace Plugin.Handler.GDI;

internal class HandleSetpixel
{
	public static CancellationTokenSource ctsStop;

	public static void Start()
	{
		if (ctsStop != null && !ctsStop.IsCancellationRequested)
		{
			return;
		}
		ctsStop = new CancellationTokenSource();
		Random random = new Random();
		int systemMetrics = NativeMethods.GetSystemMetrics(1);
		int systemMetrics2 = NativeMethods.GetSystemMetrics(0);
		int systemMetrics3 = NativeMethods.GetSystemMetrics(17);
		int systemMetrics4 = NativeMethods.GetSystemMetrics(16);
		int num = systemMetrics2 - random.Next(systemMetrics2) - (systemMetrics2 / 150 - 112) % 149;
		int num2 = (int)Math.Round((double)systemMetrics2 / 100.0);
		while (!ctsStop.IsCancellationRequested)
		{
			IntPtr dC = NativeMethods.GetDC(IntPtr.Zero);
			for (int i = 0; i < systemMetrics; i++)
			{
				for (int j = 0; j < systemMetrics2; j++)
				{
					int num3 = num2 * j;
					NativeMethods.SetPixel(dC, j, i, NativeMethods.RGB(num3, num3, num3));
					NativeMethods.SetPixel(dC, i, num, NativeMethods.RGB(systemMetrics3, num3, systemMetrics4));
					NativeMethods.SetPixel(dC, num, num, NativeMethods.RGB(num, num, num));
					NativeMethods.SetPixel(dC, num, i, NativeMethods.RGB(systemMetrics4, systemMetrics3, systemMetrics4));
				}
				if (ctsStop.IsCancellationRequested)
				{
					return;
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
