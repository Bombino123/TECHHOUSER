using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using AntdUI.Design;

namespace AntdUI;

[Description("Slider 滑动输入条")]
[ToolboxItem(true)]
[DefaultProperty("Value")]
[DefaultEvent("ValueChanged")]
public class Slider : IControl
{
	private Color? fill;

	private Color? trackColor;

	private int _minValue;

	private int _maxValue = 100;

	private int _value;

	private TooltipForm? tooltipForm;

	private string? tooltipText;

	private TAlignMini align = TAlignMini.Left;

	private int lineSize = 4;

	internal int dotSize = 10;

	internal int dotSizeActive = 12;

	private SliderMarkItemCollection? marks;

	internal Rectangle rect_read;

	private readonly StringFormat s_f = Helper.SF_NoWrap((StringAlignment)1, (StringAlignment)1);

	internal RectangleF rectEllipse;

	private bool mouseFlat;

	internal float AnimationHoverValue;

	internal bool AnimationHover;

	private bool _mouseHover;

	internal float AnimationDotHoverValue;

	internal bool AnimationDotHover;

	private bool _mouseDotHover;

	internal ITask? ThreadHover;

	private ITask? ThreadDotHover;

	[Description("颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? Fill
	{
		get
		{
			return fill;
		}
		set
		{
			if (!(fill == value))
			{
				fill = value;
				((Control)this).Invalidate();
			}
		}
	}

	[Description("悬停颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? FillHover { get; set; }

	[Description("滑轨颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? TrackColor
	{
		get
		{
			return trackColor;
		}
		set
		{
			if (!(trackColor == value))
			{
				trackColor = value;
				((Control)this).Invalidate();
			}
		}
	}

	[Description("激活颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? FillActive { get; set; }

	[Description("最小值")]
	[Category("数据")]
	[DefaultValue(0)]
	public int MinValue
	{
		get
		{
			return _minValue;
		}
		set
		{
			if (value <= _maxValue && _minValue != value)
			{
				_minValue = value;
				((Control)this).Invalidate();
			}
		}
	}

	[Description("最大值")]
	[Category("数据")]
	[DefaultValue(100)]
	public int MaxValue
	{
		get
		{
			return _maxValue;
		}
		set
		{
			if (value >= _minValue && value >= _value && _maxValue != value)
			{
				_maxValue = value;
				((Control)this).Invalidate();
			}
		}
	}

	[Description("当前值")]
	[Category("数据")]
	[DefaultValue(0)]
	public int Value
	{
		get
		{
			return _value;
		}
		set
		{
			if (value < _minValue)
			{
				value = _minValue;
			}
			else if (value > _maxValue)
			{
				value = _maxValue;
			}
			if (_value != value)
			{
				_value = value;
				this.ValueChanged?.Invoke(this, new IntEventArgs(_value));
				((Control)this).Invalidate();
				OnPropertyChanged("Value");
			}
		}
	}

	[Description("方向")]
	[Category("外观")]
	[DefaultValue(TAlignMini.Left)]
	public TAlignMini Align
	{
		get
		{
			return align;
		}
		set
		{
			if (align != value)
			{
				align = value;
				IOnSizeChanged();
				((Control)this).Invalidate();
			}
		}
	}

	[Description("是否显示数值")]
	[Category("行为")]
	[DefaultValue(false)]
	public bool ShowValue { get; set; }

	[Description("线条粗细")]
	[Category("外观")]
	[DefaultValue(4)]
	public int LineSize
	{
		get
		{
			return lineSize;
		}
		set
		{
			if (lineSize != value)
			{
				lineSize = value;
				((Control)this).Invalidate();
			}
		}
	}

	[Description("点大小")]
	[Category("外观")]
	[DefaultValue(10)]
	public int DotSize
	{
		get
		{
			return dotSize;
		}
		set
		{
			if (dotSize != value)
			{
				dotSize = value;
				((Control)this).Invalidate();
			}
		}
	}

	[Description("点激活大小")]
	[Category("外观")]
	[DefaultValue(12)]
	public int DotSizeActive
	{
		get
		{
			return dotSizeActive;
		}
		set
		{
			if (dotSizeActive != value)
			{
				dotSizeActive = value;
				((Control)this).Invalidate();
			}
		}
	}

	[Description("是否只能拖拽到刻度上")]
	[Category("数据")]
	[DefaultValue(false)]
	public bool Dots { get; set; }

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Description("刻度标记")]
	[Category("数据")]
	[DefaultValue(null)]
	public SliderMarkItemCollection Marks
	{
		get
		{
			if (marks == null)
			{
				marks = new SliderMarkItemCollection(this);
			}
			return marks;
		}
		set
		{
			marks = value.BindData(this);
		}
	}

	[Description("刻度文本间距")]
	[Category("外观")]
	[DefaultValue(4)]
	public int MarkTextGap { get; set; } = 4;


	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	internal bool ExtraMouseHover
	{
		get
		{
			return _mouseHover;
		}
		set
		{
			if (_mouseHover == value)
			{
				return;
			}
			_mouseHover = value;
			bool enabled = base.Enabled;
			SetCursor(value && enabled);
			if (Config.Animation)
			{
				ThreadHover?.Dispose();
				AnimationHover = true;
				if (value)
				{
					ThreadHover = new ITask((Control)(object)this, delegate
					{
						AnimationHoverValue = AnimationHoverValue.Calculate(0.1f);
						if (AnimationHoverValue > 1f)
						{
							AnimationHoverValue = 1f;
							return false;
						}
						((Control)this).Invalidate();
						return true;
					}, 10, delegate
					{
						AnimationHover = false;
						((Control)this).Invalidate();
					});
				}
				else
				{
					ThreadHover = new ITask((Control)(object)this, delegate
					{
						AnimationHoverValue = AnimationHoverValue.Calculate(-0.1f);
						if (AnimationHoverValue <= 0f)
						{
							AnimationHoverValue = 0f;
							return false;
						}
						((Control)this).Invalidate();
						return true;
					}, 10, delegate
					{
						AnimationHover = false;
						((Control)this).Invalidate();
					});
				}
			}
			((Control)this).Invalidate();
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	internal bool ExtraMouseDotHover
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
				ThreadDotHover?.Dispose();
				AnimationDotHover = true;
				if (value)
				{
					ThreadDotHover = new ITask((Control)(object)this, delegate
					{
						AnimationDotHoverValue = AnimationDotHoverValue.Calculate(0.1f);
						if (AnimationDotHoverValue > 1f)
						{
							AnimationDotHoverValue = 1f;
							return false;
						}
						((Control)this).Invalidate();
						return true;
					}, 10, delegate
					{
						AnimationDotHover = false;
						((Control)this).Invalidate();
					});
					return;
				}
				ThreadDotHover = new ITask((Control)(object)this, delegate
				{
					AnimationDotHoverValue = AnimationDotHoverValue.Calculate(-0.1f);
					if (AnimationDotHoverValue <= 0f)
					{
						AnimationDotHoverValue = 0f;
						return false;
					}
					((Control)this).Invalidate();
					return true;
				}, 10, delegate
				{
					AnimationDotHover = false;
					((Control)this).Invalidate();
				});
			}
			else
			{
				((Control)this).Invalidate();
			}
		}
	}

	[Description("Value格式化时发生")]
	[Category("行为")]
	public event ValueFormatEventHandler? ValueFormatChanged;

	[Description("Value 属性值更改时发生")]
	[Category("行为")]
	public event IntEventHandler? ValueChanged;

	internal void ShowTips(int Value, RectangleF dot_rect)
	{
		string text = ((this.ValueFormatChanged == null) ? Value.ToString() : this.ValueFormatChanged(this, new IntEventArgs(Value)));
		if (!(text == tooltipText) || tooltipForm == null)
		{
			tooltipText = text;
			Rectangle rectangle = ((Control)this).RectangleToScreen(((Control)this).ClientRectangle);
			Rectangle rect = new Rectangle(rectangle.X + (int)dot_rect.X, rectangle.Y + (int)dot_rect.Y, (int)dot_rect.Width, (int)dot_rect.Height);
			if (tooltipForm == null)
			{
				tooltipForm = new TooltipForm((Control)(object)this, rect, tooltipText, new TooltipConfig
				{
					Font = ((Control)this).Font,
					ArrowAlign = ((align == TAlignMini.Top || align == TAlignMini.Bottom) ? TAlign.Right : TAlign.Top)
				});
				((Form)tooltipForm).Show((IWin32Window)(object)this);
			}
			else
			{
				tooltipForm.SetText(rect, tooltipText);
			}
		}
	}

	internal void CloseTips()
	{
		tooltipForm?.IClose();
		tooltipForm = null;
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		Padding padding = ((Control)this).Padding;
		Rectangle rect = ((Control)this).ClientRectangle.PaddingRect(padding);
		if (rect.Width == 0 || rect.Height == 0)
		{
			return;
		}
		int num = (int)((float)lineSize * Config.Dpi);
		int num2 = (int)((float)((dotSizeActive > dotSize) ? dotSizeActive : dotSize) * Config.Dpi);
		int num3 = num2 * 2;
		if (align == TAlignMini.Top || align == TAlignMini.Bottom)
		{
			if (((Padding)(ref padding)).Top > num2 || ((Padding)(ref padding)).Bottom > num2)
			{
				if (((Padding)(ref padding)).Top > num2 && ((Padding)(ref padding)).Bottom > num2)
				{
					rect_read = new Rectangle(rect.X + (rect.Width - num) / 2, rect.Y, num, rect.Height);
				}
				else if (((Padding)(ref padding)).Top > num2)
				{
					rect_read = new Rectangle(rect.X + (rect.Width - num) / 2, rect.Y, num, rect.Height - num2);
				}
				else
				{
					rect_read = new Rectangle(rect.X + (rect.Width - num) / 2, rect.Y + num2, num, rect.Height - num2);
				}
			}
			else
			{
				rect_read = new Rectangle(rect.X + (rect.Width - num) / 2, rect.Y + num2, num, rect.Height - num3);
			}
		}
		else if (((Padding)(ref padding)).Left > num2 || ((Padding)(ref padding)).Right > num2)
		{
			if (((Padding)(ref padding)).Left > num2 && ((Padding)(ref padding)).Right > num2)
			{
				rect_read = new Rectangle(rect.X, rect.Y + (rect.Height - num) / 2, rect.Width, num);
			}
			else if (((Padding)(ref padding)).Left > num2)
			{
				rect_read = new Rectangle(rect.X, rect.Y + (rect.Height - num) / 2, rect.Width - num2, num);
			}
			else
			{
				rect_read = new Rectangle(rect.X + num2, rect.Y + (rect.Height - num) / 2, rect.Width - num2, num);
			}
		}
		else
		{
			rect_read = new Rectangle(rect.X + num2, rect.Y + (rect.Height - num) / 2, rect.Width - num3, num);
		}
		bool enabled = base.Enabled;
		Color color = ((!enabled) ? Colour.FillTertiary.Get("Slider") : (fill ?? Colour.InfoBorder.Get("Slider")));
		Color color_dot = ((!enabled) ? Colour.SliderHandleColorDisabled.Get("Slider") : (fill ?? Colour.InfoBorder.Get("Slider")));
		Color color_hover = FillHover ?? Colour.InfoHover.Get("Slider");
		Color color_active = FillActive ?? Colour.Primary.Get("Slider");
		Canvas g = e.Graphics.High();
		IPaint(g, rect, enabled, color, color_dot, color_hover, color_active);
		this.PaintBadge(g);
		((Control)this).OnPaint(e);
	}

	internal virtual void IPaint(Canvas g, Rectangle rect, bool enabled, Color color, Color color_dot, Color color_hover, Color color_active)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Expected O, but got Unknown
		float num = ProgValue(_value);
		GraphicsPath val = rect_read.RoundPath(rect_read.Height / 2);
		try
		{
			SolidBrush val2 = new SolidBrush(trackColor ?? Colour.FillQuaternary.Get("Slider"));
			try
			{
				g.Fill((Brush)(object)val2, val);
				if (AnimationHover)
				{
					g.Fill(Helper.ToColorN(AnimationHoverValue, val2.Color), val);
				}
				else if (ExtraMouseHover)
				{
					g.Fill((Brush)(object)val2, val);
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
			if (num > 0f)
			{
				g.SetClip(RectLine(rect_read, num));
				if (AnimationHover)
				{
					g.Fill(color, val);
					g.Fill(Helper.ToColor(255f * AnimationHoverValue, color_hover), val);
				}
				else
				{
					g.Fill(ExtraMouseHover ? color_hover : color, val);
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
			PaintMarksEllipse(g, rect, rect_read, val3, color, LineSize);
			PaintEllipse(g, rect, rect_read, num, val3, color_dot, color_hover, color_active, LineSize);
		}
		finally
		{
			((IDisposable)val3)?.Dispose();
		}
	}

	internal void PaintEllipse(Canvas g, Rectangle rect, RectangleF rect_read, float prog, SolidBrush brush, Color color, Color color_hover, Color color_active, int LineSize)
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
		rectEllipse = RectDot(rect, rect_read, prog, num2 + LineSize);
		RectangleF dot_rect = RectDot(rect, rect_read, prog, num + LineSize);
		if (ShowValue && ExtraMouseDotHover)
		{
			ShowTips(_value, dot_rect);
		}
		if (AnimationDotHover)
		{
			float num3 = (float)(num2 - num) * AnimationDotHoverValue;
			SolidBrush val = new SolidBrush(color_active.rgba(0.2f));
			try
			{
				g.FillEllipse((Brush)(object)val, RectDot(rect, rect_read, prog, (float)(num2 + LineSize) + (float)(LineSize * 2) * AnimationDotHoverValue));
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
		if (ExtraMouseDotHover)
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
			SolidBrush val7 = new SolidBrush(ExtraMouseHover ? color_hover : color);
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

	internal void PaintMarksEllipse(Canvas g, Rectangle rect, Rectangle rect_read, SolidBrush brush, Color color, int LineSize)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Expected O, but got Unknown
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
		if (marks == null || marks.Count <= 0)
		{
			return;
		}
		SolidBrush val = new SolidBrush(Colour.Text.Get("Slider"));
		try
		{
			int gap = (int)((float)MarkTextGap * Config.Dpi);
			int num = LineSize * 2;
			foreach (SliderMarkItem mark in marks)
			{
				float num2 = ProgValue(mark.Value);
				if (!string.IsNullOrWhiteSpace(mark.Text))
				{
					if (mark.Fore.HasValue)
					{
						SolidBrush val2 = new SolidBrush(mark.Fore.Value);
						try
						{
							g.String(mark.Text, ((Control)this).Font, (Brush)(object)val2, RectDotText(rect, rect_read, (int)num2, gap, g.MeasureString(mark.Text, ((Control)this).Font)), s_f);
						}
						finally
						{
							((IDisposable)val2)?.Dispose();
						}
					}
					else
					{
						g.String(mark.Text, ((Control)this).Font, (Brush)(object)val, RectDotText(rect, rect_read, (int)num2, gap, g.MeasureString(mark.Text, ((Control)this).Font)), s_f);
					}
				}
				SolidBrush val3 = new SolidBrush(color);
				try
				{
					g.FillEllipse((Brush)(object)val3, RectDot(rect, rect_read, num2, num));
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
				g.FillEllipse((Brush)(object)brush, RectDot(rect, rect_read, num2, LineSize));
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	internal float ProgValue(int val)
	{
		int num = _maxValue - _minValue;
		TAlignMini tAlignMini = align;
		if (tAlignMini == TAlignMini.Top || tAlignMini == TAlignMini.Bottom)
		{
			float num2 = rect_read.Height;
			if (val < _maxValue)
			{
				return num2 * ((float)(val - _minValue) * 1f / (float)num);
			}
			return num2;
		}
		float num3 = rect_read.Width;
		if (val < _maxValue)
		{
			return num3 * ((float)(val - _minValue) * 1f / (float)num);
		}
		return num3;
	}

	internal RectangleF RectLine(RectangleF rect, float prog)
	{
		return align switch
		{
			TAlignMini.Right => new RectangleF(rect.X + rect.Width - prog, rect.Y, prog, rect.Height), 
			TAlignMini.Top => new RectangleF(rect.X, rect.Y, rect.Width, prog), 
			TAlignMini.Bottom => new RectangleF(rect.X, rect.Y + rect.Height - prog, rect.Width, prog), 
			_ => new RectangleF(rect.X, rect.Y, prog, rect.Height), 
		};
	}

	internal RectangleF RectDot(Rectangle rect, RectangleF rect_read, float prog, float size)
	{
		return align switch
		{
			TAlignMini.Right => new RectangleF(rect_read.X + (rect_read.Width - prog - size / 2f), (float)rect.Y + ((float)rect.Height - size) / 2f, size, size), 
			TAlignMini.Top => new RectangleF((float)rect.X + ((float)rect.Width - size) / 2f, rect_read.Y + prog - size / 2f, size, size), 
			TAlignMini.Bottom => new RectangleF((float)rect.X + ((float)rect.Width - size) / 2f, rect_read.Y + (rect_read.Height - prog - size / 2f), size, size), 
			_ => new RectangleF(rect_read.X + prog - size / 2f, (float)rect.Y + ((float)rect.Height - size) / 2f, size, size), 
		};
	}

	internal Rectangle RectDotText(Rectangle rect, Rectangle rect_read, int prog, int gap, Size size)
	{
		return align switch
		{
			TAlignMini.Right => new Rectangle(rect_read.X + (rect_read.Width - prog - size.Width / 2), rect_read.Bottom + rect_read.Height + gap, size.Width, size.Height), 
			TAlignMini.Top => new Rectangle(rect_read.Right + rect_read.Width + gap, rect_read.Y + prog - size.Height / 2, size.Width, size.Height), 
			TAlignMini.Bottom => new Rectangle(rect_read.Right + rect_read.Width + gap, rect_read.Y + (rect_read.Height - prog - size.Height / 2), size.Width, size.Height), 
			_ => new Rectangle(rect_read.X + prog - size.Width / 2, rect_read.Bottom + rect_read.Height + gap, size.Width, size.Height), 
		};
	}

	internal RectangleF RectDotH(Rectangle rect, Rectangle rect_read, float prog, int DotSize)
	{
		return align switch
		{
			TAlignMini.Right => new RectangleF((float)rect_read.X + ((float)rect_read.Width - prog - (float)DotSize / 2f), rect.Y, DotSize, rect.Height), 
			TAlignMini.Top => new RectangleF(rect.X, (float)rect_read.Y + prog - (float)DotSize / 2f, rect.Width, DotSize), 
			TAlignMini.Bottom => new RectangleF(rect.X, (float)rect_read.Y + ((float)rect_read.Height - prog - (float)DotSize / 2f), rect.Width, DotSize), 
			_ => new RectangleF((float)rect_read.X + prog - (float)DotSize / 2f, rect.Y, DotSize, rect.Height), 
		};
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		((Control)this).OnMouseDown(e);
		if ((int)e.Button == 1048576)
		{
			Value = FindIndex(e.X, e.Y, mark: true);
			mouseFlat = true;
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		((Control)this).OnMouseMove(e);
		if (mouseFlat)
		{
			ExtraMouseDotHover = true;
			Value = FindIndex(e.X, e.Y, mark: false);
		}
		else
		{
			ExtraMouseDotHover = rectEllipse.Contains(e.X, e.Y);
		}
	}

	internal int FindIndex(int x, int y, bool mark)
	{
		int num = _maxValue - _minValue;
		if (marks != null && marks.Count > 0)
		{
			if (Dots)
			{
				_ = ((Control)this).ClientRectangle;
				_ = dotSize;
				_ = Config.Dpi;
				List<float> list = new List<float>(marks.Count);
				int num2 = 0;
				switch (align)
				{
				case TAlignMini.Right:
					foreach (SliderMarkItem mark2 in marks)
					{
						list.Add((float)rect_read.X + ((float)rect_read.Width - ((mark2.Value >= _maxValue) ? ((float)rect_read.Width) : ((float)rect_read.Width * ((float)(mark2.Value - _minValue) * 1f / (float)num)))));
					}
					num2 = FindNumber(x, list);
					break;
				case TAlignMini.Top:
					foreach (SliderMarkItem mark3 in marks)
					{
						list.Add((float)rect_read.Y + ((mark3.Value >= _maxValue) ? ((float)rect_read.Height) : ((float)rect_read.Height * ((float)(mark3.Value - _minValue) * 1f / (float)num))));
					}
					num2 = FindNumber(y, list);
					break;
				case TAlignMini.Bottom:
					foreach (SliderMarkItem mark4 in marks)
					{
						list.Add((float)rect_read.Y + ((float)rect_read.Height - ((mark4.Value >= _maxValue) ? ((float)rect_read.Height) : ((float)rect_read.Height * ((float)(mark4.Value - _minValue) * 1f / (float)num)))));
					}
					num2 = FindNumber(y, list);
					break;
				default:
					foreach (SliderMarkItem mark5 in marks)
					{
						list.Add((float)rect_read.X + ((mark5.Value >= _maxValue) ? ((float)rect_read.Width) : ((float)rect_read.Width * ((float)(mark5.Value - _minValue) * 1f / (float)num))));
					}
					num2 = FindNumber(x, list);
					break;
				}
				return marks[num2].Value;
			}
			if (mark)
			{
				Rectangle clientRectangle = ((Control)this).ClientRectangle;
				int num3 = (int)((float)dotSize * Config.Dpi);
				foreach (SliderMarkItem mark6 in marks)
				{
					float prog = ProgValue(mark6.Value);
					if (RectDotH(clientRectangle, rect_read, prog, num3).Contains(x, y))
					{
						return mark6.Value;
					}
				}
			}
		}
		switch (align)
		{
		case TAlignMini.Right:
		{
			float num6 = 1f - (float)(x - rect_read.X) * 1f / (float)rect_read.Width;
			if (num6 > 0f)
			{
				return (int)Math.Round(num6 * (float)num) + _minValue;
			}
			return _minValue;
		}
		case TAlignMini.Top:
		{
			float num5 = (float)(y - rect_read.Y) * 1f / (float)rect_read.Height;
			if (num5 > 0f)
			{
				return (int)Math.Round(num5 * (float)num) + _minValue;
			}
			return _minValue;
		}
		case TAlignMini.Bottom:
		{
			float num7 = 1f - (float)(y - rect_read.Y) * 1f / (float)rect_read.Height;
			if (num7 > 0f)
			{
				return (int)Math.Round(num7 * (float)num) + _minValue;
			}
			return _minValue;
		}
		default:
		{
			float num4 = (float)(x - rect_read.X) * 1f / (float)rect_read.Width;
			if (num4 > 0f)
			{
				return (int)Math.Round(num4 * (float)num) + _minValue;
			}
			return _minValue;
		}
		}
	}

	internal int FindNumber(int target, List<float> array)
	{
		int result = 0;
		float num = 2.1474836E+09f;
		for (int i = 0; i < array.Count; i++)
		{
			float num2 = Math.Abs((float)target - array[i]);
			if (num2 < num)
			{
				num = num2;
				result = i;
			}
		}
		return result;
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		((Control)this).OnMouseUp(e);
		mouseFlat = false;
		((Control)this).Invalidate();
	}

	protected override void Dispose(bool disposing)
	{
		ThreadHover?.Dispose();
		ThreadDotHover?.Dispose();
		base.Dispose(disposing);
	}

	protected override void OnMouseEnter(EventArgs e)
	{
		((Control)this).OnMouseEnter(e);
		ExtraMouseHover = true;
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		((Control)this).OnMouseLeave(e);
		CloseTips();
		bool extraMouseHover = (ExtraMouseDotHover = false);
		ExtraMouseHover = extraMouseHover;
	}

	protected override void OnLeave(EventArgs e)
	{
		((Control)this).OnLeave(e);
		CloseTips();
		bool extraMouseHover = (ExtraMouseDotHover = false);
		ExtraMouseHover = extraMouseHover;
	}
}
