using System.Drawing;

namespace AntdUI.Svg;

public interface ISvgBoundable
{
	PointF Location { get; }

	SizeF Size { get; }

	RectangleF Bounds { get; }
}
