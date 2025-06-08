using System.Drawing.Drawing2D;
using System.Globalization;

namespace AntdUI.Svg.Transforms;

public sealed class SvgTranslate : SvgTransform
{
	private float x;

	private float y;

	private bool RX;

	private bool RY;

	public float X
	{
		get
		{
			return x;
		}
		set
		{
			x = value;
		}
	}

	public float Y
	{
		get
		{
			return y;
		}
		set
		{
			y = value;
		}
	}

	public override Matrix Matrix(float w, float h)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		Matrix val = new Matrix();
		if (RX || RY)
		{
			if (RX && RY)
			{
				val.Translate(w * (X / 100f), h * (Y / 100f));
			}
			else if (RX)
			{
				val.Translate(w * (X / 100f), Y);
			}
			else
			{
				val.Translate(X, h * (Y / 100f));
			}
		}
		else
		{
			val.Translate(X, Y);
		}
		return val;
	}

	public override string WriteToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "translate({0}, {1})", X, Y);
	}

	public SvgTranslate(float _x, bool ratio_x, float _y, bool ratio_y)
	{
		x = _x;
		y = _y;
		RX = ratio_x;
		RY = ratio_y;
	}

	public override object Clone()
	{
		return new SvgTranslate(x, RX, y, RY);
	}
}
