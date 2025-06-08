using System.Drawing;
using System.Drawing.Drawing2D;

namespace AntdUI.Svg.Pathing;

public sealed class SvgCubicCurveSegment : SvgPathSegment
{
	private PointF _firstControlPoint;

	private PointF _secondControlPoint;

	public PointF FirstControlPoint
	{
		get
		{
			return _firstControlPoint;
		}
		set
		{
			_firstControlPoint = value;
		}
	}

	public PointF SecondControlPoint
	{
		get
		{
			return _secondControlPoint;
		}
		set
		{
			_secondControlPoint = value;
		}
	}

	public SvgCubicCurveSegment(PointF start, PointF firstControlPoint, PointF secondControlPoint, PointF end)
	{
		base.Start = start;
		base.End = end;
		_firstControlPoint = firstControlPoint;
		_secondControlPoint = secondControlPoint;
	}

	public override void AddToPath(GraphicsPath graphicsPath)
	{
		graphicsPath.AddBezier(base.Start, FirstControlPoint, SecondControlPoint, base.End);
	}

	public override string ToString()
	{
		return "C" + FirstControlPoint.ToSvgString() + " " + SecondControlPoint.ToSvgString() + " " + base.End.ToSvgString();
	}
}
