using System;

namespace Plugin.Handler;

internal class HandleOpenCD
{
	public void Show()
	{
		Native.mciSendString("set cdaudio door open", null, 0, IntPtr.Zero);
	}

	public void Hide()
	{
		Native.mciSendString("set CDAudio door closed", null, 0, IntPtr.Zero);
	}
}
