using System.Drawing.Drawing2D;
using System.Globalization;

namespace AntdUI.Svg.Transforms;

public sealed class SvgShear : SvgTransform
{
	private float shearFactorX;

	private float shearFactorY;

	public float X
	{
		get
		{
			return shearFactorX;
		}
		set
		{
			shearFactorX = value;
		}
	}

	public float Y
	{
		get
		{
			return shearFactorY;
		}
		set
		{
			shearFactorY = value;
		}
	}

	public override Matrix Matrix(float w, float h)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		Matrix val = new Matrix();
		val.Shear(X, Y);
		return val;
	}

	public override string WriteToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "shear({0}, {1})", X, Y);
	}

	public SvgShear(float x)
		: this(x, x)
	{
	}

	public SvgShear(float x, float y)
	{
		shearFactorX = x;
		shearFactorY = y;
	}

	public override object Clone()
	{
		return new SvgShear(X, Y);
	}
}
