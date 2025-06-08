using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using AntdUI.Svg.Transforms;

namespace AntdUI.Svg;

public sealed class SvgPatternServer : SvgPaintServer, ISvgViewPort, ISvgSupportsCoordinateUnits
{
	private SvgUnit _width;

	private SvgUnit _height;

	private SvgUnit _x;

	private SvgUnit _y;

	private SvgPaintServer _inheritGradient;

	private SvgViewBox _viewBox;

	private SvgCoordinateUnits _patternUnits;

	private SvgCoordinateUnits _patternContentUnits;

	public override string ClassName => "pattern";

	[SvgAttribute("overflow")]
	public SvgOverflow Overflow
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
	public SvgViewBox ViewBox
	{
		get
		{
			return _viewBox;
		}
		set
		{
			_viewBox = value;
		}
	}

	[SvgAttribute("preserveAspectRatio")]
	public SvgAspectRatio AspectRatio { get; set; }

	[SvgAttribute("width")]
	public SvgUnit Width
	{
		get
		{
			return _width;
		}
		set
		{
			_width = value;
		}
	}

	[SvgAttribute("patternUnits")]
	public SvgCoordinateUnits PatternUnits
	{
		get
		{
			return _patternUnits;
		}
		set
		{
			_patternUnits = value;
		}
	}

	[SvgAttribute("patternContentUnits")]
	public SvgCoordinateUnits PatternContentUnits
	{
		get
		{
			return _patternContentUnits;
		}
		set
		{
			_patternContentUnits = value;
		}
	}

	[SvgAttribute("height")]
	public SvgUnit Height
	{
		get
		{
			return _height;
		}
		set
		{
			_height = value;
		}
	}

	[SvgAttribute("x")]
	public SvgUnit X
	{
		get
		{
			return _x;
		}
		set
		{
			_x = value;
		}
	}

	[SvgAttribute("y")]
	public SvgUnit Y
	{
		get
		{
			return _y;
		}
		set
		{
			_y = value;
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

	[SvgAttribute("patternTransform")]
	public SvgTransformCollection PatternTransform
	{
		get
		{
			return Attributes.GetAttribute<SvgTransformCollection>("patternTransform");
		}
		set
		{
			Attributes["patternTransform"] = value;
		}
	}

	private Matrix EffectivePatternTransform
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Expected O, but got Unknown
			Matrix val = new Matrix();
			if (PatternTransform != null)
			{
				val.Multiply(PatternTransform.GetMatrix());
			}
			return val;
		}
	}

	public SvgPatternServer()
	{
		_x = SvgUnit.None;
		_y = SvgUnit.None;
		_width = SvgUnit.None;
		_height = SvgUnit.None;
	}

	private SvgUnit NormalizeUnit(SvgUnit orig)
	{
		if (orig.Type != SvgUnitType.Percentage || PatternUnits != SvgCoordinateUnits.ObjectBoundingBox)
		{
			return orig;
		}
		return new SvgUnit(SvgUnitType.User, orig.Value / 100f);
	}

	public override Brush GetBrush(SvgVisualElement renderingElement, ISvgRenderer renderer, float opacity, bool forStroke = false)
	{
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Expected O, but got Unknown
		//IL_030f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Expected O, but got Unknown
		//IL_03a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ad: Expected O, but got Unknown
		//IL_03ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_040d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0417: Expected O, but got Unknown
		List<SvgPatternServer> list = new List<SvgPatternServer>();
		for (SvgPatternServer svgPatternServer = this; svgPatternServer != null; svgPatternServer = SvgDeferredPaintServer.TryGet<SvgPatternServer>(svgPatternServer._inheritGradient, renderingElement))
		{
			list.Add(svgPatternServer);
		}
		SvgPatternServer svgPatternServer2 = list.Where((SvgPatternServer p) => p.Children != null && p.Children.Count > 0).FirstOrDefault();
		if (svgPatternServer2 == null)
		{
			return null;
		}
		SvgPatternServer svgPatternServer3 = list.Where(delegate(SvgPatternServer p)
		{
			_ = p.Width;
			return p.Width != SvgUnit.None;
		}).FirstOrDefault();
		SvgPatternServer svgPatternServer4 = list.Where(delegate(SvgPatternServer p)
		{
			_ = p.Height;
			return p.Height != SvgUnit.None;
		}).FirstOrDefault();
		if (svgPatternServer3 == null && svgPatternServer4 == null)
		{
			return null;
		}
		SvgViewBox svgViewBox = list.Where(delegate(SvgPatternServer p)
		{
			_ = p.ViewBox;
			return p.ViewBox != SvgViewBox.Empty;
		}).FirstOrDefault()?.ViewBox ?? SvgViewBox.Empty;
		SvgPatternServer svgPatternServer5 = list.Where(delegate(SvgPatternServer p)
		{
			_ = p.X;
			return p.X != SvgUnit.None;
		}).FirstOrDefault();
		SvgPatternServer svgPatternServer6 = list.Where(delegate(SvgPatternServer p)
		{
			_ = p.Y;
			return p.Y != SvgUnit.None;
		}).FirstOrDefault();
		SvgUnit orig = svgPatternServer5?.X ?? SvgUnit.Empty;
		SvgUnit orig2 = svgPatternServer6?.Y ?? SvgUnit.Empty;
		SvgCoordinateUnits svgCoordinateUnits = list.Where((SvgPatternServer p) => p.PatternUnits != SvgCoordinateUnits.Inherit).FirstOrDefault()?.PatternUnits ?? SvgCoordinateUnits.ObjectBoundingBox;
		SvgCoordinateUnits svgCoordinateUnits2 = list.Where((SvgPatternServer p) => p.PatternContentUnits != SvgCoordinateUnits.Inherit).FirstOrDefault()?.PatternContentUnits ?? SvgCoordinateUnits.UserSpaceOnUse;
		try
		{
			if (svgCoordinateUnits == SvgCoordinateUnits.ObjectBoundingBox)
			{
				renderer.SetBoundable(renderingElement);
			}
			Matrix val = new Matrix();
			try
			{
				RectangleF bounds = renderer.GetBoundable().Bounds;
				float num = ((svgCoordinateUnits == SvgCoordinateUnits.ObjectBoundingBox) ? bounds.Width : 1f);
				float num2 = ((svgCoordinateUnits == SvgCoordinateUnits.ObjectBoundingBox) ? bounds.Height : 1f);
				float num3 = num * NormalizeUnit(orig).ToDeviceValue(renderer, UnitRenderingType.Horizontal, this);
				float num4 = num2 * NormalizeUnit(orig2).ToDeviceValue(renderer, UnitRenderingType.Vertical, this);
				float num5 = num * NormalizeUnit(svgPatternServer3.Width).ToDeviceValue(renderer, UnitRenderingType.Horizontal, this);
				float num6 = num2 * NormalizeUnit(svgPatternServer4.Height).ToDeviceValue(renderer, UnitRenderingType.Vertical, this);
				val.Scale(((svgCoordinateUnits2 == SvgCoordinateUnits.ObjectBoundingBox) ? bounds.Width : 1f) * ((svgViewBox.Width > 0f) ? (num5 / svgViewBox.Width) : 1f), ((svgCoordinateUnits2 == SvgCoordinateUnits.ObjectBoundingBox) ? bounds.Height : 1f) * ((svgViewBox.Height > 0f) ? (num6 / svgViewBox.Height) : 1f), (MatrixOrder)0);
				Bitmap val2 = new Bitmap((int)num5, (int)num6);
				using (ISvgRenderer svgRenderer = SvgRenderer.FromImage((Image)(object)val2))
				{
					ISvgBoundable boundable;
					if (_patternContentUnits != SvgCoordinateUnits.ObjectBoundingBox)
					{
						boundable = renderer.GetBoundable();
					}
					else
					{
						ISvgBoundable svgBoundable = new GenericBoundable(0f, 0f, num5, num6);
						boundable = svgBoundable;
					}
					svgRenderer.SetBoundable(boundable);
					svgRenderer.Transform = val;
					svgRenderer.SmoothingMode = (SmoothingMode)4;
					svgRenderer.SetClip(new Region(new RectangleF(0f, 0f, (svgViewBox.Width > 0f) ? svgViewBox.Width : num5, (svgViewBox.Height > 0f) ? svgViewBox.Height : num6)), (CombineMode)0);
					foreach (SvgElement child in svgPatternServer2.Children)
					{
						child.RenderElement(svgRenderer);
					}
				}
				TextureBrush val3 = new TextureBrush((Image)(object)val2);
				Matrix val4 = EffectivePatternTransform.Clone();
				val4.Translate(num3, num4, (MatrixOrder)1);
				val3.Transform = val4;
				return (Brush)val3;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		finally
		{
			if (PatternUnits == SvgCoordinateUnits.ObjectBoundingBox)
			{
				renderer.PopBoundable();
			}
		}
	}

	public SvgCoordinateUnits GetUnits()
	{
		return _patternUnits;
	}
}
