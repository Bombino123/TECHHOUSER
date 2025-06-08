using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using AntdUI.Design;

namespace AntdUI;

[Description("Tag 标签")]
[ToolboxItem(true)]
[DefaultProperty("Text")]
public class Tag : IControl, IEventListener
{
	private Color? fore;

	private Color? back;

	private Image? backImage;

	private TFit backFit;

	internal float borderWidth = 1f;

	private int radius = 6;

	internal TTypeMini type;

	private bool closeIcon;

	private string? text;

	private StringFormat stringFormat = Helper.SF_NoWrap((StringAlignment)1, (StringAlignment)1);

	private ContentAlignment textAlign = (ContentAlignment)32;

	private bool autoEllipsis;

	private bool textMultiLine;

	private Image? image;

	private string? imageSvg;

	private ITaskOpacity hover_close;

	private RectangleF rect_close;

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
				OnPropertyChanged("ForeColor");
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
				OnPropertyChanged("BackColor");
			}
		}
	}

	[Description("背景图片")]
	[Category("外观")]
	[DefaultValue(null)]
	public Image? BackgroundImage
	{
		get
		{
			return backImage;
		}
		set
		{
			if (backImage != value)
			{
				backImage = value;
				((Control)this).Invalidate();
				OnPropertyChanged("BackgroundImage");
			}
		}
	}

	[Description("背景图片布局")]
	[Category("外观")]
	[DefaultValue(TFit.Fill)]
	public TFit BackgroundImageLayout
	{
		get
		{
			return backFit;
		}
		set
		{
			if (backFit != value)
			{
				backFit = value;
				((Control)this).Invalidate();
				OnPropertyChanged("BackgroundImageLayout");
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
				OnPropertyChanged("BorderWidth");
			}
		}
	}

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
				if (BeforeAutoSize())
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("Radius");
			}
		}
	}

	[Description("类型")]
	[Category("外观")]
	[DefaultValue(TTypeMini.Default)]
	public TTypeMini Type
	{
		get
		{
			return type;
		}
		set
		{
			if (type != value)
			{
				type = value;
				((Control)this).Invalidate();
				OnPropertyChanged("Type");
			}
		}
	}

	[Description("是否显示关闭图标")]
	[Category("行为")]
	[DefaultValue(false)]
	public bool CloseIcon
	{
		get
		{
			return closeIcon;
		}
		set
		{
			if (closeIcon != value)
			{
				closeIcon = value;
				if (BeforeAutoSize())
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("CloseIcon");
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
				OnPropertyChanged("AutoEllipsis");
			}
		}
	}

	[Description("是否多行")]
	[Category("行为")]
	[DefaultValue(false)]
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

	[Description("图像")]
	[Category("外观")]
	[DefaultValue(null)]
	public Image? Image
	{
		get
		{
			return image;
		}
		set
		{
			if (image != value)
			{
				image = value;
				if (BeforeAutoSize())
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("Image");
			}
		}
	}

	[Description("图像SVG")]
	[Category("外观")]
	[DefaultValue(null)]
	public string? ImageSvg
	{
		get
		{
			return imageSvg;
		}
		set
		{
			if (!(imageSvg == value))
			{
				imageSvg = value;
				if (BeforeAutoSize())
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("ImageSvg");
			}
		}
	}

	public bool HasImage
	{
		get
		{
			if (imageSvg == null)
			{
				return image != null;
			}
			return true;
		}
	}

	[Description("图像大小")]
	[Category("外观")]
	[DefaultValue(typeof(Size), "0, 0")]
	public Size ImageSize { get; set; } = new Size(0, 0);


	public override Rectangle ReadRectangle => ((Control)this).ClientRectangle.PaddingRect(((Control)this).Padding, borderWidth / 2f * Config.Dpi);

	public override GraphicsPath RenderRegion
	{
		get
		{
			Rectangle readRectangle = ReadRectangle;
			float num = (float)radius * Config.Dpi;
			return readRectangle.RoundPath(num);
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
		Size size = g.MeasureString(((Control)this).Text ?? "龍Qq", ((Control)this).Font);
		int num = 0;
		if (HasImage)
		{
			num++;
		}
		if (closeIcon)
		{
			num++;
		}
		return new Size(size.Width + (int)(14f * Config.Dpi) + size.Height * num, size.Height + (int)(8f * Config.Dpi));
	});

	[Description("Close时发生")]
	[Category("行为")]
	public event RBoolEventHandler? CloseChanged;

	protected override void OnPaint(PaintEventArgs e)
	{
		Canvas canvas = e.Graphics.High();
		Rectangle readRectangle = ReadRectangle;
		if (backImage != null)
		{
			canvas.Image(readRectangle, backImage, backFit, radius, round: false);
		}
		float num = (float)radius * Config.Dpi;
		Color color;
		Color color2;
		Color color3;
		switch (type)
		{
		case TTypeMini.Default:
			color = Colour.TagDefaultBg.Get("Tag");
			color2 = Colour.TagDefaultColor.Get("Tag");
			color3 = Colour.DefaultBorder.Get("Tag");
			break;
		case TTypeMini.Error:
			color = Colour.ErrorBg.Get("Tag");
			color2 = Colour.Error.Get("Tag");
			color3 = Colour.ErrorBorder.Get("Tag");
			break;
		case TTypeMini.Success:
			color = Colour.SuccessBg.Get("Tag");
			color2 = Colour.Success.Get("Tag");
			color3 = Colour.SuccessBorder.Get("Tag");
			break;
		case TTypeMini.Info:
			color = Colour.InfoBg.Get("Tag");
			color2 = Colour.Info.Get("Tag");
			color3 = Colour.InfoBorder.Get("Tag");
			break;
		case TTypeMini.Warn:
			color = Colour.WarningBg.Get("Tag");
			color2 = Colour.Warning.Get("Tag");
			color3 = Colour.WarningBorder.Get("Tag");
			break;
		default:
			color = Colour.PrimaryBg.Get("Tag");
			color2 = Colour.Primary.Get("Tag");
			color3 = Colour.Primary.Get("Tag");
			break;
		}
		if (fore.HasValue)
		{
			color2 = fore.Value;
		}
		if (back.HasValue)
		{
			color = back.Value;
		}
		if (backImage != null)
		{
			canvas.Image(readRectangle, backImage, backFit, num, round: false);
		}
		GraphicsPath val = readRectangle.RoundPath(num);
		try
		{
			canvas.Fill(color, val);
			if (borderWidth > 0f)
			{
				canvas.Draw(color3, borderWidth * Config.Dpi, val);
			}
			PaintText(canvas, ((Control)this).Text, color2, readRectangle);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		this.PaintBadge(canvas);
		((Control)this).OnPaint(e);
	}

	internal void PaintText(Canvas g, string? text, Color color, Rectangle rect_read)
	{
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Invalid comparison between Unknown and I4
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Expected O, but got Unknown
		Size font_size = g.MeasureString(text ?? "龍Qq", ((Control)this).Font);
		if (text == null)
		{
			if (PaintImageNoText(g, color, font_size, rect_read) && closeIcon)
			{
				int num = (int)((float)rect_read.Height * 0.4f);
				Rectangle rectangle = new Rectangle(rect_read.X + (rect_read.Width - num) / 2, rect_read.Y + (rect_read.Height - num) / 2, num, num);
				rect_close = rectangle;
				if (hover_close.Animation)
				{
					g.PaintIconClose(rectangle, Helper.ToColor(hover_close.Value + Colour.TextQuaternary.Get("Tag").A, Colour.Text.Get("Tag")));
				}
				else if (hover_close.Switch)
				{
					g.PaintIconClose(rectangle, Colour.Text.Get("Tag"));
				}
				else
				{
					g.PaintIconClose(rectangle, Colour.TextQuaternary.Get("Tag"));
				}
			}
			return;
		}
		bool right = (int)((Control)this).RightToLeft == 1;
		RectTextLR rectTextLR = ReadRectangle.IconRect(font_size.Height, HasImage, closeIcon, right, muit: false);
		rect_close = rectTextLR.r;
		if (closeIcon)
		{
			if (hover_close.Animation)
			{
				g.PaintIconClose(rectTextLR.r, Helper.ToColor(hover_close.Value + Colour.TextQuaternary.Get("Tag").A, Colour.Text.Get("Tag")), 0.8f);
			}
			else if (hover_close.Switch)
			{
				g.PaintIconClose(rectTextLR.r, Colour.Text.Get("Tag"), 0.8f);
			}
			else
			{
				g.PaintIconClose(rectTextLR.r, Colour.TextQuaternary.Get("Tag"), 0.8f);
			}
		}
		PaintImage(g, color, rectTextLR.l);
		SolidBrush val = new SolidBrush(color);
		try
		{
			g.String(text, ((Control)this).Font, (Brush)(object)val, rectTextLR.text, stringFormat);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private bool PaintImageNoText(Canvas g, Color? color, Size font_size, Rectangle rect_read)
	{
		if (imageSvg != null)
		{
			g.GetImgExtend(imageSvg, GetImageRectCenter(font_size, rect_read), color);
			return false;
		}
		if (image != null)
		{
			g.Image(image, GetImageRectCenter(font_size, rect_read));
			return false;
		}
		return true;
	}

	private Rectangle GetImageRectCenter(Size font_size, Rectangle rect_read)
	{
		if (ImageSize.Width > 0 && ImageSize.Height > 0)
		{
			int num = (int)((float)ImageSize.Width * Config.Dpi);
			int num2 = (int)((float)ImageSize.Height * Config.Dpi);
			return new Rectangle(rect_read.X + (rect_read.Width - num) / 2, rect_read.Y + (rect_read.Height - num2) / 2, num, num2);
		}
		int num3 = (int)((float)font_size.Height * 0.8f);
		return new Rectangle(rect_read.X + (rect_read.Width - num3) / 2, rect_read.Y + (rect_read.Height - num3) / 2, num3, num3);
	}

	private void PaintImage(Canvas g, Color? color, Rectangle rectl)
	{
		if (imageSvg != null)
		{
			g.GetImgExtend(imageSvg, GetImageRect(rectl), color);
		}
		else if (image != null)
		{
			g.Image(image, GetImageRect(rectl));
		}
	}

	private Rectangle GetImageRect(Rectangle rectl)
	{
		if (ImageSize.Width > 0 && ImageSize.Height > 0)
		{
			int num = (int)((float)ImageSize.Width * Config.Dpi);
			int num2 = (int)((float)ImageSize.Height * Config.Dpi);
			return new Rectangle(rectl.X + (rectl.Width - num) / 2, rectl.Y + (rectl.Height - num2) / 2, num, num2);
		}
		return rectl;
	}

	public Tag()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		((Control)this).BackColor = Color.Transparent;
		hover_close = new ITaskOpacity((IControl)this);
	}

	protected override void OnMouseClick(MouseEventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		if ((int)e.Button == 1048576 && closeIcon && rect_close.Contains(e.Location))
		{
			bool flag = false;
			if (this.CloseChanged == null || this.CloseChanged(this, EventArgs.Empty))
			{
				flag = true;
			}
			if (flag)
			{
				((Component)(object)this).Dispose();
			}
		}
		else
		{
			((Control)this).OnMouseClick(e);
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		if (closeIcon)
		{
			hover_close.MaxValue = Colour.Text.Get("Tag").A - Colour.TextQuaternary.Get("Tag").A;
			hover_close.Switch = rect_close.Contains(e.Location);
			SetCursor(hover_close.Switch);
		}
		else
		{
			SetCursor(val: false);
		}
		((Control)this).OnMouseMove(e);
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
