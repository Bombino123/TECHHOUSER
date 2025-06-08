using System.Drawing;
using System.Drawing.Drawing2D;

namespace AntdUI.Svg;

public class SvgGroup : SvgMarkerElement
{
	private bool markersSet;

	public override string ClassName => "g";

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

	private void AddMarkers()
	{
		if (markersSet)
		{
			return;
		}
		if (base.MarkerStart != null || base.MarkerMid != null || base.MarkerEnd != null)
		{
			foreach (SvgElement child in Children)
			{
				if (child is SvgMarkerElement)
				{
					if (base.MarkerStart != null && ((SvgMarkerElement)child).MarkerStart == null)
					{
						((SvgMarkerElement)child).MarkerStart = base.MarkerStart;
					}
					if (base.MarkerMid != null && ((SvgMarkerElement)child).MarkerMid == null)
					{
						((SvgMarkerElement)child).MarkerMid = base.MarkerMid;
					}
					if (base.MarkerEnd != null && ((SvgMarkerElement)child).MarkerEnd == null)
					{
						((SvgMarkerElement)child).MarkerEnd = base.MarkerEnd;
					}
				}
			}
		}
		markersSet = true;
	}

	protected override void Render(ISvgRenderer renderer)
	{
		AddMarkers();
		base.Render(renderer);
	}

	public override GraphicsPath Path(ISvgRenderer renderer)
	{
		return GetPaths(this, renderer);
	}
}
