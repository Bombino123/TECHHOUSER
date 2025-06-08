using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using AntdUI.Svg.ExtensionMethods;

namespace AntdUI.Svg;

public abstract class SvgMarkerElement : SvgPathBasedElement
{
	[SvgAttribute("marker-end", true)]
	public Uri MarkerEnd
	{
		get
		{
			return Attributes.GetAttribute<Uri>("marker-end").ReplaceWithNullIfNone();
		}
		set
		{
			Attributes["marker-end"] = value;
		}
	}

	[SvgAttribute("marker-mid", true)]
	public Uri MarkerMid
	{
		get
		{
			return Attributes.GetAttribute<Uri>("marker-mid").ReplaceWithNullIfNone();
		}
		set
		{
			Attributes["marker-mid"] = value;
		}
	}

	[SvgAttribute("marker-start", true)]
	public Uri MarkerStart
	{
		get
		{
			return Attributes.GetAttribute<Uri>("marker-start").ReplaceWithNullIfNone();
		}
		set
		{
			Attributes["marker-start"] = value;
		}
	}

	protected internal override bool RenderStroke(ISvgRenderer renderer)
	{
		bool result = base.RenderStroke(renderer);
		GraphicsPath val = Path(renderer);
		int num = val.PathPoints.Length;
		if (MarkerStart != null)
		{
			PointF pointF = val.PathPoints[0];
			int i;
			for (i = 1; i < num && val.PathPoints[i] == pointF; i++)
			{
			}
			PointF pMarkerPoint = val.PathPoints[i];
			OwnerDocument.GetElementById<SvgMarker>(MarkerStart.ToString()).RenderMarker(renderer, this, pointF, pointF, pMarkerPoint);
		}
		if (MarkerMid != null)
		{
			SvgMarker elementById = OwnerDocument.GetElementById<SvgMarker>(MarkerMid.ToString());
			int num2 = -1;
			for (int j = 1; j <= val.PathPoints.Length - 2; j++)
			{
				num2 = (((val.PathTypes[j] & 7) != 3) ? (-1) : ((num2 + 1) % 3));
				if (num2 == -1 || num2 == 2)
				{
					elementById.RenderMarker(renderer, this, val.PathPoints[j], val.PathPoints[j - 1], val.PathPoints[j], val.PathPoints[j + 1]);
				}
			}
		}
		if (MarkerEnd != null)
		{
			int num3 = num - 1;
			PointF pointF2 = val.PathPoints[num3];
			num3--;
			while (num3 > 0 && val.PathPoints[num3] == pointF2)
			{
				num3--;
			}
			PointF pMarkerPoint2 = val.PathPoints[num3];
			OwnerDocument.GetElementById<SvgMarker>(MarkerEnd.ToString()).RenderMarker(renderer, this, pointF2, pMarkerPoint2, val.PathPoints[val.PathPoints.Length - 1]);
		}
		return result;
	}
}
