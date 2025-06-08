using System;
using System.Threading;

namespace Plugin;

public class KeepPing
{
	public delegate void Delegate();

	private Timer timer;

	private DateTime lastPing;

	public event Delegate Send;

	public KeepPing()
	{
		lastPing = DateTime.Now;
		timer = new Timer(Check, null, 1, 1000);
	}

	private double DiffSeconds(DateTime startTime, DateTime endTime)
	{
		return Math.Abs(new TimeSpan(endTime.Ticks - startTime.Ticks).TotalSeconds);
	}

	private void Check(object obj)
	{
		if (DiffSeconds(lastPing, DateTime.Now) > 10.0 && this.Send != null)
		{
			this.Send();
			Last();
		}
	}

	public void Disconnect()
	{
		timer.Dispose();
	}

	public void Last()
	{
		lastPing = DateTime.Now;
	}
}
