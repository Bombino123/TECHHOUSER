using System.Drawing;
using System.Drawing.Drawing2D;

namespace AntdUI.Svg;

public class SvgSwitch : SvgVisualElement
{
	public override string ClassName => "switch";

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

	public override GraphicsPath Path(ISvgRenderer renderer)
	{
		return GetPaths(this, renderer);
	}

	protected override void Render(ISvgRenderer renderer)
	{
		if (Visible && Displayable)
		{
			PushTransforms(renderer);
			SetClip(renderer);
			base.RenderChildren(renderer);
			ResetClip(renderer);
			PopTransforms(renderer);
		}
	}
}
