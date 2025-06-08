using System.ComponentModel;
using System.Drawing;

namespace AntdUI.Svg;

public struct SvgPoint
{
	private SvgUnit x;

	private SvgUnit y;

	public SvgUnit X
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

	public SvgUnit Y
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

	public PointF ToDeviceValue(ISvgRenderer renderer, SvgElement owner)
	{
		return SvgUnit.GetDevicePoint(X, Y, renderer, owner);
	}

	public bool IsEmpty()
	{
		if (X.Value == 0f)
		{
			return Y.Value == 0f;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!(obj.GetType() == typeof(SvgPoint)))
		{
			return false;
		}
		SvgPoint svgPoint = (SvgPoint)obj;
		if (svgPoint.X.Equals(X))
		{
			return svgPoint.Y.Equals(Y);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public SvgPoint(string _x, string _y)
	{
		TypeConverter converter = TypeDescriptor.GetConverter(typeof(SvgUnit));
		x = (SvgUnit)converter.ConvertFrom(_x);
		y = (SvgUnit)converter.ConvertFrom(_y);
	}

	public SvgPoint(SvgUnit _x, SvgUnit _y)
	{
		x = _x;
		y = _y;
	}
}
