using System.Drawing.Drawing2D;
using System.Globalization;

namespace AntdUI.Svg.Transforms;

public sealed class SvgRotate : SvgTransform
{
	public float Angle { get; set; }

	public float CenterX { get; set; }

	public float CenterY { get; set; }

	public override Matrix Matrix(float w, float h)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		Matrix val = new Matrix();
		val.Translate(CenterX, CenterY);
		val.Rotate(Angle);
		val.Translate(0f - CenterX, 0f - CenterY);
		return val;
	}

	public override string WriteToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "rotate({0}, {1}, {2})", Angle, CenterX, CenterY);
	}

	public SvgRotate(float angle)
	{
		Angle = angle;
	}

	public SvgRotate(float angle, float centerX, float centerY)
		: this(angle)
	{
		CenterX = centerX;
		CenterY = centerY;
	}

	public override object Clone()
	{
		return new SvgRotate(Angle, CenterX, CenterY);
	}
}
