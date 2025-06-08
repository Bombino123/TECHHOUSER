using System.Drawing;
using System.Drawing.Drawing2D;

namespace AntdUI.Svg;

public class SvgForeignObject : SvgVisualElement
{
	public override string ClassName => "foreignObject";

	public override RectangleF Bounds
	{
		get
		{
			RectangleF rectangleF = default(RectangleF);
			foreach (SvgElement child in Children)
			{
				if (!(child is SvgVisualElement))
				{
					continue;
				}
				if (rectangleF.IsEmpty)
				{
					rectangleF = ((SvgVisualElement)child).Bounds;
					continue;
				}
				RectangleF bounds = ((SvgVisualElement)child).Bounds;
				if (!bounds.IsEmpty)
				{
					rectangleF = RectangleF.Union(rectangleF, bounds);
				}
			}
			return TransformedBounds(rectangleF);
		}
	}

	protected override bool Renderable => false;

	public override GraphicsPath Path(ISvgRenderer renderer)
	{
		return GetPaths(this, renderer);
	}
}
