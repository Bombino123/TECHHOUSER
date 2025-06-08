using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using AntdUI.Svg.Transforms;

namespace AntdUI.Svg;

public abstract class SvgGradientServer : SvgPaintServer, ISvgSupportsCoordinateUnits
{
	private SvgCoordinateUnits _gradientUnits;

	private SvgGradientSpreadMethod _spreadMethod;

	private SvgPaintServer _inheritGradient;

	private List<SvgGradientStop> _stops;

	public List<SvgGradientStop> Stops => _stops;

	[SvgAttribute("spreadMethod")]
	public SvgGradientSpreadMethod SpreadMethod
	{
		get
		{
			return _spreadMethod;
		}
		set
		{
			_spreadMethod = value;
		}
	}

	[SvgAttribute("gradientUnits")]
	public SvgCoordinateUnits GradientUnits
	{
		get
		{
			return _gradientUnits;
		}
		set
		{
			_gradientUnits = value;
		}
	}

	[SvgAttribute("href", "http://www.w3.org/1999/xlink")]
	public SvgPaintServer InheritGradient
	{
		get
		{
			return _inheritGradient;
		}
		set
		{
			_inheritGradient = value;
		}
	}

	[SvgAttribute("gradientTransform")]
	public SvgTransformCollection GradientTransform
	{
		get
		{
			return Attributes.GetAttribute<SvgTransformCollection>("gradientTransform");
		}
		set
		{
			Attributes["gradientTransform"] = value;
		}
	}

	protected Matrix EffectiveGradientTransform
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Expected O, but got Unknown
			Matrix val = new Matrix();
			if (GradientTransform != null)
			{
				val.Multiply(GradientTransform.GetMatrix());
			}
			return val;
		}
	}

	internal SvgGradientServer()
	{
		GradientUnits = SvgCoordinateUnits.ObjectBoundingBox;
		SpreadMethod = SvgGradientSpreadMethod.Pad;
		_stops = new List<SvgGradientStop>();
	}

	protected override void AddElement(SvgElement child, int index)
	{
		if (child is SvgGradientStop)
		{
			Stops.Add((SvgGradientStop)child);
		}
		base.AddElement(child, index);
	}

	protected override void RemoveElement(SvgElement child)
	{
		if (child is SvgGradientStop)
		{
			Stops.Remove((SvgGradientStop)child);
		}
		base.RemoveElement(child);
	}

	protected ColorBlend GetColorBlend(ISvgRenderer renderer, float opacity, bool radial)
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		int num = Stops.Count;
		bool flag = false;
		bool flag2 = false;
		if (Stops[0].Offset.Value > 0f)
		{
			num++;
			if (radial)
			{
				flag2 = true;
			}
			else
			{
				flag = true;
			}
		}
		float value = Stops[Stops.Count - 1].Offset.Value;
		if (value < 100f || value < 1f)
		{
			num++;
			if (radial)
			{
				flag = true;
			}
			else
			{
				flag2 = true;
			}
		}
		ColorBlend val = new ColorBlend(num);
		int num2 = 0;
		float num3 = 0f;
		Color black = System.Drawing.Color.Black;
		for (int i = 0; i < num; i++)
		{
			SvgGradientStop svgGradientStop = Stops[radial ? (Stops.Count - 1 - num2) : num2];
			float width = renderer.GetBoundable().Bounds.Width;
			float num4 = opacity * svgGradientStop.Opacity;
			num3 = (radial ? (1f - svgGradientStop.Offset.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this) / width) : (svgGradientStop.Offset.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this) / width));
			num3 = (float)Math.Round(num3, 1, MidpointRounding.AwayFromZero);
			black = System.Drawing.Color.FromArgb((int)Math.Round(num4 * 255f), svgGradientStop.GetColor(this));
			num2++;
			if (flag && i == 0)
			{
				val.Positions[i] = 0f;
				val.Colors[i] = black;
				i++;
			}
			val.Positions[i] = num3;
			val.Colors[i] = black;
			if (flag2 && i == num - 2)
			{
				i++;
				val.Positions[i] = 1f;
				val.Colors[i] = black;
			}
		}
		return val;
	}

	protected void LoadStops(SvgVisualElement parent)
	{
		SvgGradientServer svgGradientServer = SvgDeferredPaintServer.TryGet<SvgGradientServer>(_inheritGradient, parent);
		if (Stops.Count == 0 && svgGradientServer != null)
		{
			_stops.AddRange(svgGradientServer.Stops);
		}
	}

	protected static double CalculateDistance(PointF first, PointF second)
	{
		return Math.Sqrt(Math.Pow(first.X - second.X, 2.0) + Math.Pow(first.Y - second.Y, 2.0));
	}

	protected static float CalculateLength(PointF vector)
	{
		return (float)Math.Sqrt(Math.Pow(vector.X, 2.0) + Math.Pow(vector.Y, 2.0));
	}

	public SvgCoordinateUnits GetUnits()
	{
		return _gradientUnits;
	}
}
