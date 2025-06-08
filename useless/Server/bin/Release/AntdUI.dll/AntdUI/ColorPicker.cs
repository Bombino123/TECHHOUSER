using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using AntdUI.Design;

namespace AntdUI;

[Description("ColorPicker 颜色选择器")]
[ToolboxItem(true)]
[DefaultProperty("Value")]
[DefaultEvent("ValueChanged")]
public class ColorPicker : IControl, SubLayeredForm
{
	private Color? fore;

	internal Color? back;

	internal float borderWidth = 1f;

	internal Color? borderColor;

	private int radius = 6;

	private bool round;

	private Color _value = Colour.Primary.Get("ColorPicker");

	private bool showText;

	private bool allowclear;

	private bool hasvalue;

	private TColorMode mode;

	private bool joinLeft;

	private bool joinRight;

	private bool init;

	internal StringFormat stringLeft = Helper.SF_NoWrap((StringAlignment)1, (StringAlignment)0);

	private Bitmap? bmp_alpha;

	private bool AnimationFocus;

	private int AnimationFocusValue;

	private bool hasFocus;

	internal bool _mouseDown;

	internal int AnimationHoverValue;

	internal bool AnimationHover;

	internal bool _mouseHover;

	private LayeredFormColorPicker? subForm;

	private ITask? ThreadHover;

	private ITask? ThreadFocus;

	private TAutoSize autoSize;

	[Description("原装背景颜色")]
	[Category("外观")]
	[DefaultValue(typeof(Color), "Transparent")]
	public Color OriginalBackColor
	{
		get
		{
			return ((Control)this).BackColor;
		}
		set
		{
			((Control)this).BackColor = value;
		}
	}

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
			}
		}
	}

	[Description("背景颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? BackColor
	{
		get
		{
			return back;
		}
		set
		{
			if (!(back == value))
			{
				back = value;
				((Control)this).Invalidate();
			}
		}
	}

	[Description("边框宽度")]
	[Category("边框")]
	[DefaultValue(1f)]
	public float BorderWidth
	{
		get
		{
			return borderWidth;
		}
		set
		{
			if (borderWidth != value)
			{
				borderWidth = value;
				((Control)this).Invalidate();
			}
		}
	}

	[Description("边框颜色")]
	[Category("边框")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? BorderColor
	{
		get
		{
			return borderColor;
		}
		set
		{
			if (!(borderColor == value))
			{
				borderColor = value;
				((Control)this).Invalidate();
			}
		}
	}

	[Description("悬停边框颜色")]
	[Category("边框")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? BorderHover { get; set; }

	[Description("激活边框颜色")]
	[Category("边框")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? BorderActive { get; set; }

	[Description("波浪大小")]
	[Category("外观")]
	[DefaultValue(4)]
	public int WaveSize { get; set; } = 4;


	[Description("圆角")]
	[Category("外观")]
	[DefaultValue(6)]
	public int Radius
	{
		get
		{
			return radius;
		}
		set
		{
			if (radius != value)
			{
				radius = value;
				((Control)this).Invalidate();
			}
		}
	}

	[Description("圆角样式")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool Round
	{
		get
		{
			return round;
		}
		set
		{
			if (round != value)
			{
				round = value;
				((Control)this).Invalidate();
			}
		}
	}

	[Description("颜色的值")]
	[Category("值")]
	[DefaultValue(typeof(Color), "Transparent")]
	public Color Value
	{
		get
		{
			return _value;
		}
		set
		{
			hasvalue = true;
			if (!(value == _value))
			{
				if (DisabledAlpha && value.A != byte.MaxValue)
				{
					value = Color.FromArgb(255, value);
				}
				_value = value;
				if (BeforeAutoSize())
				{
					((Control)this).Invalidate();
				}
				this.ValueChanged?.Invoke(this, new ColorEventArgs(value));
				OnPropertyChanged("Value");
			}
		}
	}

	[Description("显示Hex文字")]
	[Category("值")]
	[DefaultValue(false)]
	public bool ShowText
	{
		get
		{
			return showText;
		}
		set
		{
			if (showText != value)
			{
				showText = value;
				if (BeforeAutoSize())
				{
					((Control)this).Invalidate();
				}
			}
		}
	}

	[Description("禁用透明度")]
	[Category("行为")]
	[DefaultValue(false)]
	public bool DisabledAlpha { get; set; }

	[Description("支持清除")]
	[Category("行为")]
	[DefaultValue(false)]
	public bool AllowClear
	{
		get
		{
			return allowclear;
		}
		set
		{
			if (allowclear != value)
			{
				allowclear = value;
				((Control)this).Invalidate();
			}
		}
	}

	public bool HasValue
	{
		get
		{
			if (allowclear)
			{
				return hasvalue;
			}
			return true;
		}
	}

	public Color? ValueClear
	{
		get
		{
			if (allowclear && !hasvalue)
			{
				return null;
			}
			return _value;
		}
	}

	[Description("颜色模式")]
	[Category("行为")]
	[DefaultValue(TColorMode.Hex)]
	public TColorMode Mode
	{
		get
		{
			return mode;
		}
		set
		{
			if (mode != value)
			{
				mode = value;
				if (BeforeAutoSize())
				{
					((Control)this).Invalidate();
				}
			}
		}
	}

	[Description("触发下拉的行为")]
	[Category("行为")]
	[DefaultValue(Trigger.Click)]
	public Trigger Trigger { get; set; }

	[Description("菜单弹出位置")]
	[Category("行为")]
	[DefaultValue(TAlignFrom.BL)]
	public TAlignFrom Placement { get; set; } = TAlignFrom.BL;


	[Description("下拉箭头是否显示")]
	[Category("外观")]
	[DefaultValue(true)]
	public bool DropDownArrow { get; set; } = true;


	[Description("连接左边")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool JoinLeft
	{
		get
		{
			return joinLeft;
		}
		set
		{
			if (joinLeft != value)
			{
				joinLeft = value;
				if (BeforeAutoSize())
				{
					((Control)this).Invalidate();
				}
			}
		}
	}

	[Description("连接右边")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool JoinRight
	{
		get
		{
			return joinRight;
		}
		set
		{
			if (joinRight != value)
			{
				joinRight = value;
				if (BeforeAutoSize())
				{
					((Control)this).Invalidate();
				}
			}
		}
	}

	public override Rectangle ReadRectangle => ((Control)this).ClientRectangle.PaddingRect(((Control)this).Padding).ReadRect(((float)WaveSize + borderWidth / 2f) * Config.Dpi, JoinLeft, JoinRight);

	public override GraphicsPath RenderRegion
	{
		get
		{
			Rectangle readRectangle = ReadRectangle;
			float num = (round ? ((float)readRectangle.Height) : ((float)radius * Config.Dpi));
			return Path(readRectangle, num);
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
			if (value && (_mouseDown || _mouseHover))
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

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	internal bool ExtraMouseDown
	{
		get
		{
			return _mouseDown;
		}
		set
		{
			if (_mouseDown == value)
			{
				return;
			}
			_mouseDown = value;
			if (Config.Animation && WaveSize > 0)
			{
				ThreadFocus?.Dispose();
				AnimationFocus = true;
				if (value)
				{
					ThreadFocus = new ITask((Control)(object)this, delegate
					{
						AnimationFocusValue += 4;
						if (AnimationFocusValue > 30)
						{
							return false;
						}
						((Control)this).Invalidate();
						return true;
					}, 20, delegate
					{
						AnimationFocus = false;
						((Control)this).Invalidate();
					});
					return;
				}
				ThreadFocus = new ITask((Control)(object)this, delegate
				{
					AnimationFocusValue -= 4;
					if (AnimationFocusValue < 1)
					{
						return false;
					}
					((Control)this).Invalidate();
					return true;
				}, 20, delegate
				{
					AnimationFocus = false;
					((Control)this).Invalidate();
				});
			}
			else
			{
				((Control)this).Invalidate();
			}
		}
	}

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
			if (!base.Enabled)
			{
				return;
			}
			if (Config.Animation && !ExtraMouseDown)
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
		Size size;
		if (this.ValueFormatChanged == null)
		{
			if (HasValue)
			{
				TColorMode tColorMode = mode;
				size = ((tColorMode == TColorMode.Hex || tColorMode != TColorMode.Rgb) ? g.MeasureString((_value.A == byte.MaxValue) ? "#DDDCCC" : "#DDDDCCCC", ((Control)this).Font) : g.MeasureString((_value.A == byte.MaxValue) ? "rgb(255,255,255)" : "rgba(255,255,255,0.99)", ((Control)this).Font));
			}
			else
			{
				size = g.MeasureString("Transparent", ((Control)this).Font);
			}
		}
		else
		{
			size = g.MeasureString(this.ValueFormatChanged(this, new ColorEventArgs(_value)), ((Control)this).Font);
		}
		int num = (int)((float)(20 + WaveSize) * Config.Dpi);
		if (showText)
		{
			int num2 = size.Height + num;
			return new Size(num2 + size.Width, num2);
		}
		int num3 = size.Height + num;
		return new Size(num3, num3);
	});

	[Description("Value 属性值更改时发生")]
	[Category("行为")]
	public event ColorEventHandler? ValueChanged;

	[Description("Value格式化时发生")]
	[Category("行为")]
	public event ColorFormatEventHandler? ValueFormatChanged;

	public ColorPicker()
		: base(ControlType.Select)
	{
		((Control)this).BackColor = Color.Transparent;
	}

	public void ClearValue()
	{
		ClearValue(Colour.Primary.Get("ColorPicker"));
	}

	public void ClearValue(Color def)
	{
		if (allowclear)
		{
			if (hasvalue)
			{
				hasvalue = false;
				_value = def;
				((Control)this).Invalidate();
				this.ValueChanged?.Invoke(this, new ColorEventArgs(_value));
			}
			else
			{
				_value = def;
			}
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fb: Expected O, but got Unknown
		init = true;
		Rectangle rect = ((Control)this).ClientRectangle.PaddingRect(((Control)this).Padding);
		Canvas canvas = e.Graphics.High();
		Rectangle readRectangle = ReadRectangle;
		float num = (round ? ((float)readRectangle.Height) : ((float)radius * Config.Dpi));
		GraphicsPath val = Path(readRectangle, num);
		try
		{
			Color color = fore ?? Colour.Text.Get("ColorPicker");
			Color color2 = back ?? Colour.BgContainer.Get("ColorPicker");
			Color color3 = borderColor ?? Colour.BorderColor.Get("ColorPicker");
			Color color4 = BorderHover ?? Colour.PrimaryHover.Get("ColorPicker");
			Color color5 = BorderActive ?? Colour.Primary.Get("ColorPicker");
			PaintClick(canvas, val, rect, color5, num);
			int num2 = (int)((float)readRectangle.Height * 0.75f);
			if (base.Enabled)
			{
				if (hasFocus && WaveSize > 0)
				{
					float num3 = (float)WaveSize * Config.Dpi / 2f;
					float num4 = num3 * 2f;
					GraphicsPath val2 = new RectangleF((float)readRectangle.X - num3, (float)readRectangle.Y - num3, (float)readRectangle.Width + num4, (float)readRectangle.Height + num4).RoundPath(num + num3);
					try
					{
						canvas.Draw(Colour.PrimaryBorder.Get("ColorPicker"), num3, val2);
					}
					finally
					{
						((IDisposable)val2)?.Dispose();
					}
				}
				canvas.Fill(color2, val);
				if (borderWidth > 0f)
				{
					float width = borderWidth * Config.Dpi;
					if (AnimationHover)
					{
						canvas.Draw(color3.BlendColors(AnimationHoverValue, color4), width, val);
					}
					else if (ExtraMouseDown)
					{
						canvas.Draw(color5, width, val);
					}
					else if (ExtraMouseHover)
					{
						canvas.Draw(color4, width, val);
					}
					else
					{
						canvas.Draw(color3, width, val);
					}
				}
			}
			else
			{
				color = Colour.TextQuaternary.Get("ColorPicker");
				canvas.Fill(Colour.FillTertiary.Get("ColorPicker"), val);
				if (borderWidth > 0f)
				{
					canvas.Draw(color3, borderWidth * Config.Dpi, val);
				}
			}
			float r = num * 0.75f;
			if (showText)
			{
				int num5 = (readRectangle.Height - num2) / 2;
				Rectangle rect_color = new Rectangle(readRectangle.X + num5, readRectangle.Y + num5, num2, num2);
				PaintValue(canvas, r, rect_color);
				SolidBrush val3 = new SolidBrush(color);
				try
				{
					int num6 = num5 * 2 + num2;
					if (this.ValueFormatChanged == null)
					{
						if (HasValue)
						{
							switch (mode)
							{
							case TColorMode.Hex:
								canvas.String("#" + _value.ToHex(), ((Control)this).Font, (Brush)(object)val3, new Rectangle(readRectangle.X + num6, readRectangle.Y, readRectangle.Width - num6, readRectangle.Height), stringLeft);
								break;
							case TColorMode.Rgb:
								if (_value.A == byte.MaxValue)
								{
									canvas.String($"rgb({_value.R},{_value.G},{_value.B})", ((Control)this).Font, (Brush)(object)val3, new Rectangle(readRectangle.X + num6, readRectangle.Y, readRectangle.Width - num6, readRectangle.Height), stringLeft);
									break;
								}
								canvas.String($"rgba({_value.R},{_value.G},{_value.B},{Math.Round((double)(int)_value.A / 255.0, 2)})", ((Control)this).Font, (Brush)(object)val3, new Rectangle(readRectangle.X + num6, readRectangle.Y, readRectangle.Width - num6, readRectangle.Height), stringLeft);
								break;
							}
						}
						else
						{
							canvas.String("Transparent", ((Control)this).Font, (Brush)(object)val3, new Rectangle(readRectangle.X + num6, readRectangle.Y, readRectangle.Width - num6, readRectangle.Height), stringLeft);
						}
					}
					else
					{
						canvas.String(this.ValueFormatChanged(this, new ColorEventArgs(_value)), ((Control)this).Font, (Brush)(object)val3, new Rectangle(readRectangle.X + num6, readRectangle.Y, readRectangle.Width - num6, readRectangle.Height), stringLeft);
					}
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
			}
			else
			{
				int num7 = (int)((float)readRectangle.Width * 0.75f);
				Rectangle rect_color2 = new Rectangle(readRectangle.X + (readRectangle.Width - num7) / 2, readRectangle.Y + (readRectangle.Height - num2) / 2, num7, num2);
				PaintValue(canvas, r, rect_color2);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		this.PaintBadge(canvas);
		((Control)this).OnPaint(e);
	}

	private void PaintValue(Canvas g, float r, Rectangle rect_color)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		GraphicsPath val = rect_color.RoundPath(r);
		try
		{
			_ = HasValue;
			if (allowclear && !hasvalue)
			{
				g.SetClip(val);
				Pen val2 = new Pen(Color.FromArgb(245, 34, 45), (float)rect_color.Height * 0.12f);
				try
				{
					g.DrawLine(val2, new Point(rect_color.X, rect_color.Bottom), new Point(rect_color.Right, rect_color.Y));
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
				g.ResetClip();
				g.Draw(Colour.Split.Get("ColorPicker"), Config.Dpi, val);
			}
			else
			{
				PaintAlpha(g, r, rect_color);
				g.Fill(_value, val);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void PaintAlpha(Canvas g, float radius, Rectangle rect)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		if (bmp_alpha == null || ((Image)bmp_alpha).Width != rect.Width || ((Image)bmp_alpha).Height != rect.Height)
		{
			Bitmap? obj = bmp_alpha;
			if (obj != null)
			{
				((Image)obj).Dispose();
			}
			bmp_alpha = new Bitmap(rect.Width, rect.Height);
			Bitmap val = new Bitmap(rect.Width, rect.Height);
			try
			{
				using (Canvas g2 = Graphics.FromImage((Image)(object)val).High())
				{
					PaintAlpha(g2, rect);
				}
				using Canvas canvas = Graphics.FromImage((Image)(object)bmp_alpha).High();
				canvas.Image(new Rectangle(0, 0, rect.Width, rect.Height), (Image)(object)val, TFit.Fill, radius, round: false);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		g.Image(bmp_alpha, rect);
	}

	private void PaintAlpha(Canvas g, Rectangle rect)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		int num = 0;
		int num2 = rect.Height / 4;
		bool flag = false;
		SolidBrush val = new SolidBrush(Colour.FillSecondary.Get("ColorPicker"));
		try
		{
			while (num < rect.Height)
			{
				int num3 = 0;
				bool flag2 = flag;
				while (num3 < rect.Width)
				{
					if (flag2)
					{
						g.Fill((Brush)(object)val, new Rectangle(num3, num, num2, num2));
					}
					num3 += num2;
					flag2 = !flag2;
				}
				num += num2;
				flag = !flag;
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	internal GraphicsPath Path(Rectangle rect_read, float _radius)
	{
		if (JoinLeft && JoinRight)
		{
			return rect_read.RoundPath(0f);
		}
		if (JoinRight)
		{
			return rect_read.RoundPath(_radius, TL: true, TR: false, BR: false, BL: true);
		}
		if (JoinLeft)
		{
			return rect_read.RoundPath(_radius, TL: false, TR: true, BR: true, BL: false);
		}
		return rect_read.RoundPath(_radius);
	}

	internal void PaintClick(Canvas g, GraphicsPath path, Rectangle rect, Color color, float radius)
	{
		if (AnimationFocus)
		{
			if (AnimationFocusValue > 0)
			{
				GraphicsPath val = rect.RoundPath(radius, round);
				try
				{
					val.AddPath(path, false);
					g.Fill(Helper.ToColor(AnimationFocusValue, color), val);
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
		}
		else if (ExtraMouseDown && WaveSize > 0)
		{
			GraphicsPath val2 = rect.RoundPath(radius, round);
			try
			{
				val2.AddPath(path, false);
				g.Fill(Color.FromArgb(30, color), val2);
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
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

	protected override void OnMouseEnter(EventArgs e)
	{
		SetCursor(val: true);
		((Control)this).OnMouseEnter(e);
		ExtraMouseHover = true;
		if (Trigger == Trigger.Hover && subForm == null)
		{
			ClickDown();
		}
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		SetCursor(val: false);
		((Control)this).OnMouseLeave(e);
		ExtraMouseHover = false;
	}

	protected override void OnLeave(EventArgs e)
	{
		SetCursor(val: false);
		((Control)this).OnLeave(e);
		ExtraMouseHover = false;
	}

	public ILayeredForm? SubForm()
	{
		return subForm;
	}

	protected override void OnMouseClick(MouseEventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		if ((int)e.Button == 1048576 && Trigger == Trigger.Click)
		{
			init = false;
			((Control)this).ImeMode = (ImeMode)3;
			((Control)this).Focus();
			ClickDown();
		}
		((Control)this).OnMouseClick(e);
	}

	private void ClickDown()
	{
		ExtraMouseDown = true;
		if (subForm == null)
		{
			subForm = new LayeredFormColorPicker(this, ReadRectangle, delegate(Color color)
			{
				Value = color;
			});
			((Component)(object)subForm).Disposed += delegate
			{
				ExtraMouseDown = false;
				subForm = null;
			};
			((Form)subForm).Show((IWin32Window)(object)this);
		}
	}

	protected override void Dispose(bool disposing)
	{
		ThreadFocus?.Dispose();
		ThreadHover?.Dispose();
		base.Dispose(disposing);
	}

	protected override void OnFontChanged(EventArgs e)
	{
		BeforeAutoSize();
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

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		subForm?.IProcessCmdKey(ref msg, keyData);
		return ((Control)this).ProcessCmdKey(ref msg, keyData);
	}

	protected override void OnKeyPress(KeyPressEventArgs e)
	{
		if (subForm != null)
		{
			subForm.IKeyPress(e);
		}
		((Control)this).OnKeyPress(e);
	}
}
