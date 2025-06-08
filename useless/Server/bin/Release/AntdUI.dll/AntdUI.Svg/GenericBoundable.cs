using System.Drawing;

namespace AntdUI.Svg;

internal class GenericBoundable : ISvgBoundable
{
	private RectangleF _rect;

	public PointF Location => _rect.Location;

	public SizeF Size => _rect.Size;

	public RectangleF Bounds => _rect;

	public GenericBoundable(RectangleF rect)
	{
		_rect = rect;
	}

	public GenericBoundable(float x, float y, float width, float height)
	{
		_rect = new RectangleF(x, y, width, height);
	}
}
