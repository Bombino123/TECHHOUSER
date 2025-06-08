using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AntdUI;

public abstract class ILayeredFormOpacityDown : ILayeredForm
{
	private ITask? task_start;

	private bool run_end;

	private bool ok_end;

	public bool Inverted;

	private Bitmap? bmp_tmp;

	public bool RunAnimation = true;

	private int AnimateY = -1;

	private int AnimateHeight = -1;

	public override bool MessageEnable => true;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	public override bool CanLoadMessage { get; set; }

	protected override void OnLoad(EventArgs e)
	{
		if (Config.Animation)
		{
			int t = Animation.TotalFrames(10, 100);
			if (Inverted)
			{
				int _y = base.TargetRect.Y;
				int _height2 = base.TargetRect.Height;
				task_start = new ITask(delegate(int i)
				{
					float num2 = Animation.Animate(i, t, 1f, AnimationType.Ball);
					int num3 = (int)((float)_height2 * num2);
					SetAnimateValue(_y + (_height2 - num3), num3, num2);
					return true;
				}, 10, t, delegate
				{
					DisposeTmp();
					alpha = byte.MaxValue;
					AnimateHeight = -1;
					RunAnimation = false;
					Print();
					LoadOK();
				});
			}
			else
			{
				int _height = base.TargetRect.Height;
				task_start = new ITask(delegate(int i)
				{
					float num = Animation.Animate(i, t, 1f, AnimationType.Ball);
					_ = _height;
					SetAnimateValue((int)((float)_height * num), num);
					return true;
				}, 10, t, delegate
				{
					DisposeTmp();
					alpha = byte.MaxValue;
					AnimateHeight = -1;
					RunAnimation = false;
					Print();
					LoadOK();
				});
			}
		}
		else
		{
			alpha = byte.MaxValue;
			RunAnimation = false;
			Print();
			LoadOK();
		}
		base.OnLoad(e);
	}

	internal void DisposeTmp()
	{
		Bitmap? obj = bmp_tmp;
		if (obj != null)
		{
			((Image)obj).Dispose();
		}
		bmp_tmp = null;
	}

	private void SetAnimateValue(int y, int height, float alpha)
	{
		SetAnimateValue(y, height, (byte)(255f * alpha));
	}

	private void SetAnimateValue(int height, float alpha)
	{
		SetAnimateValue(height, (byte)(255f * alpha));
	}

	private void SetAnimateValue(int y, int height, byte _alpha)
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		if (AnimateY == y && AnimateHeight == height && alpha == _alpha)
		{
			return;
		}
		AnimateY = y;
		AnimateHeight = height;
		alpha = _alpha;
		if (height == 0)
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
				Rectangle rect = new Rectangle(base.TargetRect.X, y, base.TargetRect.Width, height);
				Bitmap val = new Bitmap(rect.Width, rect.Height);
				Graphics val2 = Graphics.FromImage((Image)(object)val);
				try
				{
					val2.DrawImage((Image)(object)bmp_tmp, 0, 0, rect.Width, rect.Height);
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
				Print(val, rect);
			}
		}
		catch
		{
		}
	}

	private void SetAnimateValue(int height, byte _alpha)
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		if (AnimateHeight == height && alpha == _alpha)
		{
			return;
		}
		AnimateHeight = height;
		alpha = _alpha;
		if (height == 0)
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
				Rectangle rect = new Rectangle(base.TargetRect.X, base.TargetRect.Y, base.TargetRect.Width, height);
				Bitmap val = new Bitmap(rect.Width, rect.Height);
				Graphics val2 = Graphics.FromImage((Image)(object)val);
				try
				{
					val2.DrawImage((Image)(object)bmp_tmp, 0, 0, rect.Width, rect.Height);
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
				Print(val, rect);
			}
		}
		catch
		{
		}
	}

	public virtual void LoadOK()
	{
		CanLoadMessage = true;
		LoadMessage();
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
					RunAnimation = true;
					int t = Animation.TotalFrames(10, 100);
					if (Inverted)
					{
						int _y = base.TargetRect.Y;
						int _height2 = base.TargetRect.Height;
						new ITask(delegate(int i)
						{
							float num2 = 1f - Animation.Animate(i, t, 1f, AnimationType.Ball);
							int num3 = (int)((float)_height2 * num2);
							SetAnimateValue(_y + (_height2 - num3), num3, num2);
							return true;
						}, 10, t, delegate
						{
							DisposeTmp();
							ok_end = true;
							IClose(isdispose: true);
						});
					}
					else
					{
						int _height = base.TargetRect.Height;
						new ITask(delegate(int i)
						{
							float num = 1f - Animation.Animate(i, t, 1f, AnimationType.Ball);
							SetAnimateValue((int)((float)_height * num), num);
							return true;
						}, 10, t, delegate
						{
							DisposeTmp();
							ok_end = true;
							IClose(isdispose: true);
						});
					}
				}
			}
			else
			{
				ok_end = true;
				IClose(isdispose: true);
			}
		}
		((Form)this).OnClosing(e);
	}

	protected override void Dispose(bool disposing)
	{
		task_start?.Dispose();
		base.Dispose(disposing);
	}

	public void CLocation(Point Point, TAlignFrom Placement, bool DropDownArrow, int Padding, int Width, int Height, Rectangle Rect, ref bool Inverted, ref TAlign ArrowAlign, bool Collision = false)
	{
		switch (Placement)
		{
		case TAlignFrom.Top:
			Inverted = true;
			if (DropDownArrow)
			{
				ArrowAlign = TAlign.Top;
			}
			SetLocation(Point.X + Rect.X + (Rect.Width - Width) / 2, Point.Y - Height + Rect.Y);
			return;
		case TAlignFrom.TL:
		{
			Inverted = true;
			if (DropDownArrow)
			{
				ArrowAlign = TAlign.TL;
			}
			int num2 = Point.X + Rect.X - Padding;
			int y2 = Point.Y - Height + Rect.Y;
			SetLocation(num2, y2);
			if (!Collision)
			{
				return;
			}
			Rectangle workingArea = Screen.FromPoint(base.TargetRect.Location).WorkingArea;
			if (num2 > workingArea.X + workingArea.Width - base.TargetRect.Width)
			{
				if (DropDownArrow)
				{
					ArrowAlign = TAlign.TR;
				}
				num2 = Point.X + (Rect.X + Rect.Width) - Width + Padding;
				SetLocation(num2, y2);
			}
			return;
		}
		case TAlignFrom.TR:
		{
			Inverted = true;
			if (DropDownArrow)
			{
				ArrowAlign = TAlign.TR;
			}
			int num3 = Point.X + (Rect.X + Rect.Width) - Width + Padding;
			int y3 = Point.Y - Height + Rect.Y;
			SetLocation(num3, y3);
			if (!Collision)
			{
				return;
			}
			_ = Screen.FromPoint(base.TargetRect.Location).WorkingArea;
			if (num3 < 0)
			{
				if (DropDownArrow)
				{
					ArrowAlign = TAlign.TL;
				}
				num3 = Point.X + Rect.X - Padding;
				SetLocation(num3, y3);
			}
			return;
		}
		case TAlignFrom.Bottom:
			if (DropDownArrow)
			{
				ArrowAlign = TAlign.Bottom;
			}
			SetLocation(Point.X + Rect.X + (Rect.Width - Width) / 2, Point.Y + Rect.Bottom);
			return;
		case TAlignFrom.BR:
		{
			if (DropDownArrow)
			{
				ArrowAlign = TAlign.BR;
			}
			int num = Point.X + (Rect.X + Rect.Width) - Width + Padding;
			int y = Point.Y + Rect.Bottom;
			SetLocation(num, y);
			if (!Collision)
			{
				return;
			}
			_ = Screen.FromPoint(base.TargetRect.Location).WorkingArea;
			if (num < 0)
			{
				if (DropDownArrow)
				{
					ArrowAlign = TAlign.BL;
				}
				num = Point.X + Rect.X - Padding;
				SetLocation(num, y);
			}
			return;
		}
		}
		if (DropDownArrow)
		{
			ArrowAlign = TAlign.BL;
		}
		int num4 = Point.X + Rect.X - Padding;
		int y4 = Point.Y + Rect.Bottom;
		SetLocation(num4, y4);
		if (!Collision)
		{
			return;
		}
		Rectangle workingArea2 = Screen.FromPoint(base.TargetRect.Location).WorkingArea;
		if (num4 > workingArea2.X + workingArea2.Width - base.TargetRect.Width)
		{
			if (DropDownArrow)
			{
				ArrowAlign = TAlign.BR;
			}
			num4 = Point.X + (Rect.X + Rect.Width) - Width + Padding;
			SetLocation(num4, y4);
		}
	}

	public PointF[]? CLocation(Point Point, TAlignFrom Placement, bool DropDownArrow, int ArrowSize, int Padding, int Width, int Height, Rectangle Rect, ref bool Inverted, ref TAlign ArrowAlign, bool Collision = false)
	{
		CLocation(Point, Placement, DropDownArrow, Padding, Width, Height, Rect, ref Inverted, ref ArrowAlign, Collision);
		if (Rect.Height >= Rect.Width)
		{
			switch (Placement)
			{
			case TAlignFrom.TL:
			{
				if (ArrowAlign != TAlign.TR)
				{
					int num9 = Padding + Rect.Width / 2;
					int num10 = Height - Padding;
					return new PointF[3]
					{
						new PointF(num9 - ArrowSize, num10),
						new PointF(num9 + ArrowSize, num10),
						new PointF(num9, num10 + ArrowSize)
					};
				}
				int num11 = Width - Rect.Width - Padding + Rect.Width / 2;
				int num12 = Height - Padding;
				return new PointF[3]
				{
					new PointF(num11 - ArrowSize, num12),
					new PointF(num11 + ArrowSize, num12),
					new PointF(num11, num12 + ArrowSize)
				};
			}
			case TAlignFrom.TR:
			{
				if (ArrowAlign != TAlign.TL)
				{
					int num5 = Width - Rect.Width - Padding + Rect.Width / 2;
					int num6 = Height - Padding;
					return new PointF[3]
					{
						new PointF(num5 - ArrowSize, num6),
						new PointF(num5 + ArrowSize, num6),
						new PointF(num5, num6 + ArrowSize)
					};
				}
				int num7 = Padding + Rect.Width / 2;
				int num8 = Height - Padding;
				return new PointF[3]
				{
					new PointF(num7 - ArrowSize, num8),
					new PointF(num7 + ArrowSize, num8),
					new PointF(num7, num8 + ArrowSize)
				};
			}
			case TAlignFrom.BR:
			{
				if (ArrowAlign != TAlign.BL)
				{
					int num13 = Width - Rect.Width - Padding + Rect.Width / 2;
					int num14 = Padding - ArrowSize;
					return new PointF[3]
					{
						new PointF(num13, num14),
						new PointF(num13 - ArrowSize, num14 + ArrowSize),
						new PointF(num13 + ArrowSize, num14 + ArrowSize)
					};
				}
				int num15 = Padding + Rect.Width / 2;
				int num16 = Padding - ArrowSize;
				return new PointF[3]
				{
					new PointF(num15, num16),
					new PointF(num15 - ArrowSize, num16 + ArrowSize),
					new PointF(num15 + ArrowSize, num16 + ArrowSize)
				};
			}
			default:
			{
				if (ArrowAlign != TAlign.BR)
				{
					int num = Padding + Rect.Width / 2;
					int num2 = Padding - ArrowSize;
					return new PointF[3]
					{
						new PointF(num, num2),
						new PointF(num - ArrowSize, num2 + ArrowSize),
						new PointF(num + ArrowSize, num2 + ArrowSize)
					};
				}
				int num3 = Width - Rect.Width - Padding + Rect.Width / 2;
				int num4 = Padding - ArrowSize;
				return new PointF[3]
				{
					new PointF(num3, num4),
					new PointF(num3 - ArrowSize, num4 + ArrowSize),
					new PointF(num3 + ArrowSize, num4 + ArrowSize)
				};
			}
			}
		}
		return null;
	}
}
