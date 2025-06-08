using System.Drawing;
using System.Drawing.Drawing2D;

namespace AntdUI.Svg.Pathing;

public sealed class SvgQuadraticCurveSegment : SvgPathSegment
{
	private PointF _controlPoint;

	public PointF ControlPoint
	{
		get
		{
			return _controlPoint;
		}
		set
		{
			_controlPoint = value;
		}
	}

	private PointF FirstControlPoint
	{
		get
		{
			float x = base.Start.X + (ControlPoint.X - base.Start.X) * 2f / 3f;
			float y = base.Start.Y + (ControlPoint.Y - base.Start.Y) * 2f / 3f;
			return new PointF(x, y);
		}
	}

	private PointF SecondControlPoint
	{
		get
		{
			float x = ControlPoint.X + (base.End.X - ControlPoint.X) / 3f;
			float y = ControlPoint.Y + (base.End.Y - ControlPoint.Y) / 3f;
			return new PointF(x, y);
		}
	}

	public SvgQuadraticCurveSegment(PointF start, PointF controlPoint, PointF end)
	{
		base.Start = start;
		_controlPoint = controlPoint;
		base.End = end;
	}

	public override void AddToPath(GraphicsPath graphicsPath)
	{
		graphicsPath.AddBezier(base.Start, FirstControlPoint, SecondControlPoint, base.End);
	}

	public override string ToString()
	{
		return "Q" + ControlPoint.ToSvgString() + " " + base.End.ToSvgString();
	}
}
