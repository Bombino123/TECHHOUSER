using System;

namespace Plugin.Handler;

internal class HandleMonitor
{
	private const int HWND_BROADCAST = 65535;

	private const int WM_SYSCOMMAND = 274;

	private const int SC_MONITORPOWER = 61808;

	public void TurnOff()
	{
		Native.SendMessage(new IntPtr(65535), 274u, new IntPtr(61808), new IntPtr(2));
	}
}
