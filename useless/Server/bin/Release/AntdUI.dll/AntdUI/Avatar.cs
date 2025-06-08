using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows.Forms;
using AntdUI.Design;

namespace AntdUI;

[Description("Avatar 头像")]
[ToolboxItem(true)]
[DefaultProperty("Image")]
[Designer(typeof(IControlDesigner))]
public class Avatar : IControl, ShadowConfig
{
	private Color back = Color.Transparent;

	private string? text;

	private int radius;

	private bool round;

	private Image? image;

	private object _lock = new object();

	private string? imageSvg;

	private TFit imageFit = TFit.Cover;

	private bool loading;

	private float _value;

	private float borderWidth;

	private Color borColor = Color.FromArgb(246, 248, 250);

	private int shadow;

	private float shadowOpacity = 0.3f;

	private int shadowOffsetX;

	private int shadowOffsetY;

	private readonly StringFormat stringCenter = Helper.SF_ALL((StringAlignment)1, (StringAlignment)1);

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

	[Description("背景颜色")]
	[Category("外观")]
	[DefaultValue(typeof(Color), "Transparent")]
	public Color BackColor
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
				if (value != null && value.Length > 1)
				{
					value = value.Substring(0, 1);
				}
				text = value;
				((Control)this).Invalidate();
				((Control)this).OnTextChanged(EventArgs.Empty);
				OnPropertyChanged("Text");
			}
		}
	}

	[Description("文本")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationText { get; set; }

	[Description("圆角")]
	[Category("外观")]
	[DefaultValue(0)]
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
				OnPropertyChanged("Radius");
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
				OnPropertyChanged("Round");
			}
		}
	}

	[Description("图片")]
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
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Expected O, but got Unknown
			if (image == value)
			{
				return;
			}
			image = value;
			if (value != null && PlayGIF)
			{
				FrameDimension val = new FrameDimension(value.FrameDimensionsList[0]);
				int frameCount = value.GetFrameCount(val);
				if (frameCount > 1)
				{
					PlayGif(value, val, frameCount);
				}
				else
				{
					((Control)this).Invalidate();
				}
			}
			else
			{
				((Control)this).Invalidate();
			}
			OnPropertyChanged("Image");
		}
	}

	[Description("播放GIF")]
	[Category("行为")]
	[DefaultValue(true)]
	public bool PlayGIF { get; set; } = true;


	[Description("图片SVG")]
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
				((Control)this).Invalidate();
				OnPropertyChanged("ImageSvg");
			}
		}
	}

	[Description("图片布局")]
	[Category("外观")]
	[DefaultValue(TFit.Cover)]
	public TFit ImageFit
	{
		get
		{
			return imageFit;
		}
		set
		{
			if (imageFit != value)
			{
				imageFit = value;
				((Control)this).Invalidate();
				OnPropertyChanged("ImageFit");
			}
		}
	}

	[Description("加载状态")]
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
			if (loading != value)
			{
				loading = value;
				((Control)this).Invalidate();
				OnPropertyChanged("Loading");
			}
		}
	}

	[Description("加载进度 0F-1F")]
	[Category("数据")]
	[DefaultValue(0f)]
	public float LoadingProgress
	{
		get
		{
			return _value;
		}
		set
		{
			if (_value != value)
			{
				if (value < 0f)
				{
					value = 0f;
				}
				else if (value > 1f)
				{
					value = 1f;
				}
				_value = value;
				if (loading)
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("LoadingProgress");
			}
		}
	}

	[Description("边框宽度")]
	[Category("边框")]
	[DefaultValue(0f)]
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

	[Description("边框颜色")]
	[Category("边框")]
	[DefaultValue(typeof(Color), "246, 248, 250")]
	public Color BorderColor
	{
		get
		{
			return borColor;
		}
		set
		{
			if (!(borColor == value))
			{
				borColor = value;
				if (borderWidth > 0f)
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("BorderColor");
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

	public override Rectangle ReadRectangle
	{
		get
		{
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			if (borderWidth > 0f)
			{
				return ((Control)this).ClientRectangle.PaddingRect(((Control)this).Padding, borderWidth * Config.Dpi / 2f);
			}
			return ((Control)this).ClientRectangle.PaddingRect(((Control)this).Padding);
		}
	}

	public override GraphicsPath RenderRegion
	{
		get
		{
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Expected O, but got Unknown
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Expected O, but got Unknown
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Expected O, but got Unknown
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Expected O, but got Unknown
			if (borderWidth > 0f)
			{
				Rectangle readRectangle = ReadRectangle;
				if (round)
				{
					GraphicsPath val = new GraphicsPath();
					val.AddEllipse(readRectangle);
					return val;
				}
				if (radius > 0)
				{
					return readRectangle.RoundPath((float)radius * Config.Dpi);
				}
				GraphicsPath val2 = new GraphicsPath();
				val2.AddRectangle(readRectangle);
				return val2;
			}
			Rectangle readRectangle2 = ReadRectangle;
			if (round)
			{
				GraphicsPath val3 = new GraphicsPath();
				val3.AddEllipse(readRectangle2);
				return val3;
			}
			if (radius > 0)
			{
				return readRectangle2.RoundPath((float)radius * Config.Dpi);
			}
			GraphicsPath val4 = new GraphicsPath();
			val4.AddRectangle(readRectangle2);
			return val4;
		}
	}

	public Avatar()
	{
		((Control)this).BackColor = Color.Transparent;
	}

	private void PlayGif(Image value, FrameDimension fd, int count)
	{
		Image value2 = value;
		FrameDimension fd2 = fd;
		int[] delays = GifDelays(value2, count);
		ITask.Run(delegate
		{
			while (image == value2)
			{
				for (int i = 0; i < count; i++)
				{
					if (image != value2)
					{
						return;
					}
					lock (_lock)
					{
						value2.SelectActiveFrame(fd2, i);
					}
					((Control)this).Invalidate();
					Thread.Sleep(delays[i]);
				}
			}
		});
	}

	private int[] GifDelays(Image value, int count)
	{
		int num = 20736;
		PropertyItem propertyItem = value.GetPropertyItem(num);
		if (propertyItem != null)
		{
			byte[] value2 = propertyItem.Value;
			if (value2 != null)
			{
				int[] array = new int[count];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = BitConverter.ToInt32(value2, i * 4) * 10;
				}
				return array;
			}
		}
		int[] array2 = new int[count];
		for (int j = 0; j < array2.Length; j++)
		{
			array2[j] = 100;
		}
		return array2;
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Expected O, but got Unknown
		Rectangle clientRectangle = ((Control)this).ClientRectangle;
		if (clientRectangle.Width == 0 || clientRectangle.Height == 0)
		{
			return;
		}
		Canvas canvas = e.Graphics.High();
		float num = (float)radius * Config.Dpi;
		Rectangle readRectangle = ReadRectangle;
		if (shadow > 0 && shadowOpacity > 0f)
		{
			canvas.PaintShadow(this, clientRectangle, readRectangle, num, round);
		}
		FillRect(canvas, readRectangle, back, num, round);
		if (image != null)
		{
			lock (_lock)
			{
				canvas.Image(readRectangle, image, imageFit, num, round);
			}
		}
		else if (imageSvg != null)
		{
			Bitmap imgExtend = SvgExtend.GetImgExtend(imageSvg, readRectangle, ((Control)this).ForeColor);
			try
			{
				if (imgExtend == null)
				{
					canvas.String(((Control)this).Text, ((Control)this).Font, base.Enabled ? ((Control)this).ForeColor : Colour.TextQuaternary.Get("Avatar"), readRectangle, stringCenter);
				}
				else
				{
					canvas.Image(readRectangle, (Image)(object)imgExtend, imageFit, num, round);
				}
			}
			finally
			{
				((IDisposable)imgExtend)?.Dispose();
			}
		}
		else
		{
			canvas.String(((Control)this).Text, ((Control)this).Font, base.Enabled ? ((Control)this).ForeColor : Colour.TextQuaternary.Get("Avatar"), readRectangle, stringCenter);
		}
		if (borderWidth > 0f)
		{
			DrawRect(canvas, readRectangle, borColor, borderWidth * Config.Dpi, num, round);
		}
		if (loading)
		{
			float num2 = 6f * Config.Dpi;
			int num3 = (int)(40f * Config.Dpi);
			Rectangle rectangle = new Rectangle(readRectangle.X + (readRectangle.Width - num3) / 2, readRectangle.Y + (readRectangle.Height - num3) / 2, num3, num3);
			canvas.DrawEllipse(Color.FromArgb(220, Colour.PrimaryColor.Get("Avatar")), num2, rectangle);
			Pen val = new Pen(Colour.Primary.Get("Avatar"), num2);
			try
			{
				canvas.DrawArc(val, rectangle, -90f, 360f * _value);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		this.PaintBadge(canvas);
		((Control)this).OnPaint(e);
	}

	private void FillRect(Canvas g, Rectangle rect, Color color, float radius, bool round)
	{
		if (round)
		{
			g.FillEllipse(color, rect);
			return;
		}
		if (radius > 0f)
		{
			GraphicsPath val = rect.RoundPath(radius);
			try
			{
				g.Fill(color, val);
				return;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		g.Fill(color, rect);
	}

	private void DrawRect(Canvas g, Rectangle rect, Color color, float width, float radius, bool round)
	{
		if (round)
		{
			g.DrawEllipse(color, width, rect);
			return;
		}
		if (radius > 0f)
		{
			GraphicsPath val = rect.RoundPath(radius);
			try
			{
				g.Draw(color, width, val);
				return;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		g.Draw(color, width, rect);
	}
}
