using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using AntdUI.Svg.DataTypes;
using AntdUI.Svg.Transforms;

namespace AntdUI.Svg;

public class SvgMarker : SvgPathBasedElement, ISvgViewPort
{
	private SvgOrient _svgOrient = new SvgOrient();

	private SvgVisualElement _markerElement;

	public override string ClassName => "marker";

	private SvgVisualElement MarkerElement
	{
		get
		{
			if (_markerElement == null)
			{
				_markerElement = (SvgVisualElement)Children.FirstOrDefault((SvgElement x) => x is SvgVisualElement);
			}
			return _markerElement;
		}
	}

	[SvgAttribute("refX")]
	public virtual SvgUnit RefX
	{
		get
		{
			return Attributes.GetAttribute<SvgUnit>("refX");
		}
		set
		{
			Attributes["refX"] = value;
		}
	}

	[SvgAttribute("refY")]
	public virtual SvgUnit RefY
	{
		get
		{
			return Attributes.GetAttribute<SvgUnit>("refY");
		}
		set
		{
			Attributes["refY"] = value;
		}
	}

	[SvgAttribute("orient")]
	public virtual SvgOrient Orient
	{
		get
		{
			return _svgOrient;
		}
		set
		{
			_svgOrient = value;
		}
	}

	[SvgAttribute("overflow")]
	public virtual SvgOverflow Overflow
	{
		get
		{
			return Attributes.GetAttribute<SvgOverflow>("overflow");
		}
		set
		{
			Attributes["overflow"] = value;
		}
	}

	[SvgAttribute("viewBox")]
	public virtual SvgViewBox ViewBox
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
	public virtual SvgAspectRatio AspectRatio
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

	[SvgAttribute("markerWidth")]
	public virtual SvgUnit MarkerWidth
	{
		get
		{
			return Attributes.GetAttribute<SvgUnit>("markerWidth");
		}
		set
		{
			Attributes["markerWidth"] = value;
		}
	}

	[SvgAttribute("markerHeight")]
	public virtual SvgUnit MarkerHeight
	{
		get
		{
			return Attributes.GetAttribute<SvgUnit>("markerHeight");
		}
		set
		{
			Attributes["markerHeight"] = value;
		}
	}

	[SvgAttribute("markerUnits")]
	public virtual SvgMarkerUnits MarkerUnits
	{
		get
		{
			return Attributes.GetAttribute<SvgMarkerUnits>("markerUnits");
		}
		set
		{
			Attributes["markerUnits"] = value;
		}
	}

	public override SvgPaintServer Fill
	{
		get
		{
			if (MarkerElement != null)
			{
				return MarkerElement.Fill;
			}
			return base.Fill;
		}
	}

	public override SvgPaintServer Stroke
	{
		get
		{
			if (MarkerElement != null)
			{
				return MarkerElement.Stroke;
			}
			return base.Stroke;
		}
	}

	public SvgMarker()
	{
		MarkerUnits = SvgMarkerUnits.StrokeWidth;
		MarkerHeight = 3f;
		MarkerWidth = 3f;
		Overflow = SvgOverflow.Hidden;
	}

	public override GraphicsPath Path(ISvgRenderer renderer)
	{
		if (MarkerElement != null)
		{
			return MarkerElement.Path(renderer);
		}
		return null;
	}

	public void RenderMarker(ISvgRenderer pRenderer, SvgVisualElement pOwner, PointF pRefPoint, PointF pMarkerPoint1, PointF pMarkerPoint2)
	{
		float fAngle = 0f;
		if (Orient.IsAuto)
		{
			float num = pMarkerPoint2.X - pMarkerPoint1.X;
			fAngle = (float)(Math.Atan2(pMarkerPoint2.Y - pMarkerPoint1.Y, num) * 180.0 / Math.PI);
		}
		RenderPart2(fAngle, pRenderer, pOwner, pRefPoint);
	}

	public void RenderMarker(ISvgRenderer pRenderer, SvgVisualElement pOwner, PointF pRefPoint, PointF pMarkerPoint1, PointF pMarkerPoint2, PointF pMarkerPoint3)
	{
		float num = pMarkerPoint2.X - pMarkerPoint1.X;
		float num2 = (float)(Math.Atan2(pMarkerPoint2.Y - pMarkerPoint1.Y, num) * 180.0 / Math.PI);
		num = pMarkerPoint3.X - pMarkerPoint2.X;
		float num3 = (float)(Math.Atan2(pMarkerPoint3.Y - pMarkerPoint2.Y, num) * 180.0 / Math.PI);
		RenderPart2((num2 + num3) / 2f, pRenderer, pOwner, pRefPoint);
	}

	private void RenderPart2(float fAngle, ISvgRenderer pRenderer, SvgVisualElement pOwner, PointF pMarkerPoint)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		Pen val = CreatePen(pOwner, pRenderer);
		try
		{
			GraphicsPath clone = GetClone(pOwner);
			try
			{
				Matrix val2 = new Matrix();
				try
				{
					val2.Translate(pMarkerPoint.X, pMarkerPoint.Y);
					if (Orient.IsAuto)
					{
						val2.Rotate(fAngle);
					}
					else
					{
						val2.Rotate(Orient.Angle);
					}
					switch (MarkerUnits)
					{
					case SvgMarkerUnits.StrokeWidth:
						if (ViewBox.Width > 0f && ViewBox.Height > 0f)
						{
							val2.Scale((float)MarkerWidth, (float)MarkerHeight);
							float num = pOwner.StrokeWidth.ToDeviceValue(pRenderer, UnitRenderingType.Other, this);
							val2.Translate(AdjustForViewBoxWidth((0f - RefX.ToDeviceValue(pRenderer, UnitRenderingType.Horizontal, this)) * num), AdjustForViewBoxHeight((0f - RefY.ToDeviceValue(pRenderer, UnitRenderingType.Vertical, this)) * num));
						}
						else
						{
							val2.Translate(0f - RefX.ToDeviceValue(pRenderer, UnitRenderingType.Horizontal, this), 0f - RefY.ToDeviceValue(pRenderer, UnitRenderingType.Vertical, this));
						}
						break;
					case SvgMarkerUnits.UserSpaceOnUse:
						val2.Translate(0f - RefX.ToDeviceValue(pRenderer, UnitRenderingType.Horizontal, this), 0f - RefY.ToDeviceValue(pRenderer, UnitRenderingType.Vertical, this));
						break;
					}
					if (MarkerElement != null && MarkerElement.Transforms != null)
					{
						foreach (SvgTransform transform in MarkerElement.Transforms)
						{
							val2.Multiply(transform.Matrix(0f, 0f));
						}
					}
					clone.Transform(val2);
					if (val != null)
					{
						pRenderer.DrawPath(val, clone);
					}
					SvgPaintServer fill = Children.First().Fill;
					_ = FillRule;
					float fillOpacity = FillOpacity;
					if (fill != null)
					{
						Brush brush = fill.GetBrush(this, pRenderer, fillOpacity);
						try
						{
							pRenderer.FillPath(brush, clone);
							return;
						}
						finally
						{
							((IDisposable)brush)?.Dispose();
						}
					}
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)clone)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private Pen CreatePen(SvgVisualElement pPath, ISvgRenderer renderer)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		if (Stroke == null)
		{
			return null;
		}
		Brush brush = Stroke.GetBrush(this, renderer, Opacity);
		return (Pen)(MarkerUnits switch
		{
			SvgMarkerUnits.StrokeWidth => (object)new Pen(brush, pPath.StrokeWidth.ToDeviceValue(renderer, UnitRenderingType.Other, this)), 
			SvgMarkerUnits.UserSpaceOnUse => (object)new Pen(brush, StrokeWidth.ToDeviceValue(renderer, UnitRenderingType.Other, this)), 
			_ => (object)new Pen(brush, StrokeWidth.ToDeviceValue(renderer, UnitRenderingType.Other, this)), 
		});
	}

	private GraphicsPath GetClone(SvgVisualElement pPath)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		object obj = Path(null).Clone();
		GraphicsPath val = (GraphicsPath)((obj is GraphicsPath) ? obj : null);
		switch (MarkerUnits)
		{
		case SvgMarkerUnits.StrokeWidth:
		{
			Matrix val2 = new Matrix();
			try
			{
				val2.Scale(AdjustForViewBoxWidth(pPath.StrokeWidth), AdjustForViewBoxHeight(pPath.StrokeWidth));
				val.Transform(val2);
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
			break;
		}
		}
		return val;
	}

	private float AdjustForViewBoxWidth(float fWidth)
	{
		if (!(ViewBox.Width <= 0f))
		{
			return fWidth / ViewBox.Width;
		}
		return 1f;
	}

	private float AdjustForViewBoxHeight(float fHeight)
	{
		if (!(ViewBox.Height <= 0f))
		{
			return fHeight / ViewBox.Height;
		}
		return 1f;
	}
}
