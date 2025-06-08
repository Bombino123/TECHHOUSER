using System;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace AntdUI.Svg.Transforms;

public sealed class SvgSkew : SvgTransform
{
	public float AngleX { get; set; }

	public float AngleY { get; set; }

	public override Matrix Matrix(float w, float h)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		Matrix val = new Matrix();
		val.Shear((float)Math.Tan((double)(AngleX / 180f) * Math.PI), (float)Math.Tan((double)(AngleY / 180f) * Math.PI));
		return val;
	}

	public override string WriteToString()
	{
		if (AngleY == 0f)
		{
			return string.Format(CultureInfo.InvariantCulture, "skewX({0})", AngleX);
		}
		return string.Format(CultureInfo.InvariantCulture, "skewY({0})", AngleY);
	}

	public SvgSkew(float x, float y)
	{
		AngleX = x;
		AngleY = y;
	}

	public override object Clone()
	{
		return new SvgSkew(AngleX, AngleY);
	}
}
