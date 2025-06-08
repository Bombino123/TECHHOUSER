using System.Drawing;
using System.Drawing.Drawing2D;

namespace AntdUI.Svg.Pathing;

public class SvgMoveToSegment : SvgPathSegment
{
	public SvgMoveToSegment(PointF moveTo)
	{
		base.Start = moveTo;
		base.End = moveTo;
	}

	public override void AddToPath(GraphicsPath graphicsPath)
	{
		graphicsPath.StartFigure();
	}

	public override string ToString()
	{
		return "M" + base.Start.ToSvgString();
	}
}
