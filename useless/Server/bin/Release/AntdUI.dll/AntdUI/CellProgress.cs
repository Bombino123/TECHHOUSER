using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace AntdUI;

public class CellProgress : ICell
{
	private Color? back;

	private Color? fill;

	private int radius = 6;

	private TShape shape = TShape.Round;

	private float _value;

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

	public TShape Shape
	{
		get
		{
			return shape;
		}
		set
		{
			if (shape != value)
			{
				shape = value;
				OnPropertyChanged(layout: true);
			}
		}
	}

	public float Value
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
				OnPropertyChanged();
			}
		}
	}

	public CellProgress(float value)
	{
		_value = value;
	}

	public override string ToString()
	{
		return _value * 100f + "%";
	}

	public override void PaintBack(Canvas g)
	{
	}

	public override void Paint(Canvas g, Font font, bool enable, SolidBrush fore)
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected O, but got Unknown
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Expected O, but got Unknown
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Expected O, but got Unknown
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Expected O, but got Unknown
		Color color = Fill ?? Colour.Primary.Get("Progress");
		Color color2 = Back ?? Colour.FillSecondary.Get("Progress");
		if (Shape == TShape.Circle)
		{
			float num = (float)Radius * Config.Dpi;
			g.DrawEllipse(color2, num, base.Rect);
			if (Value > 0f)
			{
				int num2 = (int)(360f * Value);
				Pen val = new Pen(color, num);
				try
				{
					LineCap startCap = (LineCap)2;
					val.EndCap = (LineCap)2;
					val.StartCap = startCap;
					g.DrawArc(val, base.Rect, -90f, num2);
					return;
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			return;
		}
		float num3 = (float)Radius * Config.Dpi;
		if (Shape == TShape.Round)
		{
			num3 = base.Rect.Height;
		}
		GraphicsPath val2 = base.Rect.RoundPath(num3);
		try
		{
			g.Fill(color2, val2);
			if (!(Value > 0f))
			{
				return;
			}
			float num4 = (float)base.Rect.Width * Value;
			if (num4 > num3)
			{
				GraphicsPath val3 = new RectangleF(base.Rect.X, base.Rect.Y, num4, base.Rect.Height).RoundPath(num3);
				try
				{
					g.Fill(color, val3);
					return;
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
			}
			Bitmap val4 = new Bitmap(base.Rect.Width, base.Rect.Height);
			try
			{
				using (Canvas canvas = Graphics.FromImage((Image)(object)val4).High())
				{
					SolidBrush val5 = new SolidBrush(color);
					try
					{
						canvas.FillEllipse((Brush)(object)val5, new RectangleF(0f, 0f, num4, base.Rect.Height));
					}
					finally
					{
						((IDisposable)val5)?.Dispose();
					}
				}
				TextureBrush val6 = new TextureBrush((Image)(object)val4, (WrapMode)4);
				try
				{
					val6.TranslateTransform((float)base.Rect.X, (float)base.Rect.Y);
					g.Fill((Brush)(object)val6, val2);
				}
				finally
				{
					((IDisposable)val6)?.Dispose();
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
	}

	public override Size GetSize(Canvas g, Font font, int gap, int gap2)
	{
		int height = g.MeasureString("龍Qq", font).Height;
		if (Shape == TShape.Circle)
		{
			int num = gap2 + height;
			return new Size(num, num);
		}
		return new Size(height * 2, height / 2);
	}

	public override void SetRect(Canvas g, Font font, Rectangle rect, Size size, int gap, int gap2)
	{
		int num = rect.Width;
		int num2 = size.Height;
		if (Shape == TShape.Circle)
		{
			num = size.Width - gap2;
			num2 = size.Height - gap2;
		}
		base.Rect = new Rectangle(rect.X + (rect.Width - num) / 2, rect.Y + (rect.Height - num2) / 2, num, num2);
	}
}
