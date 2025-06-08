using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace WinFormAnimation;

public class Timer
{
	private static Thread _timerThread;

	private static readonly object LockHandle = new object();

	private static readonly long StartTimeAsMs = DateTime.Now.Ticks;

	private static readonly List<Timer> Subscribers = new List<Timer>();

	private readonly Action<ulong> _callback;

	public long LastTick { get; private set; }

	public int FrameLimiter { get; set; }

	public long FirstTick { get; private set; }

	public Timer(Action<ulong> callback, FPSLimiterKnownValues fpsKnownLimit = FPSLimiterKnownValues.LimitThirty)
		: this(callback, (int)fpsKnownLimit)
	{
	}

	public Timer(Action<ulong> callback, int fpsLimit)
	{
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		_callback = callback;
		FrameLimiter = fpsLimit;
		lock (LockHandle)
		{
			if (_timerThread == null)
			{
				Thread obj = new Thread(ThreadCycle)
				{
					IsBackground = true
				};
				_timerThread = obj;
				obj.Start();
			}
		}
	}

	private void Tick()
	{
		if (1000 / FrameLimiter < GetTimeDifferenceAsMs() - LastTick)
		{
			LastTick = GetTimeDifferenceAsMs();
			_callback((ulong)(LastTick - FirstTick));
		}
	}

	private static long GetTimeDifferenceAsMs()
	{
		return (DateTime.Now.Ticks - StartTimeAsMs) / 10000;
	}

	private static void ThreadCycle()
	{
		while (true)
		{
			try
			{
				bool flag;
				lock (Subscribers)
				{
					flag = Subscribers.Count == 0;
					if (!flag)
					{
						foreach (Timer item in Subscribers.ToList())
						{
							item.Tick();
						}
					}
				}
				Thread.Sleep((!flag) ? 1 : 50);
			}
			catch
			{
			}
		}
	}

	public void ResetClock()
	{
		FirstTick = GetTimeDifferenceAsMs();
	}

	public void Resume()
	{
		lock (Subscribers)
		{
			if (!Subscribers.Contains(this))
			{
				FirstTick += GetTimeDifferenceAsMs() - LastTick;
				Subscribers.Add(this);
			}
		}
	}

	public void Start()
	{
		lock (Subscribers)
		{
			if (!Subscribers.Contains(this))
			{
				FirstTick = GetTimeDifferenceAsMs();
				Subscribers.Add(this);
			}
		}
	}

	public void Stop()
	{
		lock (Subscribers)
		{
			if (Subscribers.Contains(this))
			{
				Subscribers.Remove(this);
			}
		}
	}
}
