using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using AntdUI.Design;

namespace AntdUI;

[Description("Checkbox 多选框")]
[ToolboxItem(true)]
[DefaultProperty("Checked")]
[DefaultEvent("CheckedChanged")]
public class Checkbox : IControl, IEventListener
{
	private Color? fore;

	private Color? fill;

	private string? text;

	private StringFormat stringFormat = Helper.SF_ALL((StringAlignment)1, (StringAlignment)0);

	private ContentAlignment textAlign = (ContentAlignment)16;

	private bool AnimationCheck;

	private float AnimationCheckValue;

	private bool _checked;

	private RightToLeft rightToLeft;

	private bool init;

	private int AnimationHoverValue;

	private bool AnimationHover;

	private bool _mouseHover;

	private ITask? ThreadHover;

	private ITask? ThreadCheck;

	private TAutoSize autoSize;

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

	[Description("填充颜色")]
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

	[Description("文本")]
	[Category("外观")]
	[DefaultValue(null)]
	public override string? Text
	{
		get
		{
			return ((Control)(object)this).GetLangI(LocalizationText, text);
		}
		set
		{
			if (!(text == value))
			{
				text = value;
				if (BeforeAutoSize())
				{
					((Control)this).Invalidate();
				}
				((Control)this).OnTextChanged(EventArgs.Empty);
				OnPropertyChanged("Text");
			}
		}
	}

	[Description("文本")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationText { get; set; }

	[Description("文本位置")]
	[Category("外观")]
	[DefaultValue(/*Could not decode attribute arguments.*/)]
	public ContentAlignment TextAlign
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return textAlign;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			if (textAlign != value)
			{
				textAlign = value;
				textAlign.SetAlignment(ref stringFormat);
				((Control)this).Invalidate();
				OnPropertyChanged("TextAlign");
			}
		}
	}

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
						AnimationCheckValue = AnimationCheckValue.Calculate(0.2f);
						if (AnimationCheckValue > 1f)
						{
							AnimationCheckValue = 1f;
							return false;
						}
						((Control)this).Invalidate();
						return true;
					}, 20, delegate
					{
						AnimationCheck = false;
						((Control)this).Invalidate();
					});
				}
				else
				{
					ThreadCheck = new ITask((Control)(object)this, delegate
					{
						AnimationCheckValue = AnimationCheckValue.Calculate(-0.2f);
						if (AnimationCheckValue <= 0f)
						{
							AnimationCheckValue = 0f;
							return false;
						}
						((Control)this).Invalidate();
						return true;
					}, 20, delegate
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


	[Description("反向")]
	[Category("外观")]
	[DefaultValue(/*Could not decode attribute arguments.*/)]
	public override RightToLeft RightToLeft
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return rightToLeft;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Invalid comparison between Unknown and I4
			if (rightToLeft != value)
			{
				rightToLeft = value;
				stringFormat.Alignment = (StringAlignment)(((int)((Control)this).RightToLeft == 1) ? 2 : 0);
				((Control)this).Invalidate();
				OnPropertyChanged("RightToLeft");
			}
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
						AnimationHoverValue += 20;
						if (AnimationHoverValue > 255)
						{
							AnimationHoverValue = 255;
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
						AnimationHoverValue -= 20;
						if (AnimationHoverValue < 1)
						{
							AnimationHoverValue = 0;
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
				AnimationHoverValue = 255;
			}
			((Control)this).Invalidate();
		}
	}

	[Browsable(true)]
	[Description("自动大小")]
	[Category("外观")]
	[DefaultValue(false)]
	public override bool AutoSize
	{
		get
		{
			return ((Control)this).AutoSize;
		}
		set
		{
			if (((Control)this).AutoSize == value)
			{
				return;
			}
			((Control)this).AutoSize = value;
			if (value)
			{
				if (autoSize == TAutoSize.None)
				{
					autoSize = TAutoSize.Auto;
				}
			}
			else
			{
				autoSize = TAutoSize.None;
			}
			BeforeAutoSize();
		}
	}

	[Description("自动大小模式")]
	[Category("外观")]
	[DefaultValue(TAutoSize.None)]
	public TAutoSize AutoSizeMode
	{
		get
		{
			return autoSize;
		}
		set
		{
			if (autoSize != value)
			{
				autoSize = value;
				((Control)this).AutoSize = autoSize != TAutoSize.None;
				BeforeAutoSize();
			}
		}
	}

	private Size PSize => Helper.GDI(delegate(Canvas g)
	{
		int num = (int)(20f * Config.Dpi);
		if (string.IsNullOrWhiteSpace(((Control)this).Text))
		{
			Size size = g.MeasureString("龍Qq", ((Control)this).Font);
			return new Size(size.Height + num, size.Height + num);
		}
		Size size2 = g.MeasureString(((Control)this).Text, ((Control)this).Font);
		return new Size(size2.Width + size2.Height + num, size2.Height + num);
	});

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

	public Checkbox()
		: base(ControlType.Select)
	{
	}//IL_0010: Unknown result type (might be due to invalid IL or missing references)


	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Invalid comparison between Unknown and I4
		init = true;
		Rectangle rect = ((Control)this).ClientRectangle.DeflateRect(((Control)this).Padding);
		Canvas canvas = e.Graphics.High();
		bool enabled = base.Enabled;
		if (string.IsNullOrWhiteSpace(((Control)this).Text))
		{
			Size size = canvas.MeasureString("龍Qq", ((Control)this).Font);
			PaintChecked(icon_rect: new Rectangle(rect.X + (rect.Width - size.Height) / 2, rect.Y + (rect.Height - size.Height) / 2, size.Height, size.Height), g: canvas, rect: rect, enabled: enabled, right: false);
		}
		else
		{
			Size size2 = canvas.MeasureString(((Control)this).Text, ((Control)this).Font);
			rect.IconRectL(size2.Height, out var icon_rect2, out var text_rect);
			bool flag = (int)rightToLeft == 1;
			PaintChecked(canvas, rect, enabled, icon_rect2, flag);
			if (flag)
			{
				text_rect.X = rect.Width - text_rect.X - text_rect.Width;
			}
			SolidBrush val = fore.Brush(Colour.Text.Get("Checkbox"), Colour.TextQuaternary.Get("Checkbox"), enabled);
			try
			{
				canvas.String(((Control)this).Text, ((Control)this).Font, (Brush)(object)val, text_rect, stringFormat);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		this.PaintBadge(canvas);
		((Control)this).OnPaint(e);
	}

	private void PaintChecked(Canvas g, Rectangle rect, bool enabled, Rectangle icon_rect, bool right)
	{
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Expected O, but got Unknown
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Expected O, but got Unknown
		float num = icon_rect.Height;
		float num2 = num * 0.2f;
		if (right)
		{
			icon_rect.X = rect.Width - icon_rect.X - icon_rect.Width;
		}
		GraphicsPath val = icon_rect.RoundPath(num2);
		try
		{
			float num3 = 2f * Config.Dpi;
			if (enabled)
			{
				if (hasFocus && (float)(rect.Height - icon_rect.Height) > num3)
				{
					float num4 = num3;
					float num5 = num4 * 2f;
					GraphicsPath val2 = new RectangleF((float)icon_rect.X - num4, (float)icon_rect.Y - num4, (float)icon_rect.Width + num5, (float)icon_rect.Height + num5).RoundPath(num2 + num4);
					try
					{
						g.Draw(Colour.PrimaryBorder.Get("Checkbox"), num4, val2);
					}
					finally
					{
						((IDisposable)val2)?.Dispose();
					}
				}
				Color color = fill ?? Colour.Primary.Get("Checkbox");
				if (AnimationCheck)
				{
					float alpha = 255f * AnimationCheckValue;
					g.Fill(Helper.ToColor(alpha, color), val);
					Pen val3 = new Pen(Helper.ToColor(alpha, Colour.BgBase.Get("Checkbox")), 2.6f * Config.Dpi);
					try
					{
						g.DrawLines(val3, icon_rect.CheckArrow());
					}
					finally
					{
						((IDisposable)val3)?.Dispose();
					}
					if (_checked)
					{
						float num6 = (float)icon_rect.Height + (float)(rect.Height - icon_rect.Height) * AnimationCheckValue;
						SolidBrush val4 = new SolidBrush(Helper.ToColor(100f * (1f - AnimationCheckValue), color));
						try
						{
							g.FillEllipse((Brush)(object)val4, new RectangleF((float)icon_rect.X + ((float)icon_rect.Width - num6) / 2f, (float)icon_rect.Y + ((float)icon_rect.Height - num6) / 2f, num6, num6));
						}
						finally
						{
							((IDisposable)val4)?.Dispose();
						}
					}
					g.Draw(color, num3, val);
				}
				else if (_checked)
				{
					g.Fill(color, val);
					g.DrawLines(Colour.BgBase.Get("Checkbox"), 2.6f * Config.Dpi, icon_rect.CheckArrow());
				}
				else if (AnimationHover)
				{
					g.Draw(Colour.BorderColor.Get("Checkbox").BlendColors(AnimationHoverValue, color), num3, val);
				}
				else if (ExtraMouseHover)
				{
					g.Draw(color, num3, val);
				}
				else
				{
					g.Draw(Colour.BorderColor.Get("Checkbox"), num3, val);
				}
			}
			else
			{
				g.Fill(Colour.FillQuaternary.Get("Checkbox"), val);
				if (_checked)
				{
					g.DrawLines(Colour.TextQuaternary.Get("Checkbox"), 2.6f * Config.Dpi, icon_rect.CheckArrow());
				}
				g.Draw(Colour.BorderColorDisable.Get("Checkbox"), num3, val);
			}
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
		ThreadCheck?.Dispose();
		ThreadHover?.Dispose();
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
		ExtraMouseHover = false;
	}

	protected override void OnLeave(EventArgs e)
	{
		((Control)this).OnLeave(e);
		ExtraMouseHover = false;
	}

	protected override void OnFontChanged(EventArgs e)
	{
		if (BeforeAutoSize())
		{
			((Control)this).Invalidate();
		}
		((Control)this).OnFontChanged(e);
	}

	public override Size GetPreferredSize(Size proposedSize)
	{
		if (autoSize == TAutoSize.None)
		{
			return ((Control)this).GetPreferredSize(proposedSize);
		}
		if (autoSize == TAutoSize.Width)
		{
			return new Size(PSize.Width, ((Control)this).GetPreferredSize(proposedSize).Height);
		}
		if (autoSize == TAutoSize.Height)
		{
			return new Size(((Control)this).GetPreferredSize(proposedSize).Width, PSize.Height);
		}
		return PSize;
	}

	protected override void OnResize(EventArgs e)
	{
		BeforeAutoSize();
		((Control)this).OnResize(e);
	}

	private bool BeforeAutoSize()
	{
		if (autoSize == TAutoSize.None)
		{
			return true;
		}
		if (((Control)this).InvokeRequired)
		{
			bool flag = false;
			((Control)this).Invoke((Delegate)(Action)delegate
			{
				flag = BeforeAutoSize();
			});
			return flag;
		}
		Size pSize = PSize;
		switch (autoSize)
		{
		case TAutoSize.Width:
			if (((Control)this).Width == pSize.Width)
			{
				return true;
			}
			((Control)this).Width = pSize.Width;
			break;
		case TAutoSize.Height:
			if (((Control)this).Height == pSize.Height)
			{
				return true;
			}
			((Control)this).Height = pSize.Height;
			break;
		default:
			if (((Control)this).Width == pSize.Width && ((Control)this).Height == pSize.Height)
			{
				return true;
			}
			((Control)this).Size = pSize;
			break;
		}
		return false;
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

	protected override void OnHandleCreated(EventArgs e)
	{
		((Control)this).OnHandleCreated(e);
		((Control)(object)this).AddListener();
	}

	public void HandleEvent(EventType id, object? tag)
	{
		if (id == EventType.LANG)
		{
			BeforeAutoSize();
		}
	}
}
