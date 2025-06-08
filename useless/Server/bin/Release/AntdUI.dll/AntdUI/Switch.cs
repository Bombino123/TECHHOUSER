using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using AntdUI.Design;

namespace AntdUI;

[Description("Switch 开关")]
[ToolboxItem(true)]
[DefaultProperty("Checked")]
[DefaultEvent("CheckedChanged")]
public class Switch : IControl
{
	private Color? fore;

	private Color? fill;

	private bool AnimationCheck;

	private float AnimationCheckValue;

	private bool _checked;

	private string? _checkedText;

	private string? _unCheckedText;

	private bool loading;

	private ITask? ThreadLoading;

	internal float LineWidth = 6f;

	internal float LineAngle;

	private bool init;

	private float AnimationHoverValue;

	private bool AnimationHover;

	private bool _mouseHover;

	private ITask? ThreadHover;

	private ITask? ThreadCheck;

	private ITask? ThreadClick;

	private bool AnimationClick;

	private float AnimationClickValue;

	private bool hasFocus;

	[Description("文字颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ForeColor
	{
		get
		{
			return fore;
		}
		set
		{
			if (!(fore == value))
			{
				fore = value;
				((Control)this).Invalidate();
				OnPropertyChanged("ForeColor");
			}
		}
	}

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
				OnPropertyChanged("Fill");
			}
		}
	}

	[Description("悬停颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? FillHover { get; set; }

	[Description("选中状态")]
	[Category("数据")]
	[DefaultValue(false)]
	public bool Checked
	{
		get
		{
			return _checked;
		}
		set
		{
			if (_checked == value)
			{
				return;
			}
			_checked = value;
			ThreadCheck?.Dispose();
			if (((Control)this).IsHandleCreated && Config.Animation)
			{
				AnimationCheck = true;
				if (value)
				{
					ThreadCheck = new ITask((Control)(object)this, delegate
					{
						AnimationCheckValue = AnimationCheckValue.Calculate(0.1f);
						if (AnimationCheckValue > 1f)
						{
							AnimationCheckValue = 1f;
							return false;
						}
						((Control)this).Invalidate();
						return true;
					}, 10, delegate
					{
						AnimationCheck = false;
						((Control)this).Invalidate();
					});
				}
				else
				{
					ThreadCheck = new ITask((Control)(object)this, delegate
					{
						AnimationCheckValue = AnimationCheckValue.Calculate(-0.1f);
						if (AnimationCheckValue <= 0f)
						{
							AnimationCheckValue = 0f;
							return false;
						}
						((Control)this).Invalidate();
						return true;
					}, 10, delegate
					{
						AnimationCheck = false;
						((Control)this).Invalidate();
					});
				}
			}
			else
			{
				AnimationCheckValue = (value ? 1f : 0f);
			}
			((Control)this).Invalidate();
			this.CheckedChanged?.Invoke(this, new BoolEventArgs(value));
			OnPropertyChanged("Checked");
		}
	}

	[Description("点击时自动改变选中状态")]
	[Category("行为")]
	[DefaultValue(true)]
	public bool AutoCheck { get; set; } = true;


	[Description("波浪大小")]
	[Category("外观")]
	[DefaultValue(4)]
	public int WaveSize { get; set; } = 4;


	[Description("间距")]
	[Category("外观")]
	[DefaultValue(2)]
	public int Gap { get; set; } = 2;


	[Description("选中时显示的文本")]
	[Category("外观")]
	[DefaultValue(null)]
	[Localizable(true)]
	public string? CheckedText
	{
		get
		{
			return ((Control)(object)this).GetLangI(LocalizationCheckedText, _checkedText);
		}
		set
		{
			if (!(_checkedText == value))
			{
				_checkedText = value;
				if (_checked)
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("CheckedText");
			}
		}
	}

	[Description("选中时显示的文本")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationCheckedText { get; set; }

	[Description("未选中时显示的文本")]
	[Category("外观")]
	[DefaultValue(null)]
	[Localizable(true)]
	public string? UnCheckedText
	{
		get
		{
			return ((Control)(object)this).GetLangI(LocalizationTextUnCheckedText, _unCheckedText);
		}
		set
		{
			if (!(_unCheckedText == value))
			{
				_unCheckedText = value;
				if (!_checked)
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("UnCheckedText");
			}
		}
	}

	[Description("未选中时显示的文本")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationTextUnCheckedText { get; set; }

	[Description("加载中")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool Loading
	{
		get
		{
			return loading;
		}
		set
		{
			if (loading == value)
			{
				return;
			}
			loading = value;
			if (((Control)this).IsHandleCreated)
			{
				if (loading)
				{
					bool ProgState = false;
					ThreadLoading = new ITask((Control)(object)this, delegate
					{
						if (ProgState)
						{
							LineAngle = LineAngle.Calculate(9f);
							LineWidth = LineWidth.Calculate(0.6f);
							if (LineWidth > 75f)
							{
								ProgState = false;
							}
						}
						else
						{
							LineAngle = LineAngle.Calculate(9.6f);
							LineWidth = LineWidth.Calculate(-0.6f);
							if (LineWidth < 6f)
							{
								ProgState = true;
							}
						}
						if (LineAngle >= 360f)
						{
							LineAngle = 0f;
						}
						((Control)this).Invalidate();
						return true;
					}, 10);
				}
				else
				{
					ThreadLoading?.Dispose();
				}
			}
			((Control)this).Invalidate();
		}
	}

	public override Rectangle ReadRectangle => ((Control)this).ClientRectangle.PaddingRect(((Control)this).Padding, (float)WaveSize * Config.Dpi);

	public override GraphicsPath RenderRegion
	{
		get
		{
			Rectangle readRectangle = ReadRectangle;
			return readRectangle.RoundPath(readRectangle.Height);
		}
	}

	private bool ExtraMouseHover
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
			if (!enabled)
			{
				return;
			}
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
			else
			{
				AnimationHoverValue = 255f;
			}
			((Control)this).Invalidate();
		}
	}

	[Browsable(false)]
	[Description("是否存在焦点")]
	[Category("行为")]
	[DefaultValue(false)]
	public bool HasFocus
	{
		get
		{
			return hasFocus;
		}
		private set
		{
			if (value && _mouseHover)
			{
				value = false;
			}
			if (hasFocus != value)
			{
				hasFocus = value;
				((Control)this).Invalidate();
			}
		}
	}

	[Description("Checked 属性值更改时发生")]
	[Category("行为")]
	public event BoolEventHandler? CheckedChanged;

	public Switch()
		: base(ControlType.Select)
	{
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Expected O, but got Unknown
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Expected O, but got Unknown
		//IL_0511: Unknown result type (might be due to invalid IL or missing references)
		//IL_0518: Expected O, but got Unknown
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_051e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0525: Unknown result type (might be due to invalid IL or missing references)
		//IL_059a: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a1: Expected O, but got Unknown
		//IL_0415: Unknown result type (might be due to invalid IL or missing references)
		//IL_041c: Expected O, but got Unknown
		//IL_0422: Unknown result type (might be due to invalid IL or missing references)
		//IL_0429: Unknown result type (might be due to invalid IL or missing references)
		init = true;
		Canvas canvas = e.Graphics.High();
		Rectangle rect = ((Control)this).ClientRectangle.PaddingRect(((Control)this).Padding);
		Rectangle readRectangle = ReadRectangle;
		bool enabled = base.Enabled;
		GraphicsPath val = readRectangle.RoundPath(readRectangle.Height);
		try
		{
			Color color = fill ?? Colour.Primary.Get("Switch");
			PaintClick(canvas, val, rect, readRectangle, color);
			if (enabled && hasFocus && WaveSize > 0)
			{
				float num = (float)WaveSize * Config.Dpi / 2f;
				float num2 = num * 2f;
				GraphicsPath val2 = new RectangleF((float)readRectangle.X - num, (float)readRectangle.Y - num, (float)readRectangle.Width + num2, (float)readRectangle.Height + num2).RoundPath(0f, TShape.Round);
				try
				{
					canvas.Draw(Colour.PrimaryBorder.Get("Switch"), num, val2);
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
			}
			SolidBrush val3 = new SolidBrush(Colour.TextQuaternary.Get("Switch"));
			try
			{
				canvas.Fill((Brush)(object)val3, val);
				if (AnimationHover)
				{
					canvas.Fill(Helper.ToColorN(AnimationHoverValue, val3.Color), val);
				}
				else if (ExtraMouseHover)
				{
					canvas.Fill((Brush)(object)val3, val);
				}
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
			int num3 = (int)((float)Gap * Config.Dpi);
			int num4 = num3 * 2;
			if (AnimationCheck)
			{
				float alpha = 255f * AnimationCheckValue;
				canvas.Fill(Helper.ToColor(alpha, color), val);
				RectangleF rect2 = new RectangleF((float)(readRectangle.X + num3) + (float)(readRectangle.Width - readRectangle.Height) * AnimationCheckValue, readRectangle.Y + num3, readRectangle.Height - num4, readRectangle.Height - num4);
				canvas.FillEllipse(enabled ? Colour.BgBase.Get("Switch") : Color.FromArgb(200, Colour.BgBase.Get("Switch")), rect2);
				if (loading)
				{
					RectangleF rect3 = new RectangleF(rect2.X + (float)num3, rect2.Y + (float)num3, rect2.Height - (float)num4, rect2.Height - (float)num4);
					float num5 = (float)readRectangle.Height * 0.1f;
					Pen val4 = new Pen(color, num5);
					try
					{
						LineCap startCap = (LineCap)2;
						val4.EndCap = (LineCap)2;
						val4.StartCap = startCap;
						canvas.DrawArc(val4, rect3, LineAngle, LineWidth * 3.6f);
					}
					finally
					{
						((IDisposable)val4)?.Dispose();
					}
				}
			}
			else if (_checked)
			{
				Color color2 = FillHover ?? Colour.PrimaryHover.Get("Switch");
				canvas.Fill(enabled ? color : Color.FromArgb(200, color), val);
				if (AnimationHover)
				{
					canvas.Fill(Helper.ToColorN(AnimationHoverValue, color2), val);
				}
				else if (ExtraMouseHover)
				{
					canvas.Fill(color2, val);
				}
				RectangleF rect4 = new RectangleF(readRectangle.X + num3 + readRectangle.Width - readRectangle.Height, readRectangle.Y + num3, readRectangle.Height - num4, readRectangle.Height - num4);
				canvas.FillEllipse(enabled ? Colour.BgBase.Get("Switch") : Color.FromArgb(200, Colour.BgBase.Get("Switch")), rect4);
				if (loading)
				{
					RectangleF rect5 = new RectangleF(rect4.X + (float)num3, rect4.Y + (float)num3, rect4.Height - (float)num4, rect4.Height - (float)num4);
					float num6 = (float)readRectangle.Height * 0.1f;
					Pen val5 = new Pen(color, num6);
					try
					{
						LineCap startCap = (LineCap)2;
						val5.EndCap = (LineCap)2;
						val5.StartCap = startCap;
						canvas.DrawArc(val5, rect5, LineAngle, LineWidth * 3.6f);
					}
					finally
					{
						((IDisposable)val5)?.Dispose();
					}
				}
			}
			else
			{
				RectangleF rect6 = new RectangleF(readRectangle.X + num3, readRectangle.Y + num3, readRectangle.Height - num4, readRectangle.Height - num4);
				canvas.FillEllipse(enabled ? Colour.BgBase.Get("Switch") : Color.FromArgb(200, Colour.BgBase.Get("Switch")), rect6);
				if (loading)
				{
					RectangleF rect7 = new RectangleF(rect6.X + (float)num3, rect6.Y + (float)num3, rect6.Height - (float)num4, rect6.Height - (float)num4);
					float num7 = (float)readRectangle.Height * 0.1f;
					Pen val6 = new Pen(color, num7);
					try
					{
						LineCap startCap = (LineCap)2;
						val6.EndCap = (LineCap)2;
						val6.StartCap = startCap;
						canvas.DrawArc(val6, rect7, LineAngle, LineWidth * 3.6f);
					}
					finally
					{
						((IDisposable)val6)?.Dispose();
					}
				}
			}
			string text = (Checked ? CheckedText : UnCheckedText);
			if (text != null)
			{
				SolidBrush val7 = new SolidBrush(fore ?? Colour.PrimaryColor.Get("Switch"));
				try
				{
					Size size = canvas.MeasureString(text, ((Control)this).Font);
					Rectangle rect8 = (Checked ? new Rectangle(readRectangle.X + (readRectangle.Width - readRectangle.Height + num4) / 2 - size.Width / 2, readRectangle.Y + readRectangle.Height / 2 - size.Height / 2, size.Width, size.Height) : new Rectangle(readRectangle.X + (readRectangle.Height - num3 + (readRectangle.Width - readRectangle.Height + num3) / 2 - size.Width / 2), readRectangle.Y + readRectangle.Height / 2 - size.Height / 2, size.Width, size.Height));
					canvas.String(text, ((Control)this).Font, (Brush)(object)val7, rect8);
				}
				finally
				{
					((IDisposable)val7)?.Dispose();
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		this.PaintBadge(canvas);
		((Control)this).OnPaint(e);
	}

	internal void PaintClick(Canvas g, GraphicsPath path, Rectangle rect, RectangleF rect_read, Color color)
	{
		_ = AnimationClick;
		float alpha = 100f * (1f - AnimationClickValue);
		float num = rect_read.Width + ((float)rect.Width - rect_read.Width) * AnimationClickValue;
		float num2 = rect_read.Height + ((float)rect.Height - rect_read.Height) * AnimationClickValue;
		GraphicsPath val = new RectangleF((float)rect.X + ((float)rect.Width - num) / 2f, (float)rect.Y + ((float)rect.Height - num2) / 2f, num, num2).RoundPath(num2);
		try
		{
			val.AddPath(path, false);
			g.Fill(Helper.ToColor(alpha, color), val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	protected override void OnClick(EventArgs e)
	{
		if (AutoCheck)
		{
			Checked = !_checked;
		}
		((Control)this).OnClick(e);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		init = false;
		((Control)this).Focus();
		((Control)this).OnMouseDown(e);
	}

	protected override void OnKeyUp(KeyEventArgs e)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		((Control)this).OnKeyUp(e);
		if ((int)e.KeyCode == 32 || (int)e.KeyCode == 13)
		{
			((Control)this).OnClick(EventArgs.Empty);
			e.Handled = true;
		}
	}

	protected override void Dispose(bool disposing)
	{
		ThreadClick?.Dispose();
		ThreadCheck?.Dispose();
		ThreadHover?.Dispose();
		ThreadLoading?.Dispose();
		base.Dispose(disposing);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		((Control)this).OnMouseUp(e);
		if (!Config.Animation || (int)e.Button != 1048576)
		{
			return;
		}
		ThreadClick?.Dispose();
		AnimationClickValue = 0f;
		AnimationClick = true;
		ThreadClick = new ITask((Control)(object)this, delegate
		{
			if ((double)AnimationClickValue > 0.6)
			{
				AnimationClickValue = AnimationClickValue.Calculate(0.04f);
			}
			else
			{
				AnimationClickValue = AnimationClickValue.Calculate(0.1f);
			}
			if (AnimationClickValue > 1f)
			{
				AnimationClickValue = 0f;
				return false;
			}
			((Control)this).Invalidate();
			return true;
		}, 50, delegate
		{
			AnimationClick = false;
			((Control)this).Invalidate();
		});
	}

	protected override void OnMouseEnter(EventArgs e)
	{
		((Control)this).OnMouseEnter(e);
		ExtraMouseHover = true;
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		((Control)this).OnMouseLeave(e);
		ExtraMouseHover = false;
	}

	protected override void OnLeave(EventArgs e)
	{
		((Control)this).OnLeave(e);
		ExtraMouseHover = false;
	}

	protected override void OnGotFocus(EventArgs e)
	{
		((Control)this).OnGotFocus(e);
		if (init)
		{
			HasFocus = true;
		}
	}

	protected override void OnLostFocus(EventArgs e)
	{
		((Control)this).OnLostFocus(e);
		HasFocus = false;
	}
}
