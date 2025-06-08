using System.Drawing.Drawing2D;
using System.Globalization;

namespace AntdUI.Svg.Transforms;

public sealed class SvgScale : SvgTransform
{
	private float scaleFactorX;

	private float scaleFactorY;

	public float X
	{
		get
		{
			return scaleFactorX;
		}
		set
		{
			scaleFactorX = value;
		}
	}

	public float Y
	{
		get
		{
			return scaleFactorY;
		}
		set
		{
			scaleFactorY = value;
		}
	}

	public override Matrix Matrix(float w, float h)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		Matrix val = new Matrix();
		val.Scale(X, Y);
		return val;
	}

	public override string WriteToString()
	{
		if (X == Y)
		{
			return string.Format(CultureInfo.InvariantCulture, "scale({0})", X);
		}
		return string.Format(CultureInfo.InvariantCulture, "scale({0}, {1})", X, Y);
	}

	public SvgScale(float x)
		: this(x, x)
	{
	}

	public SvgScale(float x, float y)
	{
		scaleFactorX = x;
		scaleFactorY = y;
	}

	public override object Clone()
	{
		return new SvgScale(X, Y);
	}
}
