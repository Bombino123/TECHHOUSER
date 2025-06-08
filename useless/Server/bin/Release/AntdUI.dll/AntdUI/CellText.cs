using System.Drawing;
using System.Drawing.Drawing2D;

namespace AntdUI;

public class CellText : ICell
{
	private Color? _back;

	private Color? _fore;

	private Font? _font;

	private string? _text;

	private float iconratio = 0.7f;

	private Image? prefix;

	private string? prefixSvg;

	private Image? suffix;

	private string? suffixSvg;

	private Rectangle RectL;

	private Rectangle RectR;

	public Color? Back
	{
		get
		{
			return _back;
		}
		set
		{
			if (!(_back == value))
			{
				_back = value;
				OnPropertyChanged();
			}
		}
	}

	public Color? Fore
	{
		get
		{
			return _fore;
		}
		set
		{
			if (!(_fore == value))
			{
				_fore = value;
				OnPropertyChanged();
			}
		}
	}

	public Font? Font
	{
		get
		{
			return _font;
		}
		set
		{
			if (_font != value)
			{
				_font = value;
				OnPropertyChanged();
			}
		}
	}

	public string? Text
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
				OnPropertyChanged(layout: true);
			}
		}
	}

	public Image? Prefix
	{
		get
		{
			return prefix;
		}
		set
		{
			if (prefix != value)
			{
				prefix = value;
				OnPropertyChanged(layout: true);
			}
		}
	}

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
				OnPropertyChanged(layout: true);
			}
		}
	}

	public bool HasPrefix
	{
		get
		{
			if (prefixSvg == null)
			{
				return prefix != null;
			}
			return true;
		}
	}

	public Image? Suffix
	{
		get
		{
			return suffix;
		}
		set
		{
			if (suffix != value)
			{
				suffix = value;
				OnPropertyChanged(layout: true);
			}
		}
	}

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
				OnPropertyChanged(layout: true);
			}
		}
	}

	public bool HasSuffix
	{
		get
		{
			if (suffixSvg == null)
			{
				return suffix != null;
			}
			return true;
		}
	}

	public CellText(string text)
	{
		_text = text;
	}

	public CellText(string text, Color fore)
	{
		_text = text;
		_fore = fore;
	}

	public override string? ToString()
	{
		return _text;
	}

	public override void PaintBack(Canvas g)
	{
		if (Back.HasValue)
		{
			g.Fill(Back.Value, base.PARENT.RECT);
		}
	}

	public override void Paint(Canvas g, Font font, bool enable, SolidBrush fore)
	{
		GraphicsState state = g.Save();
		g.SetClip(base.Rect);
		if (Fore.HasValue)
		{
			g.String(Text, Font ?? font, Fore.Value, base.Rect, Table.StringF(base.PARENT.COLUMN));
		}
		else
		{
			g.String(Text, Font ?? font, (Brush)(object)fore, base.Rect, Table.StringF(base.PARENT.COLUMN));
		}
		g.Restore(state);
		if (PrefixSvg != null)
		{
			g.GetImgExtend(PrefixSvg, RectL, Fore ?? fore.Color);
		}
		else if (Prefix != null)
		{
			g.Image(Prefix, RectL);
		}
		if (SuffixSvg != null)
		{
			g.GetImgExtend(SuffixSvg, RectR, Fore ?? fore.Color);
		}
		else if (Suffix != null)
		{
			g.Image(Suffix, RectR);
		}
	}

	public override Size GetSize(Canvas g, Font font, int gap, int gap2)
	{
		Size size = g.MeasureString(Text, Font ?? font);
		bool hasPrefix = HasPrefix;
		bool hasSuffix = HasSuffix;
		if (hasPrefix && hasSuffix)
		{
			return new Size((int)((float)size.Height * IconRatio) * 2 + gap2 + size.Width, size.Height);
		}
		if (hasPrefix || hasSuffix)
		{
			return new Size((int)((float)size.Height * IconRatio) + gap + size.Width, size.Height);
		}
		return new Size(size.Width, size.Height);
	}

	public override void SetRect(Canvas g, Font font, Rectangle rect, Size size, int gap, int gap2)
	{
		bool hasPrefix = HasPrefix;
		bool hasSuffix = HasSuffix;
		if (hasPrefix && hasSuffix)
		{
			int num = (int)((float)size.Height * IconRatio);
			RectL = new Rectangle(rect.X, rect.Y + (rect.Height - num) / 2, num, num);
			RectR = new Rectangle(rect.Right - num, RectL.Y, num, num);
			base.Rect = new Rectangle(RectL.Right + gap, rect.Y + (rect.Height - size.Height) / 2, rect.Width - (num * 2 + gap2), size.Height);
		}
		else if (hasPrefix)
		{
			int num2 = (int)((float)size.Height * IconRatio);
			RectL = new Rectangle(rect.X, rect.Y + (rect.Height - num2) / 2, num2, num2);
			base.Rect = new Rectangle(RectL.Right + gap, rect.Y + (rect.Height - size.Height) / 2, rect.Width - num2 - gap, size.Height);
		}
		else if (hasSuffix)
		{
			int num3 = (int)((float)size.Height * IconRatio);
			RectR = new Rectangle(rect.Right - num3, rect.Y + (rect.Height - num3) / 2, num3, num3);
			base.Rect = new Rectangle(rect.X, rect.Y + (rect.Height - size.Height) / 2, rect.Width - num3 - gap2, size.Height);
		}
		else
		{
			base.Rect = new Rectangle(rect.X, rect.Y + (rect.Height - size.Height) / 2, rect.Width, size.Height);
		}
	}
}
