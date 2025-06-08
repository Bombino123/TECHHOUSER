using System.Drawing;

namespace AntdUI.Svg;

public static class PointFExtensions
{
	public static string ToSvgString(this PointF p)
	{
		return p.X + " " + p.Y;
	}
}
