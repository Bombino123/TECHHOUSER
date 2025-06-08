using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AntdUI;

public abstract class ILayeredFormOpacity : ILayeredForm
{
	private ITask? task_start;

	private bool run_end;

	private bool ok_end;

	public byte maxalpha = 240;

	private Bitmap? bmp_tmp;

	protected override void OnLoad(EventArgs e)
	{
		if (Config.Animation)
		{
			int t = Animation.TotalFrames(10, 80);
			task_start = new ITask(delegate(int i)
			{
				float num = Animation.Animate(i, t, 1f, AnimationType.Ball);
				SetAnimateValue((byte)((float)(int)maxalpha * num));
				return true;
			}, 10, t, IStart);
		}
		else
		{
			IStart();
		}
		base.OnLoad(e);
	}

	private void SetAnimateValue(byte _alpha, bool isrint = false)
	{
		if (alpha == _alpha)
		{
			return;
		}
		alpha = _alpha;
		if (isrint)
		{
			Print();
		}
		else
		{
			if (!((Control)this).IsHandleCreated || base.TargetRect.Width <= 0 || base.TargetRect.Height <= 0)
			{
				return;
			}
			try
			{
				if (bmp_tmp == null)
				{
					bmp_tmp = PrintBit();
				}
				if (bmp_tmp != null)
				{
					Print(bmp_tmp);
				}
			}
			catch
			{
			}
		}
	}

	public virtual void LoadOK()
	{
	}

	protected override void OnClosing(CancelEventArgs e)
	{
		task_start?.Dispose();
		if (!ok_end)
		{
			e.Cancel = true;
			if (Config.Animation)
			{
				if (!run_end)
				{
					run_end = true;
					int t = Animation.TotalFrames(10, 80);
					new ITask(delegate(int i)
					{
						float num = Animation.Animate(i, t, 1f, AnimationType.Ball);
						SetAnimateValue((byte)((float)(int)maxalpha * (1f - num)));
						return true;
					}, 10, t, IEnd);
				}
			}
			else
			{
				IEnd();
			}
		}
		((Form)this).OnClosing(e);
	}

	private void IStart()
	{
		Bitmap? obj = bmp_tmp;
		if (obj != null)
		{
			((Image)obj).Dispose();
		}
		bmp_tmp = null;
		SetAnimateValue(maxalpha, isrint: true);
		LoadOK();
	}

	private void IEnd()
	{
		Bitmap? obj = bmp_tmp;
		if (obj != null)
		{
			((Image)obj).Dispose();
		}
		bmp_tmp = null;
		ok_end = true;
		IClose(isdispose: true);
	}

	protected override void Dispose(bool disposing)
	{
		task_start?.Dispose();
		task_start = null;
		base.Dispose(disposing);
	}
}
