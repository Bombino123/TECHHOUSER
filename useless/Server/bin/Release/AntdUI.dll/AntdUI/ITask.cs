using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AntdUI;

public class ITask : IDisposable
{
	public bool IsRun;

	private Task task;

	private CancellationTokenSource? token = new CancellationTokenSource();

	public object? Tag { get; set; }

	public ITask(Control control, Func<int, bool> action, int interval, int max, int add, Action? end = null)
	{
		Control control2 = control;
		Func<int, bool> action2 = action;
		Action end2 = end;
		base._002Ector();
		ITask task = this;
		bool ok = true;
		IsRun = true;
		this.task = Run(delegate
		{
			int num = 0;
			while (true)
			{
				if (task.token.Wait(control2))
				{
					ok = false;
					break;
				}
				num += add;
				if (num > max)
				{
					num = 0;
				}
				if (!action2(num))
				{
					break;
				}
				Thread.Sleep(interval);
			}
		}).ContinueWith(delegate
		{
			if (ok && end2 != null)
			{
				end2();
			}
			task.Dispose();
		});
	}

	public ITask(Control control, Action<float> action, int interval, float max, float add, Action? end = null)
	{
		Control control2 = control;
		Action<float> action2 = action;
		Action end2 = end;
		base._002Ector();
		ITask task = this;
		bool ok = true;
		IsRun = true;
		this.task = Run(delegate
		{
			float num = 0f;
			while (!task.token.Wait(control2))
			{
				num += add;
				if (num > max)
				{
					num = 0f;
				}
				action2(num);
				Thread.Sleep(interval);
			}
			ok = false;
		}).ContinueWith(delegate
		{
			if (ok && end2 != null)
			{
				end2();
			}
			task.Dispose();
		});
	}

	public ITask(Control control, Func<bool> action, int interval, Action? end = null, int sleep = 0)
	{
		Control control2 = control;
		Func<bool> action2 = action;
		Action end2 = end;
		base._002Ector();
		ITask task = this;
		bool ok = true;
		IsRun = true;
		this.task = Run(delegate
		{
			if (sleep > 0)
			{
				Thread.Sleep(sleep);
			}
			while (true)
			{
				if (task.token.Wait(control2))
				{
					ok = false;
					break;
				}
				if (!action2())
				{
					break;
				}
				Thread.Sleep(interval);
			}
		}).ContinueWith(delegate
		{
			if (ok && end2 != null)
			{
				end2();
			}
			task.Dispose();
		});
	}

	public ITask(Func<int, bool> action, int interval, int totalFrames, Action end, int sleep = 0)
	{
		Func<int, bool> action2 = action;
		Action end2 = end;
		base._002Ector();
		ITask task = this;
		IsRun = true;
		bool ok = true;
		this.task = Run(delegate
		{
			if (sleep > 0)
			{
				Thread.Sleep(sleep);
			}
			for (int i = 0; i < totalFrames; i++)
			{
				if (task.token.Wait())
				{
					ok = false;
					break;
				}
				if (!action2(i + 1))
				{
					break;
				}
				Thread.Sleep(interval);
			}
		}).ContinueWith(delegate
		{
			if (ok)
			{
				end2();
			}
			task.Dispose();
		});
	}

	public ITask(bool _is, int interval, int totalFrames, float cold, AnimationType type, Action<int, float> action, Action end)
	{
		Action<int, float> action2 = action;
		Action end2 = end;
		base._002Ector();
		ITask task = this;
		IsRun = true;
		bool ok = true;
		this.task = Run(delegate
		{
			double num = 1.0;
			if (_is)
			{
				if (cold > -1f)
				{
					num = cold;
				}
				for (int i = 0; i < totalFrames; i++)
				{
					if (task.token.Wait())
					{
						ok = false;
						break;
					}
					int num2 = i + 1;
					double progress = (double)num2 * 1.0 / (double)totalFrames;
					float num3 = (float)(num - Animation.Animate(progress, num, type));
					task.Tag = num3;
					action2(num2, num3);
					Thread.Sleep(interval);
				}
			}
			else
			{
				num = ((!(cold > -1f)) ? 0.0 : ((double)cold));
				for (int j = 0; j < totalFrames; j++)
				{
					if (task.token.Wait())
					{
						ok = false;
						break;
					}
					int num4 = j + 1;
					float num5 = (float)(Animation.Animate((double)num4 * 1.0 / (double)totalFrames, 1.0 + num, type) - num);
					if (num5 < 0f)
					{
						break;
					}
					task.Tag = num5;
					action2(num4, num5);
					Thread.Sleep(interval);
				}
			}
		}).ContinueWith(delegate
		{
			if (ok)
			{
				task.Tag = null;
				end2();
			}
			task.Dispose();
		});
	}

	public void Wait()
	{
		task.Wait();
	}

	public void Cancel()
	{
		token?.Cancel();
	}

	public void Dispose()
	{
		if (token != null)
		{
			token?.Cancel();
			token?.Dispose();
			token = null;
		}
		IsRun = false;
		GC.SuppressFinalize(this);
	}

	public static Task Run(Action action, Action? end = null)
	{
		Action end2 = end;
		if (end2 == null)
		{
			return Task.Run(action);
		}
		return Task.Run(action).ContinueWith(delegate
		{
			end2();
		});
	}

	public static Task<TResult> Run<TResult>(Func<TResult> action)
	{
		return Task.Run(action);
	}
}
