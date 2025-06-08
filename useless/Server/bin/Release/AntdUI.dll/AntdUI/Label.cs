using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using AntdUI.Design;

namespace AntdUI;

[Description("Label 文本")]
[ToolboxItem(true)]
[DefaultProperty("Text")]
public class Label : IControl, ShadowConfig, IEventListener
{
	private Color? fore;

	private string? colorExtend;

	private string? text;

	private StringFormat stringCNoWrap = new StringFormat
	{
		LineAlignment = (StringAlignment)1,
		Alignment = (StringAlignment)1,
		FormatFlags = (StringFormatFlags)4096
	};

	private StringFormat stringFormat = new StringFormat
	{
		LineAlignment = (StringAlignment)1,
		Alignment = (StringAlignment)0
	};

	private ContentAlignment textAlign = (ContentAlignment)16;

	private bool autoEllipsis;

	private bool textMultiLine = true;

	private float iconratio = 0.7f;

	private string? prefix;

	private string? prefixSvg;

	private string? suffix;

	private string? suffixSvg;

	private TRotate rotate;

	private int shadow;

	private float shadowOpacity = 0.3f;

	private int shadowOffsetX;

	private int shadowOffsetY;

	private bool ellipsis;

	private TooltipForm? tooltipForm;

	private TAutoSize autoSize;

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

	[Description("文字渐变色")]
	[Category("外观")]
	[DefaultValue(null)]
	public string? ColorExtend
	{
		get
		{
			return colorExtend;
		}
		set
		{
			if (!(colorExtend == value))
			{
				colorExtend = value;
				((Control)this).Invalidate();
				OnPropertyChanged("ColorExtend");
			}
		}
	}

	[Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
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

	[Description("文本超出自动处理")]
	[Category("行为")]
	[DefaultValue(false)]
	public bool AutoEllipsis
	{
		get
		{
			return autoEllipsis;
		}
		set
		{
			if (autoEllipsis != value)
			{
				autoEllipsis = value;
				stringFormat.Trimming = (StringTrimming)(value ? 3 : 0);
				((Control)this).Invalidate();
				OnPropertyChanged("AutoEllipsis");
			}
		}
	}

	[Description("是否多行")]
	[Category("行为")]
	[DefaultValue(true)]
	public bool TextMultiLine
	{
		get
		{
			return textMultiLine;
		}
		set
		{
			if (textMultiLine != value)
			{
				textMultiLine = value;
				stringFormat.FormatFlags = (StringFormatFlags)((!value) ? 4096 : 0);
				((Control)this).Invalidate();
				OnPropertyChanged("TextMultiLine");
			}
		}
	}

	[Description("图标比例")]
	[Category("外观")]
	[DefaultValue(0.7f)]
	public float IconRatio
	{
		get
		{
			return iconratio;
		}
		set
		{
			if (iconratio != value)
			{
				iconratio = value;
				IOnSizeChanged();
				((Control)this).Invalidate();
				OnPropertyChanged("IconRatio");
			}
		}
	}

	[Description("前缀")]
	[Category("外观")]
	[DefaultValue(null)]
	[Localizable(true)]
	public string? Prefix
	{
		get
		{
			return ((Control)(object)this).GetLangI(LocalizationPrefix, prefix);
		}
		set
		{
			if (!(prefix == value))
			{
				prefix = value;
				IOnSizeChanged();
				if (BeforeAutoSize())
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("Prefix");
			}
		}
	}

	[Description("前缀")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationPrefix { get; set; }

	[Description("前缀SVG")]
	[Category("外观")]
	[DefaultValue(null)]
	public string? PrefixSvg
	{
		get
		{
			return prefixSvg;
		}
		set
		{
			if (!(prefixSvg == value))
			{
				prefixSvg = value;
				IOnSizeChanged();
				if (BeforeAutoSize())
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("PrefixSvg");
			}
		}
	}

	[Description("前缀颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? PrefixColor { get; set; }

	public bool HasPrefix
	{
		get
		{
			if (prefixSvg == null)
			{
				return Prefix != null;
			}
			return true;
		}
	}

	[Description("后缀")]
	[Category("外观")]
	[DefaultValue(null)]
	[Localizable(true)]
	public string? Suffix
	{
		get
		{
			return ((Control)(object)this).GetLangI(LocalizationSuffix, suffix);
		}
		set
		{
			if (!(suffix == value))
			{
				suffix = value;
				IOnSizeChanged();
				if (BeforeAutoSize())
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("Suffix");
			}
		}
	}

	[Description("后缀")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationSuffix { get; set; }

	[Description("后缀SVG")]
	[Category("外观")]
	[DefaultValue(null)]
	public string? SuffixSvg
	{
		get
		{
			return suffixSvg;
		}
		set
		{
			if (!(suffixSvg == value))
			{
				suffixSvg = value;
				IOnSizeChanged();
				if (BeforeAutoSize())
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("SuffixSvg");
			}
		}
	}

	[Description("后缀颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? SuffixColor { get; set; }

	[Description("缀标完全展示")]
	[Category("外观")]
	[DefaultValue(true)]
	public bool Highlight { get; set; } = true;


	public bool HasSuffix
	{
		get
		{
			if (suffixSvg == null)
			{
				return Suffix != null;
			}
			return true;
		}
	}

	[Description("超出文字显示 Tooltip")]
	[Category("外观")]
	[DefaultValue(true)]
	public bool ShowTooltip { get; set; } = true;


	[Browsable(false)]
	[Description("超出文字提示配置")]
	[Category("行为")]
	[DefaultValue(null)]
	public TooltipConfig? TooltipConfig { get; set; }

	[Description("旋转")]
	[Category("外观")]
	[DefaultValue(TRotate.None)]
	public TRotate Rotate
	{
		get
		{
			return rotate;
		}
		set
		{
			if (rotate != value)
			{
				rotate = value;
				IOnSizeChanged();
				if (BeforeAutoSize())
				{
					((Control)this).Invalidate();
				}
			}
		}
	}

	[Description("阴影大小")]
	[Category("阴影")]
	[DefaultValue(0)]
	public int Shadow
	{
		get
		{
			return shadow;
		}
		set
		{
			if (shadow != value)
			{
				shadow = value;
				((Control)this).Invalidate();
				OnPropertyChanged("Shadow");
			}
		}
	}

	[Description("阴影颜色")]
	[Category("阴影")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ShadowColor { get; set; }

	[Description("阴影透明度")]
	[Category("阴影")]
	[DefaultValue(0.3f)]
	public float ShadowOpacity
	{
		get
		{
			return shadowOpacity;
		}
		set
		{
			if (shadowOpacity != value)
			{
				if (value < 0f)
				{
					value = 0f;
				}
				else if (value > 1f)
				{
					value = 1f;
				}
				shadowOpacity = value;
				((Control)this).Invalidate();
				OnPropertyChanged("ShadowOpacity");
			}
		}
	}

	[Description("阴影偏移X")]
	[Category("阴影")]
	[DefaultValue(0)]
	public int ShadowOffsetX
	{
		get
		{
			return shadowOffsetX;
		}
		set
		{
			if (shadowOffsetX != value)
			{
				shadowOffsetX = value;
				((Control)this).Invalidate();
				OnPropertyChanged("ShadowOffsetX");
			}
		}
	}

	[Description("阴影偏移Y")]
	[Category("阴影")]
	[DefaultValue(0)]
	public int ShadowOffsetY
	{
		get
		{
			return shadowOffsetY;
		}
		set
		{
			if (shadowOffsetY != value)
			{
				shadowOffsetY = value;
				((Control)this).Invalidate();
				OnPropertyChanged("ShadowOffsetY");
			}
		}
	}

	public override Rectangle ReadRectangle => ((Control)this).ClientRectangle.PaddingRect(((Control)this).Padding);

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

	private Size PSize
	{
		get
		{
			bool has_prefixText = Prefix != null;
			bool has_suffixText = Suffix != null;
			bool has_prefix = prefixSvg != null;
			bool has_suffix = suffixSvg != null;
			return Helper.GDI(delegate(Canvas g)
			{
				Size result = g.MeasureString(((Control)this).Text ?? "龍Qq", ((Control)this).Font);
				if (string.IsNullOrWhiteSpace(((Control)this).Text))
				{
					result.Width = 0;
				}
				if (has_prefixText || has_suffixText || has_prefix || has_suffix)
				{
					float num = 0f;
					if (has_prefix)
					{
						num += (float)result.Height;
					}
					else if (has_prefixText)
					{
						int width = g.MeasureString(Prefix, ((Control)this).Font).Width;
						num += (float)width;
					}
					if (has_suffix)
					{
						num += (float)result.Height;
					}
					else if (has_suffixText)
					{
						int width2 = g.MeasureString(Suffix, ((Control)this).Font).Width;
						num += (float)width2;
					}
					return new Size((int)Math.Ceiling((float)result.Width + num), result.Height);
				}
				return result;
			});
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Expected O, but got Unknown
		Canvas canvas = e.Graphics.High();
		Rectangle readRectangle = ReadRectangle;
		if (rotate == TRotate.Clockwise_90)
		{
			Matrix val = new Matrix();
			try
			{
				val.RotateAt(90f, new PointF(((Control)this).Width / 2, ((Control)this).Height / 2));
				e.Graphics.Transform = val;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		else if (rotate == TRotate.CounterClockwise_90)
		{
			Matrix val2 = new Matrix();
			try
			{
				val2.RotateAt(-90f, new PointF(((Control)this).Width / 2, ((Control)this).Height / 2));
				e.Graphics.Transform = val2;
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		Color color = Colour.DefaultColor.Get("Label");
		if (fore.HasValue)
		{
			color = fore.Value;
		}
		PaintText(canvas, ((Control)this).Text, color, readRectangle);
		if (shadow > 0)
		{
			Bitmap val3 = new Bitmap(((Control)this).Width, ((Control)this).Height);
			try
			{
				using (Canvas g = Graphics.FromImage((Image)(object)val3).HighLay())
				{
					PaintText(g, ((Control)this).Text, ShadowColor.GetValueOrDefault(color), readRectangle);
				}
				Helper.Blur(val3, shadow);
				canvas.Image(val3, new Rectangle(shadowOffsetX, shadowOffsetY, ((Image)val3).Width, ((Image)val3).Height), shadowOpacity);
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
		}
		this.PaintBadge(canvas);
		((Control)this).OnPaint(e);
	}

	private void PaintText(Canvas g, string? text, Color color, Rectangle rect_read)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Invalid comparison between Unknown and I4
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Invalid comparison between Unknown and I4
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Invalid comparison between Unknown and I4
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Invalid comparison between Unknown and I4
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Invalid comparison between Unknown and I4
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Invalid comparison between Unknown and I4
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Invalid comparison between Unknown and I4
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		Size font_size = g.MeasureString(text, ((Control)this).Font);
		bool flag = Prefix != null;
		bool flag2 = Suffix != null;
		bool flag3 = prefixSvg != null;
		bool flag4 = suffixSvg != null;
		if (flag || flag2 || flag3 || flag4)
		{
			ContentAlignment val = textAlign;
			if ((int)val <= 16)
			{
				if ((int)val != 1)
				{
					if ((int)val == 4)
					{
						goto IL_009c;
					}
					if ((int)val != 16)
					{
						goto IL_00b0;
					}
				}
				goto IL_0088;
			}
			if ((int)val != 64)
			{
				if ((int)val == 256)
				{
					goto IL_0088;
				}
				if ((int)val != 1024)
				{
					goto IL_00b0;
				}
			}
			goto IL_009c;
		}
		Rectangle rect = rect_read;
		goto IL_00c7;
		IL_00b0:
		rect = PaintTextCenter(g, color, rect_read, font_size, flag, flag2, flag3, flag4);
		goto IL_00c7;
		IL_0088:
		rect = PaintTextLeft(g, color, rect_read, font_size, flag, flag2, flag3, flag4);
		goto IL_00c7;
		IL_00c7:
		TRotate tRotate = rotate;
		if ((uint)(tRotate - 1) <= 1u)
		{
			if (autoEllipsis)
			{
				ellipsis = rect.Height < font_size.Width;
			}
			else
			{
				ellipsis = false;
			}
			int num = (rect.Width - rect.Height) / 2;
			int width = rect.Width;
			int x = rect.X;
			rect.X = rect.Y + num;
			rect.Width = rect.Height;
			rect.Height = width;
			rect.Y = x - num;
		}
		else if (autoEllipsis)
		{
			ellipsis = rect.Width < font_size.Width;
		}
		else
		{
			ellipsis = false;
		}
		Brush val2 = colorExtend.BrushEx(rect, color);
		try
		{
			g.String(text, ((Control)this).Font, val2, rect, stringFormat);
			return;
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
		IL_009c:
		rect = PaintTextRight(g, color, rect_read, font_size, flag, flag2, flag3, flag4);
		goto IL_00c7;
	}

	private Rectangle PaintTextLeft(Canvas g, Color color, Rectangle rect_read, Size font_size, bool has_prefixText, bool has_suffixText, bool has_prefix, bool has_suffix)
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Expected O, but got Unknown
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Expected O, but got Unknown
		int num = 0;
		if (has_prefixText)
		{
			string text = Prefix;
			Size size = g.MeasureString(text, ((Control)this).Font);
			int x = rect_read.X - size.Width;
			int width = size.Width;
			Rectangle rect = RecFixAuto(x, width, rect_read, font_size);
			if (Highlight)
			{
				num = size.Width;
				rect.X = 0;
			}
			SolidBrush val = new SolidBrush(PrefixColor.GetValueOrDefault(color));
			try
			{
				g.String(text, ((Control)this).Font, (Brush)(object)val, rect, stringCNoWrap);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		else if (has_prefix)
		{
			int num2 = (int)((float)font_size.Height * iconratio);
			int x2 = rect_read.X - num2;
			int w = num2;
			Rectangle rect2 = RecFixAuto(x2, w, rect_read, font_size);
			if (Highlight)
			{
				num = num2;
				rect2.X = 0;
			}
			g.GetImgExtend(prefixSvg, rect2, PrefixColor.GetValueOrDefault(color));
		}
		if (has_suffixText)
		{
			string text2 = Suffix;
			Size size2 = g.MeasureString(text2, ((Control)this).Font);
			int x3 = rect_read.X + num + font_size.Width;
			int width2 = size2.Width;
			SolidBrush val2 = new SolidBrush(SuffixColor.GetValueOrDefault(color));
			try
			{
				g.String(text2, ((Control)this).Font, (Brush)(object)val2, RecFixAuto(x3, width2, rect_read, font_size), stringCNoWrap);
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		else if (has_suffix)
		{
			int num3 = (int)((float)font_size.Height * iconratio);
			int x4 = rect_read.X + num + font_size.Width;
			int w2 = num3;
			Rectangle rect3 = RecFixAuto(x4, w2, rect_read, font_size);
			Bitmap imgExtend = SvgExtend.GetImgExtend(suffixSvg, rect3, SuffixColor.GetValueOrDefault(color));
			try
			{
				if (imgExtend != null)
				{
					g.Image(imgExtend, rect3);
				}
			}
			finally
			{
				((IDisposable)imgExtend)?.Dispose();
			}
		}
		if (num > 0)
		{
			return new Rectangle(rect_read.X + num, rect_read.Y, rect_read.Width - num, rect_read.Height);
		}
		return rect_read;
	}

	private Rectangle PaintTextRight(Canvas g, Color color, Rectangle rect_read, Size font_size, bool has_prefixText, bool has_suffixText, bool has_prefix, bool has_suffix)
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Expected O, but got Unknown
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Expected O, but got Unknown
		int num = 0;
		if (has_suffixText)
		{
			string text = Suffix;
			Size size = g.MeasureString(text, ((Control)this).Font);
			int right = rect_read.Right;
			int width = size.Width;
			Rectangle rect = RecFixAuto(right, width, rect_read, font_size);
			if (Highlight)
			{
				num = size.Width;
				rect.X = rect_read.Right - num;
			}
			SolidBrush val = new SolidBrush(SuffixColor.GetValueOrDefault(color));
			try
			{
				g.String(text, ((Control)this).Font, (Brush)(object)val, rect, stringCNoWrap);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		else if (has_suffix)
		{
			int num2 = (int)((float)font_size.Height * iconratio);
			int right2 = rect_read.Right;
			int w = num2;
			Rectangle rect2 = RecFixAuto(right2, w, rect_read, font_size);
			if (Highlight)
			{
				num = num2;
				rect2.X = rect_read.Right - num2;
			}
			g.GetImgExtend(suffixSvg, rect2, SuffixColor.GetValueOrDefault(color));
		}
		if (has_prefixText)
		{
			string text2 = Prefix;
			Size size2 = g.MeasureString(text2, ((Control)this).Font);
			int x = rect_read.Right - num - font_size.Width - size2.Width;
			int width2 = size2.Width;
			Rectangle rect3 = RecFixAuto(x, width2, rect_read, font_size);
			SolidBrush val2 = new SolidBrush(PrefixColor.GetValueOrDefault(color));
			try
			{
				g.String(text2, ((Control)this).Font, (Brush)(object)val2, rect3, stringCNoWrap);
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		else if (has_prefix)
		{
			int num3 = (int)((float)font_size.Height * iconratio);
			int x2 = rect_read.Right - num - font_size.Width - num3;
			int w2 = num3;
			Rectangle rect4 = RecFixAuto(x2, w2, rect_read, font_size);
			g.GetImgExtend(prefixSvg, rect4, PrefixColor.GetValueOrDefault(color));
		}
		if (num > 0)
		{
			return new Rectangle(rect_read.X, rect_read.Y, rect_read.Width - num, rect_read.Height);
		}
		return rect_read;
	}

	private Rectangle PaintTextCenter(Canvas g, Color color, Rectangle rect_read, Size font_size, bool has_prefixText, bool has_suffixText, bool has_prefix, bool has_suffix)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Expected O, but got Unknown
		int num = rect_read.X + (rect_read.Width - font_size.Width) / 2;
		if (has_prefixText)
		{
			string text = Prefix;
			Size size = g.MeasureString(text, ((Control)this).Font);
			Rectangle rect = RecFixAuto(num - size.Width, size.Width, rect_read, font_size);
			SolidBrush val = new SolidBrush(PrefixColor.GetValueOrDefault(color));
			try
			{
				g.String(text, ((Control)this).Font, (Brush)(object)val, rect, stringCNoWrap);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		else if (has_prefix)
		{
			int num2 = (int)((float)font_size.Height * iconratio);
			Rectangle rect2 = RecFixAuto(num - num2, num2, rect_read, font_size);
			g.GetImgExtend(prefixSvg, rect2, PrefixColor.GetValueOrDefault(color));
		}
		if (has_suffixText)
		{
			string text2 = Suffix;
			Size size2 = g.MeasureString(text2, ((Control)this).Font);
			SolidBrush val2 = new SolidBrush(SuffixColor.GetValueOrDefault(color));
			try
			{
				g.String(text2, ((Control)this).Font, (Brush)(object)val2, RecFixAuto(num + font_size.Width, size2.Width, rect_read, font_size), stringCNoWrap);
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		else if (has_suffix)
		{
			int w = (int)((float)font_size.Height * iconratio);
			Rectangle rect3 = RecFixAuto(num + font_size.Width, w, rect_read, font_size);
			g.GetImgExtend(suffixSvg, rect3, SuffixColor.GetValueOrDefault(color));
		}
		return rect_read;
	}

	private Rectangle RecFixAuto(int x, int w, Rectangle rect_read, Size font_size)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Invalid comparison between Unknown and I4
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Invalid comparison between Unknown and I4
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Invalid comparison between Unknown and I4
		ContentAlignment val = textAlign;
		if ((int)val <= 4)
		{
			if (val - 1 <= 1 || (int)val == 4)
			{
				return RecFixT(x, w, rect_read, font_size);
			}
		}
		else if ((int)val == 256 || (int)val == 512 || (int)val == 1024)
		{
			return RecFixB(x, w, rect_read, font_size);
		}
		return RecFix(x, w, rect_read);
	}

	private Rectangle RecFix(int x, int w, Rectangle rect_read)
	{
		return new Rectangle(x, rect_read.Y, w, rect_read.Height);
	}

	private Rectangle RecFixT(int x, int w, Rectangle rect_read, Size font_size)
	{
		return new Rectangle(x, rect_read.Y, w, font_size.Height);
	}

	private Rectangle RecFixB(int x, int w, Rectangle rect_read, Size font_size)
	{
		return new Rectangle(x, rect_read.Bottom - font_size.Height, w, font_size.Height);
	}

	protected override void OnMouseHover(EventArgs e)
	{
		TooltipForm? obj = tooltipForm;
		if (obj != null)
		{
			((Form)obj).Close();
		}
		tooltipForm = null;
		if (ellipsis && ShowTooltip && ((Control)this).Text != null && tooltipForm == null)
		{
			tooltipForm = new TooltipForm((Control)(object)this, ((Control)this).Text, TooltipConfig ?? new TooltipConfig
			{
				Font = ((Control)this).Font,
				ArrowAlign = TAlign.Top
			});
			((Form)tooltipForm).Show((IWin32Window)(object)this);
		}
		((Control)this).OnMouseHover(e);
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
