using System.Drawing;
using System.Drawing.Drawing2D;

namespace AntdUI.Svg.Pathing;

public sealed class SvgLineSegment : SvgPathSegment
{
	public SvgLineSegment(PointF start, PointF end)
	{
		base.Start = start;
		base.End = end;
	}

	public override void AddToPath(GraphicsPath graphicsPath)
	{
		graphicsPath.AddLine(base.Start, base.End);
	}

	public override string ToString()
	{
		return "L" + base.End.ToSvgString();
	}
}
