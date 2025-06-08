using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace AntdUI;

public class CellTag : ICell
{
	private Color? fore;

	private Color? back;

	private float borderWidth = 1f;

	private TTypeMini _type;

	private string _text;

	public Color? Fore
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
				OnPropertyChanged();
			}
		}
	}

	public Color? Back
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
				OnPropertyChanged();
			}
		}
	}

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
				OnPropertyChanged();
			}
		}
	}

	public TTypeMini Type
	{
		get
		{
			return _type;
		}
		set
		{
			if (_type != value)
			{
				_type = value;
				OnPropertyChanged();
			}
		}
	}

	public string Text
	{
		get
		{
			return _text;
		}
		set
		{
			if (!(_text == value))
			{
				_text = value;
				OnPropertyChanged(layout: true);
			}
		}
	}

	public CellTag(string text)
	{
		_text = text;
	}

	public CellTag(string text, TTypeMini type)
	{
		_text = text;
		_type = type;
	}

	public override string ToString()
	{
		return _text;
	}

	public override void PaintBack(Canvas g)
	{
	}

	public override void Paint(Canvas g, Font font, bool enable, SolidBrush fore)
	{
		GraphicsPath val = base.Rect.RoundPath(6f);
		try
		{
			Color color;
			Color color2;
			Color color3;
			switch (Type)
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
			if (Fore.HasValue)
			{
				color2 = Fore.Value;
			}
			if (Back.HasValue)
			{
				color = Back.Value;
			}
			g.Fill(color, val);
			if (borderWidth > 0f)
			{
				g.Draw(color3, borderWidth * Config.Dpi, val);
			}
			g.String(Text, font, color2, base.Rect, Table.stringCenter);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public override Size GetSize(Canvas g, Font font, int gap, int gap2)
	{
		Size size = g.MeasureString(Text, font);
		return new Size(size.Width + gap2, size.Height + gap);
	}

	public override void SetRect(Canvas g, Font font, Rectangle rect, Size size, int gap, int gap2)
	{
		base.Rect = new Rectangle(rect.X, rect.Y + (rect.Height - size.Height) / 2, rect.Width, size.Height);
	}
}
