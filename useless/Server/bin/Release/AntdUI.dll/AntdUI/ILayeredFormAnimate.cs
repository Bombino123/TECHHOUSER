using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace AntdUI;

public abstract class ILayeredFormAnimate : ILayeredForm
{
	internal static Dictionary<string, List<ILayeredFormAnimate>> list = new Dictionary<string, List<ILayeredFormAnimate>>();

	internal string key = "";

	private int start_X;

	private int end_X;

	private int start_Y;

	private int end_Y;

	private ITask? task_start;

	private Bitmap? bmp_tmp;

	private DateTime closetime;

	private bool handclose;

	internal virtual TAlignFrom Align => TAlignFrom.TR;

	internal virtual bool ActiveAnimation => true;

	public int ReadY => end_Y;

	public int ReadB => end_Y + base.TargetRect.Height;

	internal bool SetPosition(Form form, bool InWindow)
	{
		Rectangle workingArea = ((!InWindow && !Config.ShowInWindow) ? Screen.FromControl((Control)(object)form).WorkingArea : new Rectangle(form.Location, form.Size));
		key = Align.ToString() + "|" + workingArea.X + "|" + workingArea.Y + "|" + workingArea.Right + "|" + workingArea.Bottom;
		int width = base.TargetRect.Width;
		int height = base.TargetRect.Height;
		switch (Align)
		{
		case TAlignFrom.Top:
		{
			if (TopY(workingArea, out var result5))
			{
				return true;
			}
			int x2 = (start_X = (end_X = workingArea.X + (workingArea.Width - width) / 2));
			end_Y = result5;
			SetLocation(x2, start_Y = end_Y - height / 2);
			break;
		}
		case TAlignFrom.Bottom:
		{
			if (BottomY(workingArea, out var result3))
			{
				return true;
			}
			int x = (start_X = (end_X = workingArea.X + (workingArea.Width - width) / 2));
			end_Y = result3;
			SetLocation(x, start_Y = end_Y + height / 2);
			break;
		}
		case TAlignFrom.TL:
		{
			if (TopY(workingArea, out var result6))
			{
				return true;
			}
			end_X = workingArea.X;
			SetLocation(start_X = end_X - width / 3, start_Y = (end_Y = result6));
			break;
		}
		case TAlignFrom.TR:
		{
			if (TopY(workingArea, out var result2))
			{
				return true;
			}
			end_X = workingArea.X + workingArea.Width - width;
			SetLocation(start_X = end_X + width / 3, start_Y = (end_Y = result2));
			break;
		}
		case TAlignFrom.BL:
		{
			if (BottomY(workingArea, out var result4))
			{
				return true;
			}
			end_X = workingArea.X;
			SetLocation(start_X = end_X - width / 3, start_Y = (end_Y = result4));
			break;
		}
		case TAlignFrom.BR:
		{
			if (BottomY(workingArea, out var result))
			{
				return true;
			}
			end_X = workingArea.X + workingArea.Width - width;
			SetLocation(start_X = end_X + width / 3, start_Y = (end_Y = result));
			break;
		}
		}
		Add();
		return false;
	}

	private void Add()
	{
		if (list.TryGetValue(key, out List<ILayeredFormAnimate> value))
		{
			value.Add(this);
			return;
		}
		list.Add(key, new List<ILayeredFormAnimate> { this });
	}

	private bool TopY(Rectangle workingArea, out int result)
	{
		int offset = (int)((float)Config.NoticeWindowOffsetXY * Config.Dpi);
		int num = TopYCore(workingArea, offset);
		if (num < workingArea.Bottom - base.TargetRect.Height)
		{
			result = num;
			return false;
		}
		result = 0;
		return true;
	}

	private int TopYCore(Rectangle workingArea, int offset)
	{
		if (list.TryGetValue(key, out List<ILayeredFormAnimate> value) && value.Count > 0)
		{
			int readB = value[value.Count - 1].ReadB;
			for (int i = 0; i < value.Count - 1; i++)
			{
				ILayeredFormAnimate layeredFormAnimate = value[i];
				if (layeredFormAnimate.ReadB > readB)
				{
					readB = layeredFormAnimate.ReadB;
				}
			}
			return readB;
		}
		return workingArea.Y + offset;
	}

	private bool BottomY(Rectangle workingArea, out int result)
	{
		int offset = (int)((float)Config.NoticeWindowOffsetXY * Config.Dpi);
		int num = BottomYCore(workingArea, offset) - base.TargetRect.Height;
		if (num >= 0)
		{
			result = num;
			return false;
		}
		result = 0;
		return true;
	}

	private int BottomYCore(Rectangle workingArea, int offset)
	{
		if (list.TryGetValue(key, out List<ILayeredFormAnimate> value) && value.Count > 0)
		{
			int readY = value[value.Count - 1].ReadY;
			for (int i = 0; i < value.Count - 1; i++)
			{
				ILayeredFormAnimate layeredFormAnimate = value[i];
				if (layeredFormAnimate.ReadY < readY)
				{
					readY = layeredFormAnimate.ReadY;
				}
			}
			return readY;
		}
		return workingArea.Bottom - offset;
	}

	internal void SetPositionCenter(int w)
	{
		if (Align == TAlignFrom.Top || Align == TAlignFrom.Bottom)
		{
			int locationX = base.TargetRect.X + (w - base.TargetRect.Width) / 2;
			SetLocationX(locationX);
			start_X = (end_X = locationX);
		}
		else if (Align == TAlignFrom.TR || Align == TAlignFrom.BR)
		{
			int locationX2 = base.TargetRect.X - (base.TargetRect.Width - w);
			SetLocationX(locationX2);
			start_X = (end_X = locationX2);
		}
		Print();
	}

	internal void SetPositionY(int y)
	{
		SetLocationY(y);
		end_Y = y;
		start_Y = y - base.TargetRect.Height / 2;
	}

	internal void SetPositionYB(int y)
	{
		SetLocationY(y);
		end_Y = y;
		start_Y = y + base.TargetRect.Height / 2;
	}

	protected override void OnLoad(EventArgs e)
	{
		if (ActiveAnimation)
		{
			PlayAnimation();
		}
		base.OnLoad(e);
	}

	public void PlayAnimation()
	{
		if (Config.Animation)
		{
			int t = Animation.TotalFrames(10, 200);
			task_start = new ITask((start_X == end_X) ? ((Func<int, bool>)delegate(int i)
			{
				float num2 = Animation.Animate(i, t, 1f, AnimationType.Ball);
				SetAnimateValueY(start_Y + (int)((float)(end_Y - start_Y) * num2), (byte)(240f * num2));
				return true;
			}) : ((Func<int, bool>)delegate(int i)
			{
				float num = Animation.Animate(i, t, 1f, AnimationType.Ball);
				SetAnimateValueX(start_X + (int)((float)(end_X - start_X) * num), (byte)(240f * num));
				return true;
			}), 10, t, delegate
			{
				DisposeAnimation();
				SetAnimateValue(end_X, end_Y, 240);
			});
		}
		else
		{
			SetAnimateValue(end_X, end_Y, 240);
		}
	}

	internal ITask StopAnimation()
	{
		task_start?.Dispose();
		int t = Animation.TotalFrames(10, 200);
		return new ITask((start_X == end_X) ? ((Func<int, bool>)delegate(int i)
		{
			float num2 = Animation.Animate(i, t, 1f, AnimationType.Ball);
			SetAnimateValueY(end_Y - (int)((float)(end_Y - start_Y) * num2), (byte)(240f * (1f - num2)));
			return true;
		}) : ((Func<int, bool>)delegate(int i)
		{
			float num = Animation.Animate(i, t, 1f, AnimationType.Ball);
			SetAnimateValueX(end_X - (int)((float)(end_X - start_X) * num), (byte)(240f * (1f - num)));
			return true;
		}), 10, t, delegate
		{
			DisposeAnimation();
		});
	}

	public void DisposeAnimation()
	{
		Bitmap? obj = bmp_tmp;
		if (obj != null)
		{
			((Image)obj).Dispose();
		}
		bmp_tmp = null;
	}

	private void SetAnimateValueX(int x, byte _alpha)
	{
		if (base.TargetRect.X != x || alpha != _alpha)
		{
			SetLocationX(x);
			alpha = _alpha;
			if (bmp_tmp == null)
			{
				bmp_tmp = PrintBit();
			}
			if (bmp_tmp != null)
			{
				Print(bmp_tmp);
			}
		}
	}

	private void SetAnimateValueY(int y, byte _alpha)
	{
		if (base.TargetRect.Y != y || alpha != _alpha)
		{
			SetLocationY(y);
			alpha = _alpha;
			if (bmp_tmp == null)
			{
				bmp_tmp = PrintBit();
			}
			if (bmp_tmp != null)
			{
				Print(bmp_tmp);
			}
		}
	}

	internal void SetAnimateValueY(int y)
	{
		if (base.TargetRect.Y != y)
		{
			SetLocationY(y);
			if (bmp_tmp == null)
			{
				bmp_tmp = PrintBit();
			}
			if (bmp_tmp != null)
			{
				Print(bmp_tmp);
			}
		}
	}

	private void SetAnimateValue(int x, int y, byte _alpha)
	{
		if (base.TargetRect.X != x || base.TargetRect.Y != y || alpha != _alpha)
		{
			SetLocation(x, y);
			alpha = _alpha;
			if (bmp_tmp == null)
			{
				bmp_tmp = PrintBit();
			}
			if (bmp_tmp != null)
			{
				Print(bmp_tmp);
			}
		}
	}

	public void CloseMe()
	{
		DateTime now = DateTime.Now;
		if (!handclose || !((now - closetime).TotalSeconds < 2.0))
		{
			closetime = now;
			handclose = true;
			task_start?.Dispose();
			MsgQueue.Add(this);
		}
	}

	protected override void Dispose(bool disposing)
	{
		task_start?.Dispose();
		list[key].Remove(this);
		base.Dispose(disposing);
	}
}
