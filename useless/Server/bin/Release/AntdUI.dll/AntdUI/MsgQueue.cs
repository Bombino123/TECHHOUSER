using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;

namespace AntdUI;

public static class MsgQueue
{
	private static ManualResetEvent _event;

	internal static ConcurrentQueue<object> queue;

	internal static ConcurrentQueue<object> queue_cache;

	internal static List<string> volley;

	public static void Add(Notification.Config config)
	{
		queue.Enqueue(config);
		_event.SetWait();
	}

	public static void Add(Message.Config config)
	{
		queue.Enqueue(config);
		_event.SetWait();
	}

	internal static void Add(ILayeredFormAnimate command)
	{
		queue.Enqueue(command);
		_event.SetWait();
	}

	static MsgQueue()
	{
		_event = new ManualResetEvent(initialState: false);
		queue = new ConcurrentQueue<object>();
		queue_cache = new ConcurrentQueue<object>();
		volley = new List<string>();
		Thread thread = new Thread(LongTask);
		thread.IsBackground = true;
		thread.Start();
	}

	private static void LongTask()
	{
		while (!_event.Wait())
		{
			object result;
			while (queue.TryDequeue(out result))
			{
				Hand(result);
			}
			volley.Clear();
			_event.ResetWait();
		}
	}

	private static void Hand(object d)
	{
		try
		{
			if (d is Notification.Config config)
			{
				if (Open(config))
				{
					_event.ResetWait();
				}
			}
			else if (d is Message.Config config2)
			{
				if (Open(config2))
				{
					_event.ResetWait();
				}
			}
			else if (d is ILayeredFormAnimate layeredFormAnimate)
			{
				if (Config.Animation)
				{
					layeredFormAnimate.StopAnimation().Wait();
				}
				layeredFormAnimate.IClose(isdispose: true);
				Close(layeredFormAnimate.Align, layeredFormAnimate.key);
				if (queue_cache.TryDequeue(out object result))
				{
					Hand(result);
				}
			}
		}
		catch
		{
		}
	}

	private static bool Open(Notification.Config config)
	{
		Notification.Config config2 = config;
		if (((Control)config2.Form).IsHandleCreated)
		{
			if (config2.ID != null)
			{
				string item = "N" + config2.ID;
				if (volley.Contains(item))
				{
					volley.Remove(item);
					return false;
				}
			}
			bool ishand = false;
			((Control)config2.Form).Invoke((Delegate)(Action)delegate
			{
				NotificationFrm notificationFrm = new NotificationFrm(config2);
				if (notificationFrm.IInit())
				{
					((Component)(object)notificationFrm).Dispose();
					ishand = true;
				}
				else if (config2.TopMost)
				{
					((Control)notificationFrm).Show();
				}
				else
				{
					((Form)notificationFrm).Show((IWin32Window)(object)config2.Form);
				}
			});
			if (ishand)
			{
				queue_cache.Enqueue(config2);
				return true;
			}
		}
		return false;
	}

	private static bool Open(Message.Config config)
	{
		Message.Config config2 = config;
		if (((Control)config2.Form).IsHandleCreated)
		{
			if (config2.ID != null)
			{
				string item = "M" + config2.ID;
				if (volley.Contains(item))
				{
					volley.Remove(item);
					return false;
				}
			}
			bool ishand = false;
			((Control)config2.Form).Invoke((Delegate)(Action)delegate
			{
				MessageFrm messageFrm = new MessageFrm(config2);
				if (messageFrm.IInit())
				{
					((Component)(object)messageFrm).Dispose();
					ishand = true;
				}
				else
				{
					((Form)messageFrm).Show((IWin32Window)(object)config2.Form);
				}
			});
			if (ishand)
			{
				queue_cache.Enqueue(config2);
				return true;
			}
		}
		return false;
	}

	private static void Close(TAlignFrom align, string key)
	{
		try
		{
			if ((uint)align > 2u && (uint)(align - 3) <= 2u)
			{
				CloseB(key);
			}
			else
			{
				CloseT(key);
			}
		}
		catch
		{
		}
	}

	private static void CloseT(string key)
	{
		string[] array = key.Split(new char[1] { '|' });
		int y = int.Parse(array[2]);
		int offset = (int)((float)Config.NoticeWindowOffsetXY * Config.Dpi);
		int y_temp = y + offset;
		List<ILayeredFormAnimate> list = ILayeredFormAnimate.list[key];
		Dictionary<ILayeredFormAnimate, int[]> dir = new Dictionary<ILayeredFormAnimate, int[]>(list.Count);
		foreach (ILayeredFormAnimate item in list)
		{
			int readY = item.ReadY;
			if (readY != y_temp)
			{
				dir.Add(item, new int[2]
				{
					y_temp,
					readY - y_temp
				});
			}
			y_temp += item.TargetRect.Height;
		}
		if (dir.Count <= 0)
		{
			return;
		}
		if (Config.Animation)
		{
			int t = Animation.TotalFrames(10, 200);
			new ITask(delegate(int i)
			{
				float num = Animation.Animate(i, t, 1f, AnimationType.Ball);
				foreach (KeyValuePair<ILayeredFormAnimate, int[]> item2 in dir)
				{
					item2.Key.SetAnimateValueY(item2.Value[0] + (item2.Value[1] - (int)((float)item2.Value[1] * num)));
				}
				return true;
			}, 10, t, delegate
			{
				y_temp = y + offset;
				for (int k = 0; k < list.Count; k++)
				{
					ILayeredFormAnimate layeredFormAnimate2 = list[k];
					layeredFormAnimate2.DisposeAnimation();
					layeredFormAnimate2.SetAnimateValueY(y_temp);
					layeredFormAnimate2.SetPositionY(y_temp);
					y_temp += layeredFormAnimate2.TargetRect.Height;
				}
			}).Wait();
		}
		else
		{
			y_temp = y + offset;
			for (int j = 0; j < list.Count; j++)
			{
				ILayeredFormAnimate layeredFormAnimate = list[j];
				layeredFormAnimate.DisposeAnimation();
				layeredFormAnimate.SetAnimateValueY(y_temp);
				layeredFormAnimate.SetPositionY(y_temp);
				y_temp += layeredFormAnimate.TargetRect.Height;
			}
		}
	}

	private static void CloseB(string key)
	{
		string[] array = key.Split(new char[1] { '|' });
		int b = int.Parse(array[4]);
		int offset = (int)((float)Config.NoticeWindowOffsetXY * Config.Dpi);
		int y_temp = b - offset;
		List<ILayeredFormAnimate> list = ILayeredFormAnimate.list[key];
		Dictionary<ILayeredFormAnimate, int[]> dir = new Dictionary<ILayeredFormAnimate, int[]>(list.Count);
		foreach (ILayeredFormAnimate item in list)
		{
			y_temp -= item.TargetRect.Height;
			int readY = item.ReadY;
			if (readY != y_temp)
			{
				dir.Add(item, new int[2]
				{
					y_temp,
					readY - y_temp
				});
			}
		}
		if (dir.Count <= 0)
		{
			return;
		}
		if (Config.Animation)
		{
			int t = Animation.TotalFrames(10, 200);
			new ITask(delegate(int i)
			{
				float num = Animation.Animate(i, t, 1f, AnimationType.Ball);
				foreach (KeyValuePair<ILayeredFormAnimate, int[]> item2 in dir)
				{
					item2.Key.SetAnimateValueY(item2.Value[0] + (item2.Value[1] - (int)((float)item2.Value[1] * num)));
				}
				return true;
			}, 10, t, delegate
			{
				y_temp = b - offset;
				for (int k = 0; k < list.Count; k++)
				{
					ILayeredFormAnimate layeredFormAnimate2 = list[k];
					layeredFormAnimate2.DisposeAnimation();
					y_temp -= layeredFormAnimate2.TargetRect.Height;
					layeredFormAnimate2.SetAnimateValueY(y_temp);
					layeredFormAnimate2.SetPositionY(y_temp);
				}
			}).Wait();
		}
		else
		{
			y_temp = b - offset;
			for (int j = 0; j < list.Count; j++)
			{
				ILayeredFormAnimate layeredFormAnimate = list[j];
				layeredFormAnimate.DisposeAnimation();
				y_temp -= layeredFormAnimate.TargetRect.Height;
				layeredFormAnimate.SetAnimateValueY(y_temp);
				layeredFormAnimate.SetPositionY(y_temp);
			}
		}
	}
}
