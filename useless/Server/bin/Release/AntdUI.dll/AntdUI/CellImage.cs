using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace AntdUI;

public class CellImage : ICell
{
	private Color? bordercolor;

	private float borderwidth;

	private int radius = 6;

	private bool round;

	private Size? size;

	private TFit imageFit = TFit.Cover;

	private Bitmap? image;

	private string? imageSvg;

	private Color? fillSvg;

	public Color? BorderColor
	{
		get
		{
			return bordercolor;
		}
		set
		{
			if (!(bordercolor == value))
			{
				bordercolor = value;
				if (borderwidth > 0f)
				{
					OnPropertyChanged();
				}
			}
		}
	}

	public float BorderWidth
	{
		get
		{
			return borderwidth;
		}
		set
		{
			if (borderwidth != value)
			{
				borderwidth = value;
				OnPropertyChanged();
			}
		}
	}

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
				OnPropertyChanged();
			}
		}
	}

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
				OnPropertyChanged();
			}
		}
	}

	public Size? Size
	{
		get
		{
			return size;
		}
		set
		{
			if (!(size == value))
			{
				size = value;
				OnPropertyChanged(layout: true);
			}
		}
	}

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
				OnPropertyChanged();
			}
		}
	}

	public Bitmap? Image
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
				OnPropertyChanged();
			}
		}
	}

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
				OnPropertyChanged();
			}
		}
	}

	public Color? FillSvg
	{
		get
		{
			return fillSvg;
		}
		set
		{
			if (fillSvg == value)
			{
				fillSvg = value;
			}
			fillSvg = value;
			OnPropertyChanged();
		}
	}

	public string? Tooltip { get; set; }

	public CellImage(Bitmap img)
	{
		image = img;
	}

	public CellImage(string svg)
	{
		imageSvg = svg;
	}

	public CellImage(string svg, Color svgcolor)
	{
		imageSvg = svg;
		fillSvg = svgcolor;
	}

	public CellImage(Bitmap img, int _radius)
	{
		image = img;
		radius = _radius;
	}

	public override string? ToString()
	{
		return null;
	}

	public override void PaintBack(Canvas g)
	{
	}

	public override void Paint(Canvas g, Font font, bool enable, SolidBrush fore)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Expected O, but got Unknown
		float num = (float)Radius * Config.Dpi;
		GraphicsPath val = base.Rect.RoundPath(num);
		try
		{
			Bitmap val2 = new Bitmap(base.Rect.Width, base.Rect.Height);
			try
			{
				using (Canvas canvas = Graphics.FromImage((Image)(object)val2).High())
				{
					if (ImageSvg != null)
					{
						Bitmap val3 = ImageSvg.SvgToBmp(base.Rect.Width, base.Rect.Height, FillSvg);
						try
						{
							if (val3 != null)
							{
								canvas.Image(new RectangleF(0f, 0f, base.Rect.Width, base.Rect.Height), (Image)(object)val3, ImageFit);
							}
						}
						finally
						{
							((IDisposable)val3)?.Dispose();
						}
					}
					else if (image != null)
					{
						canvas.Image(new RectangleF(0f, 0f, base.Rect.Width, base.Rect.Height), (Image)(object)image, ImageFit);
					}
				}
				TextureBrush val4 = new TextureBrush((Image)(object)val2, (WrapMode)4);
				try
				{
					val4.TranslateTransform((float)base.Rect.X, (float)base.Rect.Y);
					if (Round)
					{
						g.FillEllipse((Brush)(object)val4, base.Rect);
					}
					else
					{
						g.Fill((Brush)(object)val4, val);
					}
				}
				finally
				{
					((IDisposable)val4)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
			if (BorderWidth > 0f && BorderColor.HasValue)
			{
				g.Draw(BorderColor.Value, BorderWidth * Config.Dpi, val);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public override Size GetSize(Canvas g, Font font, int gap, int gap2)
	{
		if (Size.HasValue)
		{
			return new Size((int)Math.Ceiling((float)Size.Value.Width * Config.Dpi), (int)Math.Ceiling((float)Size.Value.Height * Config.Dpi));
		}
		int num = gap2 + g.MeasureString("龍Qq", font).Height;
		return new Size(num, num);
	}

	public override void SetRect(Canvas g, Font font, Rectangle rect, Size size, int gap, int gap2)
	{
		int num = size.Width - gap2;
		int num2 = size.Height - gap2;
		base.Rect = new Rectangle(rect.X + (rect.Width - num) / 2, rect.Y + (rect.Height - num2) / 2, num, num2);
	}
}
