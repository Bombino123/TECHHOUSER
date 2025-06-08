using System;
using System.Drawing;

namespace AntdUI;

public class CellBadge : ICell
{
	private Color? fore;

	private Color? fill;

	private TState _state = TState.Default;

	private float dotratio = 0.4f;

	private string? _text;

	private int TxtHeight;

	private Rectangle RectDot;

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
				OnPropertyChanged();
			}
		}
	}

	public TState State
	{
		get
		{
			return _state;
		}
		set
		{
			if (_state != value)
			{
				TState state = _state;
				_state = value;
				if (value == TState.Processing)
				{
					OnPropertyChanged(layout: true);
				}
				else if (state == TState.Processing)
				{
					OnPropertyChanged(layout: true);
				}
				else
				{
					OnPropertyChanged();
				}
			}
		}
	}

	public float DotRatio
	{
		get
		{
			return dotratio;
		}
		set
		{
			if (dotratio != value)
			{
				dotratio = value;
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

	public CellBadge(string text)
	{
		_text = text;
	}

	public CellBadge(TState state)
	{
		_state = state;
	}

	public CellBadge(TState state, string text)
	{
		_state = state;
		_text = text;
	}

	public override string? ToString()
	{
		return _text;
	}

	public override void PaintBack(Canvas g)
	{
	}

	public override void Paint(Canvas g, Font font, bool enable, SolidBrush fore)
	{
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Expected O, but got Unknown
		Color color;
		if (Fill.HasValue)
		{
			color = Fill.Value;
		}
		else
		{
			switch (State)
			{
			case TState.Success:
				color = Colour.Success.Get("Badge");
				break;
			case TState.Error:
				color = Colour.Error.Get("Badge");
				break;
			case TState.Primary:
			case TState.Processing:
				color = Colour.Primary.Get("Badge");
				break;
			case TState.Warn:
				color = Colour.Warning.Get("Badge");
				break;
			default:
				color = Colour.TextQuaternary.Get("Badge");
				break;
			}
		}
		SolidBrush val = new SolidBrush(color);
		try
		{
			if (State == TState.Processing && base.PARENT.PARENT != null)
			{
				float num = (float)TxtHeight * base.PARENT.PARENT.AnimationStateValue;
				float alpha = 255f * (1f - base.PARENT.PARENT.AnimationStateValue);
				g.DrawEllipse(Helper.ToColor(alpha, val.Color), 4f * Config.Dpi, new RectangleF((float)RectDot.X + ((float)RectDot.Width - num) / 2f, (float)RectDot.Y + ((float)RectDot.Height - num) / 2f, num, num));
			}
			g.FillEllipse((Brush)(object)val, RectDot);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		if (Fore.HasValue)
		{
			g.String(Text, font, Fore.Value, base.Rect, Table.StringF(base.PARENT.COLUMN));
		}
		else
		{
			g.String(Text, font, (Brush)(object)fore, base.Rect, Table.StringF(base.PARENT.COLUMN));
		}
	}

	public override Size GetSize(Canvas g, Font font, int gap, int gap2)
	{
		if (string.IsNullOrEmpty(Text))
		{
			Size size = g.MeasureString("龍Qq", font);
			return new Size(size.Height, size.Height);
		}
		Size size2 = g.MeasureString(Text, font);
		return new Size(size2.Width + size2.Height, size2.Height);
	}

	public override void SetRect(Canvas g, Font font, Rectangle rect, Size size, int gap, int gap2)
	{
		TxtHeight = size.Height;
		int num = (int)((float)size.Height * dotratio);
		if (string.IsNullOrEmpty(Text))
		{
			RectDot = new Rectangle(rect.X + (rect.Width - num) / 2, rect.Y + (rect.Height - num) / 2, num, num);
			return;
		}
		base.Rect = new Rectangle(rect.X + size.Height, rect.Y, rect.Width - size.Height, rect.Height);
		RectDot = new Rectangle(rect.X + (size.Height - num) / 2, rect.Y + (rect.Height - num) / 2, num, num);
	}
}
