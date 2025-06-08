using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace AntdUI.Svg;

public sealed class SvgLinearGradientServer : SvgGradientServer
{
	[Flags]
	private enum LinePoints
	{
		None = 0,
		Start = 1,
		End = 2
	}

	public struct GradientPoints
	{
		public PointF StartPoint;

		public PointF EndPoint;

		public GradientPoints(PointF startPoint, PointF endPoint)
		{
			StartPoint = startPoint;
			EndPoint = endPoint;
		}
	}

	private sealed class LineF
	{
		private float X1 { get; set; }

		private float Y1 { get; set; }

		private float X2 { get; set; }

		private float Y2 { get; set; }

		public LineF(float x1, float y1, float x2, float y2)
		{
			X1 = x1;
			Y1 = y1;
			X2 = x2;
			Y2 = y2;
		}

		public List<PointF> Intersection(RectangleF rectangle)
		{
			List<PointF> result = new List<PointF>();
			AddIfIntersect(this, new LineF(rectangle.X, rectangle.Y, rectangle.Right, rectangle.Y), result);
			AddIfIntersect(this, new LineF(rectangle.Right, rectangle.Y, rectangle.Right, rectangle.Bottom), result);
			AddIfIntersect(this, new LineF(rectangle.Right, rectangle.Bottom, rectangle.X, rectangle.Bottom), result);
			AddIfIntersect(this, new LineF(rectangle.X, rectangle.Bottom, rectangle.X, rectangle.Y), result);
			return result;
		}

		private PointF? Intersection(LineF other)
		{
			double num = (double)Y2 - (double)Y1;
			double num2 = (double)X1 - (double)X2;
			double num3 = num * (double)X1 + num2 * (double)Y1;
			double num4 = (double)other.Y2 - (double)other.Y1;
			double num5 = (double)other.X1 - (double)other.X2;
			double num6 = num4 * (double)other.X1 + num5 * (double)other.Y1;
			double num7 = num * num5 - num4 * num2;
			if (num7 == 0.0)
			{
				return null;
			}
			double num8 = (num5 * num3 - num2 * num6) / num7;
			double num9 = (num * num6 - num4 * num3) / num7;
			if (Math.Round(Math.Min(X1, X2), 8) <= Math.Round(num8, 8) && Math.Round(num8, 8) <= Math.Round(Math.Max(X1, X2), 8) && Math.Round(Math.Min(Y1, Y2), 8) <= Math.Round(num9, 8) && Math.Round(num9, 8) <= Math.Round(Math.Max(Y1, Y2), 8) && Math.Round(Math.Min(other.X1, other.X2), 8) <= Math.Round(num8, 8) && Math.Round(num8, 8) <= Math.Round(Math.Max(other.X1, other.X2), 8) && Math.Round(Math.Min(other.Y1, other.Y2), 8) <= Math.Round(num9, 8) && Math.Round(num9, 8) <= Math.Round(Math.Max(other.Y1, other.Y2), 8))
			{
				return new PointF((float)num8, (float)num9);
			}
			return null;
		}

		private static void AddIfIntersect(LineF first, LineF second, ICollection<PointF> result)
		{
			PointF? pointF = first.Intersection(second);
			if (pointF.HasValue)
			{
				result.Add(pointF.Value);
			}
		}
	}

	public override string ClassName => "linearGradient";

	[SvgAttribute("x1")]
	public SvgUnit X1
	{
		get
		{
			return Attributes.GetAttribute<SvgUnit>("x1");
		}
		set
		{
			Attributes["x1"] = value;
		}
	}

	[SvgAttribute("y1")]
	public SvgUnit Y1
	{
		get
		{
			return Attributes.GetAttribute<SvgUnit>("y1");
		}
		set
		{
			Attributes["y1"] = value;
		}
	}

	[SvgAttribute("x2")]
	public SvgUnit X2
	{
		get
		{
			return Attributes.GetAttribute<SvgUnit>("x2");
		}
		set
		{
			Attributes["x2"] = value;
		}
	}

	[SvgAttribute("y2")]
	public SvgUnit Y2
	{
		get
		{
			return Attributes.GetAttribute<SvgUnit>("y2");
		}
		set
		{
			Attributes["y2"] = value;
		}
	}

	private bool IsInvalid => base.Stops.Count < 2;

	public SvgLinearGradientServer()
	{
		X1 = new SvgUnit(SvgUnitType.Percentage, 0f);
		Y1 = new SvgUnit(SvgUnitType.Percentage, 0f);
		X2 = new SvgUnit(SvgUnitType.Percentage, 100f);
		Y2 = new SvgUnit(SvgUnitType.Percentage, 0f);
	}

	public override Brush GetBrush(SvgVisualElement renderingElement, ISvgRenderer renderer, float opacity, bool forStroke = false)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected O, but got Unknown
		//IL_04a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d4: Expected O, but got Unknown
		LoadStops(renderingElement);
		if (base.Stops.Count < 1)
		{
			return null;
		}
		if (base.Stops.Count == 1)
		{
			Color color = base.Stops[0].GetColor(renderingElement);
			return (Brush)new SolidBrush(System.Drawing.Color.FromArgb((int)Math.Round(opacity * ((float)(int)color.A / 255f) * 255f), color));
		}
		try
		{
			if (base.GradientUnits == SvgCoordinateUnits.ObjectBoundingBox)
			{
				renderer.SetBoundable(renderingElement);
			}
			PointF[] array = new PointF[2]
			{
				SvgUnit.GetDevicePoint(NormalizeUnit(X1), NormalizeUnit(Y1), renderer, this),
				SvgUnit.GetDevicePoint(NormalizeUnit(X2), NormalizeUnit(Y2), renderer, this)
			};
			RectangleF bounds = renderer.GetBoundable().Bounds;
			if (bounds.Width <= 0f || bounds.Height <= 0f || (array[0].X == array[1].X && array[0].Y == array[1].Y))
			{
				if (base.GetCallback != null)
				{
					return base.GetCallback().GetBrush(renderingElement, renderer, opacity, forStroke);
				}
				return null;
			}
			Matrix effectiveGradientTransform = base.EffectiveGradientTransform;
			try
			{
				PointF pointF = new PointF((array[0].X + array[1].X) / 2f, (array[0].Y + array[1].Y) / 2f);
				effectiveGradientTransform.Translate(bounds.X, bounds.Y, (MatrixOrder)0);
				if (base.GradientUnits == SvgCoordinateUnits.ObjectBoundingBox)
				{
					effectiveGradientTransform.Scale(bounds.Width, bounds.Height, (MatrixOrder)0);
					effectiveGradientTransform.RotateAt(-90f, pointF, (MatrixOrder)0);
				}
				effectiveGradientTransform.TransformPoints(array);
			}
			finally
			{
				((IDisposable)effectiveGradientTransform)?.Dispose();
			}
			if (base.GradientUnits == SvgCoordinateUnits.ObjectBoundingBox)
			{
				PointF pointF2 = new PointF((array[0].X + array[1].X) / 2f, (array[0].Y + array[1].Y) / 2f);
				float num = array[1].Y - array[0].Y;
				float num2 = array[1].X - array[0].X;
				float x = array[0].X;
				float y = array[1].Y;
				if (Math.Round(num2, 4) == 0.0)
				{
					array[0] = new PointF(pointF2.X + num / 2f * bounds.Width / bounds.Height, pointF2.Y);
					array[1] = new PointF(pointF2.X - num / 2f * bounds.Width / bounds.Height, pointF2.Y);
				}
				else if (Math.Round(num, 4) == 0.0)
				{
					array[0] = new PointF(pointF2.X, pointF2.Y - num2 / 2f * bounds.Height / bounds.Width);
					array[1] = new PointF(pointF2.X, pointF2.Y + num2 / 2f * bounds.Height / bounds.Width);
				}
				else
				{
					float num3 = (float)(((double)(num * num2 * (pointF2.Y - y)) + Math.Pow(num2, 2.0) * (double)pointF2.X + Math.Pow(num, 2.0) * (double)x) / (Math.Pow(num2, 2.0) + Math.Pow(num, 2.0)));
					float num4 = num * (num3 - x) / num2 + y;
					array[0] = new PointF(num3, num4);
					array[1] = new PointF(pointF2.X + (pointF2.X - num3), pointF2.Y + (pointF2.Y - num4));
				}
			}
			PointF pointF3 = array[0];
			PointF pointF4 = array[1];
			if (PointsToMove(renderingElement, array[0], array[1]) > LinePoints.None)
			{
				GradientPoints gradientPoints = ExpandGradient(renderingElement, array[0], array[1]);
				pointF3 = gradientPoints.StartPoint;
				pointF4 = gradientPoints.EndPoint;
			}
			return (Brush)new LinearGradientBrush(pointF3, pointF4, System.Drawing.Color.Transparent, System.Drawing.Color.Transparent)
			{
				InterpolationColors = CalculateColorBlend(renderer, opacity, array[0], pointF3, array[1], pointF4),
				WrapMode = (WrapMode)1
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

	private SvgUnit NormalizeUnit(SvgUnit orig)
	{
		if (orig.Type != SvgUnitType.Percentage || base.GradientUnits != SvgCoordinateUnits.ObjectBoundingBox)
		{
			return orig;
		}
		return new SvgUnit(SvgUnitType.User, orig.Value / 100f);
	}

	private LinePoints PointsToMove(ISvgBoundable boundable, PointF specifiedStart, PointF specifiedEnd)
	{
		RectangleF bounds = boundable.Bounds;
		if (specifiedStart.X == specifiedEnd.X)
		{
			return ((bounds.Top < specifiedStart.Y && specifiedStart.Y < bounds.Bottom) ? LinePoints.Start : LinePoints.None) | ((bounds.Top < specifiedEnd.Y && specifiedEnd.Y < bounds.Bottom) ? LinePoints.End : LinePoints.None);
		}
		if (specifiedStart.Y == specifiedEnd.Y)
		{
			return ((bounds.Left < specifiedStart.X && specifiedStart.X < bounds.Right) ? LinePoints.Start : LinePoints.None) | ((bounds.Left < specifiedEnd.X && specifiedEnd.X < bounds.Right) ? LinePoints.End : LinePoints.None);
		}
		return (boundable.Bounds.Contains(specifiedStart) ? LinePoints.Start : LinePoints.None) | (boundable.Bounds.Contains(specifiedEnd) ? LinePoints.End : LinePoints.None);
	}

	private GradientPoints ExpandGradient(ISvgBoundable boundable, PointF specifiedStart, PointF specifiedEnd)
	{
		LinePoints linePoints = PointsToMove(boundable, specifiedStart, specifiedEnd);
		if (linePoints == LinePoints.None)
		{
			return new GradientPoints(specifiedStart, specifiedEnd);
		}
		RectangleF bounds = boundable.Bounds;
		PointF pointF = specifiedStart;
		PointF pointF2 = specifiedEnd;
		IList<PointF> list = CandidateIntersections(bounds, specifiedStart, specifiedEnd);
		if (Math.Sign(list[1].X - list[0].X) != Math.Sign(specifiedEnd.X - specifiedStart.X) || Math.Sign(list[1].Y - list[0].Y) != Math.Sign(specifiedEnd.Y - specifiedStart.Y))
		{
			list = list.Reverse().ToList();
		}
		if ((linePoints & LinePoints.Start) > LinePoints.None)
		{
			pointF = list[0];
		}
		if ((linePoints & LinePoints.End) > LinePoints.None)
		{
			pointF2 = list[1];
		}
		SvgGradientSpreadMethod spreadMethod = base.SpreadMethod;
		if ((uint)(spreadMethod - 1) <= 1u)
		{
			double num = SvgGradientServer.CalculateDistance(specifiedStart, specifiedEnd);
			PointF unitVector = new PointF((specifiedEnd.X - specifiedStart.X) / (float)num, (specifiedEnd.Y - specifiedStart.Y) / (float)num);
			PointF unitVector2 = new PointF(0f - unitVector.X, 0f - unitVector.Y);
			float distance = (float)(Math.Ceiling(SvgGradientServer.CalculateDistance(pointF, specifiedStart) / num) * num);
			pointF = MovePointAlongVector(specifiedStart, unitVector2, distance);
			float distance2 = (float)(Math.Ceiling(SvgGradientServer.CalculateDistance(pointF2, specifiedEnd) / num) * num);
			pointF2 = MovePointAlongVector(specifiedEnd, unitVector, distance2);
		}
		return new GradientPoints(pointF, pointF2);
	}

	private IList<PointF> CandidateIntersections(RectangleF bounds, PointF p1, PointF p2)
	{
		List<PointF> list = new List<PointF>();
		if (Math.Round(Math.Abs(p1.Y - p2.Y), 4) == 0.0)
		{
			list.Add(new PointF(bounds.Left, p1.Y));
			list.Add(new PointF(bounds.Right, p1.Y));
		}
		else if (Math.Round(Math.Abs(p1.X - p2.X), 4) == 0.0)
		{
			list.Add(new PointF(p1.X, bounds.Top));
			list.Add(new PointF(p1.X, bounds.Bottom));
		}
		else
		{
			if ((p1.X == bounds.Left || p1.X == bounds.Right) && (p1.Y == bounds.Top || p1.Y == bounds.Bottom))
			{
				list.Add(p1);
			}
			else
			{
				PointF item = new PointF(bounds.Left, (p2.Y - p1.Y) / (p2.X - p1.X) * (bounds.Left - p1.X) + p1.Y);
				if (bounds.Top <= item.Y && item.Y <= bounds.Bottom)
				{
					list.Add(item);
				}
				item = new PointF(bounds.Right, (p2.Y - p1.Y) / (p2.X - p1.X) * (bounds.Right - p1.X) + p1.Y);
				if (bounds.Top <= item.Y && item.Y <= bounds.Bottom)
				{
					list.Add(item);
				}
			}
			if ((p2.X == bounds.Left || p2.X == bounds.Right) && (p2.Y == bounds.Top || p2.Y == bounds.Bottom))
			{
				list.Add(p2);
			}
			else
			{
				PointF item = new PointF((bounds.Top - p1.Y) / (p2.Y - p1.Y) * (p2.X - p1.X) + p1.X, bounds.Top);
				if (bounds.Left <= item.X && item.X <= bounds.Right)
				{
					list.Add(item);
				}
				item = new PointF((bounds.Bottom - p1.Y) / (p2.Y - p1.Y) * (p2.X - p1.X) + p1.X, bounds.Bottom);
				if (bounds.Left <= item.X && item.X <= bounds.Right)
				{
					list.Add(item);
				}
			}
		}
		return list;
	}

	private ColorBlend CalculateColorBlend(ISvgRenderer renderer, float opacity, PointF specifiedStart, PointF effectiveStart, PointF specifiedEnd, PointF effectiveEnd)
	{
		ColorBlend colorBlend = GetColorBlend(renderer, opacity, radial: false);
		double num = SvgGradientServer.CalculateDistance(specifiedStart, effectiveStart);
		double num2 = SvgGradientServer.CalculateDistance(specifiedEnd, effectiveEnd);
		if (!(num > 0.0) && !(num2 > 0.0))
		{
			return colorBlend;
		}
		double num3 = SvgGradientServer.CalculateDistance(specifiedStart, specifiedEnd);
		PointF unitVector = new PointF((specifiedEnd.X - specifiedStart.X) / (float)num3, (specifiedEnd.Y - specifiedStart.Y) / (float)num3);
		double num4 = SvgGradientServer.CalculateDistance(effectiveStart, effectiveEnd);
		float startExtend;
		float endExtend;
		switch (base.SpreadMethod)
		{
		case SvgGradientSpreadMethod.Reflect:
		{
			startExtend = (float)Math.Ceiling(SvgGradientServer.CalculateDistance(effectiveStart, specifiedStart) / num3);
			endExtend = (float)Math.Ceiling(SvgGradientServer.CalculateDistance(effectiveEnd, specifiedEnd) / num3);
			List<Color> list = colorBlend.Colors.ToList();
			List<float> list2 = colorBlend.Positions.Select((float p) => p + startExtend).ToList();
			for (int l = 0; (float)l < startExtend; l++)
			{
				if (l % 2 == 0)
				{
					for (int m = 1; m < colorBlend.Positions.Length; m++)
					{
						list2.Insert(0, startExtend - 1f - (float)l + 1f - colorBlend.Positions[m]);
						list.Insert(0, colorBlend.Colors[m]);
					}
				}
				else
				{
					for (int n = 0; n < colorBlend.Positions.Length - 1; n++)
					{
						list2.Insert(n, startExtend - 1f - (float)l + colorBlend.Positions[n]);
						list.Insert(n, colorBlend.Colors[n]);
					}
				}
			}
			for (int num6 = 0; (float)num6 < endExtend; num6++)
			{
				if (num6 % 2 == 0)
				{
					int count = list2.Count;
					for (int num7 = 0; num7 < colorBlend.Positions.Length - 1; num7++)
					{
						list2.Insert(count, startExtend + 1f + (float)num6 + 1f - colorBlend.Positions[num7]);
						list.Insert(count, colorBlend.Colors[num7]);
					}
				}
				else
				{
					for (int num8 = 1; num8 < colorBlend.Positions.Length; num8++)
					{
						list2.Add(startExtend + 1f + (float)num6 + colorBlend.Positions[num8]);
						list.Add(colorBlend.Colors[num8]);
					}
				}
			}
			colorBlend.Colors = list.ToArray();
			colorBlend.Positions = list2.Select((float p) => p / (startExtend + 1f + endExtend)).ToArray();
			break;
		}
		case SvgGradientSpreadMethod.Repeat:
		{
			startExtend = (float)Math.Ceiling(SvgGradientServer.CalculateDistance(effectiveStart, specifiedStart) / num3);
			endExtend = (float)Math.Ceiling(SvgGradientServer.CalculateDistance(effectiveEnd, specifiedEnd) / num3);
			List<Color> list = new List<Color>();
			List<float> list2 = new List<float>();
			for (int j = 0; (float)j < startExtend + endExtend + 1f; j++)
			{
				for (int k = 0; k < colorBlend.Positions.Length; k++)
				{
					list2.Add(((float)j + colorBlend.Positions[k] * 0.9999f) / (startExtend + endExtend + 1f));
					list.Add(colorBlend.Colors[k]);
				}
			}
			list2[list2.Count - 1] = 1f;
			colorBlend.Colors = list.ToArray();
			colorBlend.Positions = list2.ToArray();
			break;
		}
		default:
		{
			for (int i = 0; i < colorBlend.Positions.Length; i++)
			{
				PointF second = MovePointAlongVector(specifiedStart, unitVector, (float)num3 * colorBlend.Positions[i]);
				double num5 = SvgGradientServer.CalculateDistance(effectiveStart, second);
				colorBlend.Positions[i] = (float)Math.Round(Math.Max(0.0, Math.Min(num5 / num4, 1.0)), 5);
			}
			if (num > 0.0)
			{
				colorBlend.Positions = new float[1].Concat(colorBlend.Positions).ToArray();
				colorBlend.Colors = new Color[1] { colorBlend.Colors.First() }.Concat(colorBlend.Colors).ToArray();
			}
			if (num2 > 0.0)
			{
				colorBlend.Positions = colorBlend.Positions.Concat(new float[1] { 1f }).ToArray();
				colorBlend.Colors = colorBlend.Colors.Concat(new Color[1] { colorBlend.Colors.Last() }).ToArray();
			}
			break;
		}
		}
		return colorBlend;
	}

	private static PointF MovePointAlongVector(PointF start, PointF unitVector, float distance)
	{
		return start + new SizeF(unitVector.X * distance, unitVector.Y * distance);
	}
}
