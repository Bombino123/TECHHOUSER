using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace AntdUI.Svg.Transforms;

public sealed class SvgMatrix : SvgTransform
{
	private List<float> points;

	public List<float> Points
	{
		get
		{
			return points;
		}
		set
		{
			points = value;
		}
	}

	public override Matrix Matrix(float w, float h)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		return new Matrix(points[0], points[1], points[2], points[3], points[4], points[5]);
	}

	public override string WriteToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "matrix({0}, {1}, {2}, {3}, {4}, {5})", points[0], points[1], points[2], points[3], points[4], points[5]);
	}

	public SvgMatrix(List<float> m)
	{
		points = m;
	}

	public override object Clone()
	{
		return new SvgMatrix(Points);
	}
}
