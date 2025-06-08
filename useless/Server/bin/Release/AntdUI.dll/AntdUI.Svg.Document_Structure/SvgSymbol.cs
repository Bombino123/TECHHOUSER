using System.Drawing;
using System.Drawing.Drawing2D;

namespace AntdUI.Svg.Document_Structure;

public class SvgSymbol : SvgVisualElement
{
	public override string ClassName => "symbol";

	[SvgAttribute("viewBox")]
	public SvgViewBox ViewBox
	{
		get
		{
			return Attributes.GetAttribute<SvgViewBox>("viewBox");
		}
		set
		{
			Attributes["viewBox"] = value;
		}
	}

	[SvgAttribute("preserveAspectRatio")]
	public SvgAspectRatio AspectRatio
	{
		get
		{
			return Attributes.GetAttribute<SvgAspectRatio>("preserveAspectRatio");
		}
		set
		{
			Attributes["preserveAspectRatio"] = value;
		}
	}

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

	protected internal override bool PushTransforms(ISvgRenderer renderer)
	{
		if (!base.PushTransforms(renderer))
		{
			return false;
		}
		ViewBox.AddViewBoxTransform(AspectRatio, renderer, null);
		return true;
	}

	protected override void Render(ISvgRenderer renderer)
	{
		if (_parent is SvgUse)
		{
			base.Render(renderer);
		}
	}
}
