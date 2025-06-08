using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace AntdUI.Svg;

public sealed class SvgRadialGradientServer : SvgGradientServer
{
	public override string ClassName => "radialGradient";

	[SvgAttribute("cx")]
	public SvgUnit CenterX
	{
		get
		{
			return Attributes.GetAttribute<SvgUnit>("cx");
		}
		set
		{
			Attributes["cx"] = value;
		}
	}

	[SvgAttribute("cy")]
	public SvgUnit CenterY
	{
		get
		{
			return Attributes.GetAttribute<SvgUnit>("cy");
		}
		set
		{
			Attributes["cy"] = value;
		}
	}

	[SvgAttribute("r")]
	public SvgUnit Radius
	{
		get
		{
			return Attributes.GetAttribute<SvgUnit>("r");
		}
		set
		{
			Attributes["r"] = value;
		}
	}

	[SvgAttribute("fx")]
	public SvgUnit FocalX
	{
		get
		{
			SvgUnit result = Attributes.GetAttribute<SvgUnit>("fx");
			if (result.IsEmpty || result.IsNone)
			{
				result = CenterX;
			}
			return result;
		}
		set
		{
			Attributes["fx"] = value;
		}
	}

	[SvgAttribute("fy")]
	public SvgUnit FocalY
	{
		get
		{
			SvgUnit result = Attributes.GetAttribute<SvgUnit>("fy");
			if (result.IsEmpty || result.IsNone)
			{
				result = CenterY;
			}
			return result;
		}
		set
		{
			Attributes["fy"] = value;
		}
	}

	public SvgRadialGradientServer()
	{
		CenterX = new SvgUnit(SvgUnitType.Percentage, 50f);
		CenterY = new SvgUnit(SvgUnitType.Percentage, 50f);
		Radius = new SvgUnit(SvgUnitType.Percentage, 50f);
	}

	private SvgUnit NormalizeUnit(SvgUnit orig)
	{
		if (orig.Type != SvgUnitType.Percentage || base.GradientUnits != SvgCoordinateUnits.ObjectBoundingBox)
		{
			return orig;
		}
		return new SvgUnit(SvgUnitType.User, orig.Value / 100f);
	}

	public override Brush GetBrush(SvgVisualElement renderingElement, ISvgRenderer renderer, float opacity, bool forStroke = false)
	{
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Expected O, but got Unknown
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Expected O, but got Unknown
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		//IL_030c: Expected O, but got Unknown
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Expected O, but got Unknown
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Expected O, but got Unknown
		LoadStops(renderingElement);
		try
		{
			if (base.GradientUnits == SvgCoordinateUnits.ObjectBoundingBox)
			{
				renderer.SetBoundable(renderingElement);
			}
			PointF pointF = new PointF(NormalizeUnit(CenterX).ToDeviceValue(renderer, UnitRenderingType.Horizontal, this), NormalizeUnit(CenterY).ToDeviceValue(renderer, UnitRenderingType.Vertical, this));
			PointF[] array = new PointF[1]
			{
				new PointF(NormalizeUnit(FocalX).ToDeviceValue(renderer, UnitRenderingType.Horizontal, this), NormalizeUnit(FocalY).ToDeviceValue(renderer, UnitRenderingType.Vertical, this))
			};
			float num = NormalizeUnit(Radius).ToDeviceValue(renderer, UnitRenderingType.Other, this);
			GraphicsPath val = new GraphicsPath();
			val.AddEllipse(pointF.X - num, pointF.Y - num, num * 2f, num * 2f);
			Matrix effectiveGradientTransform = base.EffectiveGradientTransform;
			try
			{
				RectangleF bounds = renderer.GetBoundable().Bounds;
				effectiveGradientTransform.Translate(bounds.X, bounds.Y, (MatrixOrder)0);
				if (base.GradientUnits == SvgCoordinateUnits.ObjectBoundingBox)
				{
					effectiveGradientTransform.Scale(bounds.Width, bounds.Height, (MatrixOrder)0);
				}
				val.Transform(effectiveGradientTransform);
				effectiveGradientTransform.TransformPoints(array);
			}
			finally
			{
				((IDisposable)effectiveGradientTransform)?.Dispose();
			}
			RectangleF bounds2 = RectangleF.Inflate(renderingElement.Bounds, renderingElement.StrokeWidth, renderingElement.StrokeWidth);
			float outScale = CalcScale(bounds2, val);
			if (outScale > 1f && base.SpreadMethod == SvgGradientSpreadMethod.Pad)
			{
				SvgGradientStop svgGradientStop = base.Stops.Last();
				Color color = svgGradientStop.GetColor(renderingElement);
				Color color2 = System.Drawing.Color.FromArgb((int)Math.Round(opacity * svgGradientStop.Opacity * 255f), color);
				Region clip = renderer.GetClip();
				try
				{
					SolidBrush val2 = new SolidBrush(color2);
					try
					{
						Region val3 = clip.Clone();
						val3.Exclude(val);
						renderer.SetClip(val3, (CombineMode)0);
						GraphicsPath path = renderingElement.Path(renderer);
						if (forStroke)
						{
							Pen val4 = new Pen((Brush)(object)val2, renderingElement.StrokeWidth.ToDeviceValue(renderer, UnitRenderingType.Other, renderingElement));
							try
							{
								renderer.DrawPath(val4, path);
							}
							finally
							{
								((IDisposable)val4)?.Dispose();
							}
						}
						else
						{
							renderer.FillPath((Brush)(object)val2, path);
						}
					}
					finally
					{
						((IDisposable)val2)?.Dispose();
					}
				}
				finally
				{
					renderer.SetClip(clip, (CombineMode)0);
				}
			}
			ColorBlend interpolationColors = CalculateColorBlend(renderer, opacity, outScale, out outScale);
			RectangleF bounds3 = val.GetBounds();
			PointF pointF2 = new PointF(bounds3.Left + bounds3.Width / 2f, bounds3.Top + bounds3.Height / 2f);
			Matrix val5 = new Matrix();
			try
			{
				val5.Translate(-1f * pointF2.X, -1f * pointF2.Y, (MatrixOrder)1);
				val5.Scale(outScale, outScale, (MatrixOrder)1);
				val5.Translate(pointF2.X, pointF2.Y, (MatrixOrder)1);
				val.Transform(val5);
			}
			finally
			{
				((IDisposable)val5)?.Dispose();
			}
			return (Brush)new PathGradientBrush(val)
			{
				CenterPoint = array[0],
				InterpolationColors = interpolationColors
			};
		}
		finally
		{
			if (base.GradientUnits == SvgCoordinateUnits.ObjectBoundingBox)
			{
				renderer.PopBoundable();
			}
		}
	}

	private float CalcScale(RectangleF bounds, GraphicsPath path, Graphics graphics = null)
	{
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Expected O, but got Unknown
		PointF[] array = new PointF[4]
		{
			new PointF(bounds.Left, bounds.Top),
			new PointF(bounds.Right, bounds.Top),
			new PointF(bounds.Right, bounds.Bottom),
			new PointF(bounds.Left, bounds.Bottom)
		};
		RectangleF bounds2 = path.GetBounds();
		PointF pointF = new PointF(bounds2.X + bounds2.Width / 2f, bounds2.Y + bounds2.Height / 2f);
		Matrix val = new Matrix();
		try
		{
			val.Translate(-1f * pointF.X, -1f * pointF.Y, (MatrixOrder)1);
			val.Scale(0.95f, 0.95f, (MatrixOrder)1);
			val.Translate(pointF.X, pointF.Y, (MatrixOrder)1);
			while (!path.IsVisible(array[0]) || !path.IsVisible(array[1]) || !path.IsVisible(array[2]) || !path.IsVisible(array[3]))
			{
				PointF[] first = new PointF[4]
				{
					new PointF(array[0].X, array[0].Y),
					new PointF(array[1].X, array[1].Y),
					new PointF(array[2].X, array[2].Y),
					new PointF(array[3].X, array[3].Y)
				};
				val.TransformPoints(array);
				if (first.SequenceEqual(array))
				{
					break;
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		return bounds.Height / (array[2].Y - array[1].Y);
	}

	private static IEnumerable<GraphicsPath> GetDifference(RectangleF subject, GraphicsPath clip)
	{
		GraphicsPath val = (GraphicsPath)clip.Clone();
		val.Flatten();
		RectangleF bounds = val.GetBounds();
		RectangleF rectangleF = RectangleF.Union(subject, bounds);
		rectangleF.Inflate(rectangleF.Width * 0.3f, rectangleF.Height * 0.3f);
		PointF pointF = new PointF((bounds.Left + bounds.Right) / 2f, (bounds.Top + bounds.Bottom) / 2f);
		List<PointF> list = new List<PointF>();
		List<PointF> rightPoints = new List<PointF>();
		PointF[] pathPoints = val.PathPoints;
		for (int i = 0; i < pathPoints.Length; i++)
		{
			PointF item = pathPoints[i];
			if (item.X <= pointF.X)
			{
				list.Add(item);
			}
			else
			{
				rightPoints.Add(item);
			}
		}
		list.Sort((PointF p, PointF q) => p.Y.CompareTo(q.Y));
		rightPoints.Sort((PointF p, PointF q) => p.Y.CompareTo(q.Y));
		PointF item2 = new PointF((list.Last().X + rightPoints.Last().X) / 2f, (list.Last().Y + rightPoints.Last().Y) / 2f);
		list.Add(item2);
		rightPoints.Add(item2);
		item2 = new PointF(item2.X, rectangleF.Bottom);
		list.Add(item2);
		rightPoints.Add(item2);
		list.Add(new PointF(rectangleF.Left, rectangleF.Bottom));
		list.Add(new PointF(rectangleF.Left, rectangleF.Top));
		rightPoints.Add(new PointF(rectangleF.Right, rectangleF.Bottom));
		rightPoints.Add(new PointF(rectangleF.Right, rectangleF.Top));
		item2 = new PointF((list.First().X + rightPoints.First().X) / 2f, rectangleF.Top);
		list.Add(item2);
		rightPoints.Add(item2);
		item2 = new PointF(item2.X, (list.First().Y + rightPoints.First().Y) / 2f);
		list.Add(item2);
		rightPoints.Add(item2);
		GraphicsPath path = new GraphicsPath((FillMode)1);
		path.AddPolygon(list.ToArray());
		yield return path;
		path.Reset();
		path.AddPolygon(rightPoints.ToArray());
		yield return path;
	}

	private static GraphicsPath CreateGraphicsPath(PointF origin, PointF centerPoint, float effectiveRadius)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		GraphicsPath val = new GraphicsPath();
		val.AddEllipse(origin.X + centerPoint.X - effectiveRadius, origin.Y + centerPoint.Y - effectiveRadius, effectiveRadius * 2f, effectiveRadius * 2f);
		return val;
	}

	private ColorBlend CalculateColorBlend(ISvgRenderer renderer, float opacity, float scale, out float outScale)
	{
		ColorBlend colorBlend = GetColorBlend(renderer, opacity, radial: true);
		outScale = scale;
		if (scale > 1f)
		{
			float newScale;
			switch (base.SpreadMethod)
			{
			case SvgGradientSpreadMethod.Reflect:
			{
				newScale = (float)Math.Ceiling(scale);
				List<float> list = colorBlend.Positions.Select((float p) => 1f + (p - 1f) / newScale).ToList();
				List<Color> list2 = colorBlend.Colors.ToList();
				for (int j = 1; (float)j < newScale; j++)
				{
					if (j % 2 == 1)
					{
						for (int k = 1; k < colorBlend.Positions.Length; k++)
						{
							list.Insert(0, (newScale - (float)j - 1f) / newScale + 1f - colorBlend.Positions[k]);
							list2.Insert(0, colorBlend.Colors[k]);
						}
					}
					else
					{
						for (int l = 0; l < colorBlend.Positions.Length - 1; l++)
						{
							list.Insert(l, (newScale - (float)j - 1f) / newScale + colorBlend.Positions[l]);
							list2.Insert(l, colorBlend.Colors[l]);
						}
					}
				}
				colorBlend.Positions = list.ToArray();
				colorBlend.Colors = list2.ToArray();
				outScale = newScale;
				break;
			}
			case SvgGradientSpreadMethod.Repeat:
			{
				newScale = (float)Math.Ceiling(scale);
				List<float> list = colorBlend.Positions.Select((float p) => p / newScale).ToList();
				List<Color> list2 = colorBlend.Colors.ToList();
				int i;
				for (i = 1; (float)i < newScale; i++)
				{
					list.AddRange(colorBlend.Positions.Select((float p) => ((float)i + ((p <= 0f) ? 0.001f : p)) / newScale));
					list2.AddRange(colorBlend.Colors);
				}
				colorBlend.Positions = list.ToArray();
				colorBlend.Colors = list2.ToArray();
				outScale = newScale;
				break;
			}
			default:
				outScale = 1f;
				break;
			}
		}
		return colorBlend;
	}
}
