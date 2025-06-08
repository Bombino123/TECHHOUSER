using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AntdUI;

[Description("SliderRange 滑动范围输入条")]
[ToolboxItem(true)]
[DefaultProperty("Value")]
[DefaultEvent("ValueChanged")]
public class SliderRange : Slider
{
	private int _value2 = 10;

	private RectangleF rectEllipse2;

	private bool mouseFlat;

	internal float AnimationDot2HoverValue;

	internal bool AnimationDot2Hover;

	private bool _mouseDotHover;

	private ITask? ThreadDot2Hover;

	[Description("当前值2")]
	[Category("数据")]
	[DefaultValue(10)]
	public int Value2
	{
		get
		{
			return _value2;
		}
		set
		{
			if (value < base.MinValue)
			{
				value = base.MinValue;
			}
			else if (value > base.MaxValue)
			{
				value = base.MaxValue;
			}
			if (_value2 != value)
			{
				_value2 = value;
				this.Value2Changed?.Invoke(this, new IntEventArgs(_value2));
				((Control)this).Invalidate();
				OnPropertyChanged("Value2");
			}
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	internal bool ExtraMouseDot2Hover
	{
		get
		{
			return _mouseDotHover;
		}
		set
		{
			if (_mouseDotHover == value)
			{
				return;
			}
			_mouseDotHover = value;
			if (!value)
			{
				CloseTips();
			}
			if (Config.Animation)
			{
				ThreadHover?.Dispose();
				ThreadHover = null;
				ThreadDot2Hover?.Dispose();
				AnimationDot2Hover = true;
				if (value)
				{
					ThreadDot2Hover = new ITask((Control)(object)this, delegate
					{
						AnimationDot2HoverValue = AnimationDot2HoverValue.Calculate(0.1f);
						if (AnimationDot2HoverValue > 1f)
						{
							AnimationDot2HoverValue = 1f;
							return false;
						}
						((Control)this).Invalidate();
						return true;
					}, 10, delegate
					{
						AnimationDot2Hover = false;
						((Control)this).Invalidate();
					});
					return;
				}
				ThreadDot2Hover = new ITask((Control)(object)this, delegate
				{
					AnimationDot2HoverValue = AnimationDot2HoverValue.Calculate(-0.1f);
					if (AnimationDot2HoverValue <= 0f)
					{
						AnimationDot2HoverValue = 0f;
						return false;
					}
					((Control)this).Invalidate();
					return true;
				}, 10, delegate
				{
					AnimationDot2Hover = false;
					((Control)this).Invalidate();
				});
			}
			else
			{
				((Control)this).Invalidate();
			}
		}
	}

	[Description("Value 属性值更改时发生")]
	[Category("行为")]
	public event IntEventHandler? Value2Changed;

	internal override void IPaint(Canvas g, Rectangle rect, bool enabled, Color color, Color color_dot, Color color_hover, Color color_active)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Expected O, but got Unknown
		float num = ProgValue(base.Value);
		float num2 = ProgValue(_value2);
		GraphicsPath val = rect_read.RoundPath(rect_read.Height / 2);
		try
		{
			SolidBrush val2 = new SolidBrush(Colour.FillQuaternary.Get("Slider"));
			try
			{
				g.Fill((Brush)(object)val2, val);
				if (AnimationHover)
				{
					g.Fill(Helper.ToColorN(AnimationHoverValue, val2.Color), val);
				}
				else if (base.ExtraMouseHover)
				{
					g.Fill((Brush)(object)val2, val);
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
			if (num != num2)
			{
				g.SetClip(RectLine(rect_read, num, num2));
				if (AnimationHover)
				{
					g.Fill(color, val);
					g.Fill(Helper.ToColor(255f * AnimationHoverValue, color_hover), val);
				}
				else
				{
					g.Fill(base.ExtraMouseHover ? color_hover : color, val);
				}
				g.ResetClip();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		SolidBrush val3 = new SolidBrush(Colour.BgBase.Get("Slider"));
		try
		{
			PaintMarksEllipse(g, rect, rect_read, val3, color, base.LineSize);
			PaintEllipse(g, rect, rect_read, num, val3, color_dot, color_hover, color_active, base.LineSize);
			if (num != num2)
			{
				PaintEllipse2(g, rect, rect_read, num2, val3, color_dot, color_hover, color_active, base.LineSize);
			}
		}
		finally
		{
			((IDisposable)val3)?.Dispose();
		}
	}

	internal void PaintEllipse2(Canvas g, Rectangle rect, RectangleF rect_read, float prog, SolidBrush brush, Color color, Color color_hover, Color color_active, int LineSize)
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Expected O, but got Unknown
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Expected O, but got Unknown
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Expected O, but got Unknown
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Expected O, but got Unknown
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Expected O, but got Unknown
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Expected O, but got Unknown
		int num = (int)((float)dotSize * Config.Dpi);
		int num2 = (int)((float)dotSizeActive * Config.Dpi);
		rectEllipse2 = RectDot(rect, rect_read, prog, num2 + LineSize);
		RectangleF dot_rect = RectDot(rect, rect_read, prog, num + LineSize);
		if (base.ShowValue && ExtraMouseDot2Hover)
		{
			ShowTips(_value2, dot_rect);
		}
		if (AnimationDot2Hover)
		{
			float num3 = (float)(num2 - num) * AnimationDot2HoverValue;
			SolidBrush val = new SolidBrush(color_active.rgba(0.2f));
			try
			{
				g.FillEllipse((Brush)(object)val, RectDot(rect, rect_read, prog, (float)(num2 + LineSize) + (float)(LineSize * 2) * AnimationDot2HoverValue));
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
			SolidBrush val2 = new SolidBrush(color_active);
			try
			{
				g.FillEllipse((Brush)(object)val2, RectDot(rect, rect_read, prog, (float)(num + LineSize) + num3));
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
			g.FillEllipse((Brush)(object)brush, RectDot(rect, rect_read, prog, (float)num + num3));
			return;
		}
		if (ExtraMouseDot2Hover)
		{
			SolidBrush val3 = new SolidBrush(color_active.rgba(0.2f));
			try
			{
				g.FillEllipse((Brush)(object)val3, RectDot(rect, rect_read, prog, num2 + LineSize * 3));
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
			SolidBrush val4 = new SolidBrush(color_active);
			try
			{
				g.FillEllipse((Brush)(object)val4, RectDot(rect, rect_read, prog, num2 + LineSize));
			}
			finally
			{
				((IDisposable)val4)?.Dispose();
			}
			g.FillEllipse((Brush)(object)brush, RectDot(rect, rect_read, prog, num2));
			return;
		}
		if (AnimationHover)
		{
			SolidBrush val5 = new SolidBrush(color);
			try
			{
				SolidBrush val6 = new SolidBrush(Helper.ToColor(255f * AnimationHoverValue, color_hover));
				try
				{
					RectangleF rect2 = RectDot(rect, rect_read, prog, num + LineSize);
					g.FillEllipse((Brush)(object)val5, rect2);
					g.FillEllipse((Brush)(object)val6, rect2);
				}
				finally
				{
					((IDisposable)val6)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)val5)?.Dispose();
			}
		}
		else
		{
			SolidBrush val7 = new SolidBrush(base.ExtraMouseHover ? color_hover : color);
			try
			{
				g.FillEllipse((Brush)(object)val7, RectDot(rect, rect_read, prog, num + LineSize));
			}
			finally
			{
				((IDisposable)val7)?.Dispose();
			}
		}
		g.FillEllipse((Brush)(object)brush, RectDot(rect, rect_read, prog, num));
	}

	internal RectangleF RectLine(RectangleF rect, float prog, float prog2)
	{
		return base.Align switch
		{
			TAlignMini.Right => new RectangleF(rect.X + rect.Width - prog2, rect.Y, prog2 - prog, rect.Height), 
			TAlignMini.Top => new RectangleF(rect.X, rect.Y + prog, rect.Width, prog2 - prog), 
			TAlignMini.Bottom => new RectangleF(rect.X, rect.Y + rect.Height - prog2, rect.Width, prog2 - prog), 
			_ => new RectangleF(rect.X + prog, rect.Y, prog2 - prog, rect.Height), 
		};
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		if ((int)e.Button == 1048576)
		{
			if (rectEllipse.Contains(e.X, e.Y))
			{
				base.OnMouseDown(e);
				return;
			}
			if (rectEllipse2.Contains(e.X, e.Y))
			{
				Value2 = FindIndex(e.X, e.Y, mark: true);
				mouseFlat = true;
				return;
			}
			List<float> list = new List<float>(2);
			int num = 0;
			int num2 = base.MaxValue - base.MinValue;
			switch (base.Align)
			{
			case TAlignMini.Right:
				list.Add((float)rect_read.X + ((float)rect_read.Width - ((base.Value >= base.MaxValue) ? ((float)rect_read.Width) : ((float)rect_read.Width * ((float)(base.Value - base.MinValue) * 1f / (float)num2)))));
				list.Add((float)rect_read.X + ((float)rect_read.Width - ((_value2 >= base.MaxValue) ? ((float)rect_read.Width) : ((float)rect_read.Width * ((float)(_value2 - base.MinValue) * 1f / (float)num2)))));
				num = FindNumber(e.X, list);
				break;
			case TAlignMini.Top:
				list.Add((float)rect_read.Y + ((base.Value >= base.MaxValue) ? ((float)rect_read.Height) : ((float)rect_read.Height * ((float)(base.Value - base.MinValue) * 1f / (float)num2))));
				list.Add((float)rect_read.Y + ((_value2 >= base.MaxValue) ? ((float)rect_read.Height) : ((float)rect_read.Height * ((float)(_value2 - base.MinValue) * 1f / (float)num2))));
				num = FindNumber(e.Y, list);
				break;
			case TAlignMini.Bottom:
				list.Add((float)rect_read.Y + ((float)rect_read.Height - ((base.Value >= base.MaxValue) ? ((float)rect_read.Height) : ((float)rect_read.Height * ((float)(base.Value - base.MinValue) * 1f / (float)num2)))));
				list.Add((float)rect_read.Y + ((float)rect_read.Height - ((_value2 >= base.MaxValue) ? ((float)rect_read.Height) : ((float)rect_read.Height * ((float)(_value2 - base.MinValue) * 1f / (float)num2)))));
				num = FindNumber(e.Y, list);
				break;
			default:
				list.Add((float)rect_read.X + ((base.Value >= base.MaxValue) ? ((float)rect_read.Width) : ((float)rect_read.Width * ((float)(base.Value - base.MinValue) * 1f / (float)num2))));
				list.Add((float)rect_read.X + ((_value2 >= base.MaxValue) ? ((float)rect_read.Width) : ((float)rect_read.Width * ((float)(_value2 - base.MinValue) * 1f / (float)num2))));
				num = FindNumber(e.X, list);
				break;
			}
			if (num == 1)
			{
				Value2 = FindIndex(e.X, e.Y, mark: true);
				mouseFlat = true;
				return;
			}
		}
		base.OnMouseDown(e);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		if (mouseFlat)
		{
			ExtraMouseDot2Hover = true;
			Value2 = FindIndex(e.X, e.Y, mark: false);
		}
		else
		{
			ExtraMouseDot2Hover = rectEllipse2.Contains(e.X, e.Y);
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		base.OnMouseUp(e);
		mouseFlat = false;
		((Control)this).Invalidate();
	}

	protected override void Dispose(bool disposing)
	{
		ThreadDot2Hover?.Dispose();
		base.Dispose(disposing);
	}
}
