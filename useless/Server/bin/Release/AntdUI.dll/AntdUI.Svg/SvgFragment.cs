using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace AntdUI.Svg;

public class SvgFragment : SvgElement, ISvgViewPort, ISvgBoundable
{
	public static readonly Uri Namespace = new Uri("http://www.w3.org/2000/svg");

	private SvgUnit _x;

	private SvgUnit _y;

	public override string ClassName => "svg";

	PointF ISvgBoundable.Location => PointF.Empty;

	SizeF ISvgBoundable.Size => GetDimensions();

	RectangleF ISvgBoundable.Bounds => new RectangleF(((ISvgBoundable)this).Location, ((ISvgBoundable)this).Size);

	[SvgAttribute("x")]
	public SvgUnit X
	{
		get
		{
			return _x;
		}
		set
		{
			if (_x != value)
			{
				_x = value;
				OnAttributeChanged(new AttributeEventArgs
				{
					Attribute = "x",
					Value = value
				});
			}
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
			if (_y != value)
			{
				_y = value;
				OnAttributeChanged(new AttributeEventArgs
				{
					Attribute = "y",
					Value = value
				});
			}
		}
	}

	[SvgAttribute("width")]
	public SvgUnit Width
	{
		get
		{
			return Attributes.GetAttribute<SvgUnit>("width");
		}
		set
		{
			Attributes["width"] = value;
		}
	}

	[SvgAttribute("height")]
	public SvgUnit Height
	{
		get
		{
			return Attributes.GetAttribute<SvgUnit>("height");
		}
		set
		{
			Attributes["height"] = value;
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

	[SvgAttribute("font-size")]
	public override SvgUnit FontSize
	{
		get
		{
			if (Attributes["font-size"] != null)
			{
				return (SvgUnit)Attributes["font-size"];
			}
			return SvgUnit.Empty;
		}
		set
		{
			Attributes["font-size"] = value;
		}
	}

	[SvgAttribute("font-family")]
	public override string FontFamily
	{
		get
		{
			return Attributes["font-family"] as string;
		}
		set
		{
			Attributes["font-family"] = value;
		}
	}

	public GraphicsPath Path
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Expected O, but got Unknown
			GraphicsPath val = new GraphicsPath();
			AddPaths(this, val);
			return val;
		}
	}

	public RectangleF Bounds
	{
		get
		{
			RectangleF rectangleF = default(RectangleF);
			foreach (SvgElement child in Children)
			{
				RectangleF rectangleF2 = default(RectangleF);
				if (child is SvgFragment)
				{
					rectangleF2 = ((SvgFragment)child).Bounds;
					rectangleF2.Offset(((SvgFragment)child).X, ((SvgFragment)child).Y);
				}
				else if (child is SvgVisualElement)
				{
					rectangleF2 = ((SvgVisualElement)child).Bounds;
				}
				if (!rectangleF2.IsEmpty)
				{
					rectangleF = ((!rectangleF.IsEmpty) ? RectangleF.Union(rectangleF, rectangleF2) : rectangleF2);
				}
			}
			return TransformedBounds(rectangleF);
		}
	}

	protected internal override bool PushTransforms(ISvgRenderer renderer)
	{
		if (!base.PushTransforms(renderer))
		{
			return false;
		}
		ViewBox.AddViewBoxTransform(AspectRatio, renderer, this);
		return true;
	}

	protected override void Render(ISvgRenderer renderer)
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		SvgOverflow overflow = Overflow;
		if ((uint)(overflow - 1) <= 2u)
		{
			base.Render(renderer);
			return;
		}
		Region clip = renderer.GetClip();
		try
		{
			SizeF sizeF = ((Parent == null) ? renderer.GetBoundable().Bounds.Size : GetDimensions());
			RectangleF rectangleF = new RectangleF(X.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this), Y.ToDeviceValue(renderer, UnitRenderingType.Vertical, this), sizeF.Width, sizeF.Height);
			renderer.SetClip(new Region(rectangleF), (CombineMode)1);
			base.Render(renderer);
		}
		finally
		{
			renderer.SetClip(clip, (CombineMode)0);
		}
	}

	public SvgFragment()
	{
		_x = 0f;
		_y = 0f;
		Height = new SvgUnit(SvgUnitType.Percentage, 100f);
		Width = new SvgUnit(SvgUnitType.Percentage, 100f);
		ViewBox = SvgViewBox.Empty;
		AspectRatio = new SvgAspectRatio(SvgPreserveAspectRatio.xMidYMid);
	}

	public SizeF GetDimensions()
	{
		bool num = Width.Type == SvgUnitType.Percentage;
		bool flag = Height.Type == SvgUnitType.Percentage;
		RectangleF rectangleF = default(RectangleF);
		if (num || flag)
		{
			rectangleF = ((!(ViewBox.Width > 0f) || !(ViewBox.Height > 0f)) ? Bounds : new RectangleF(ViewBox.MinX, ViewBox.MinY, ViewBox.Width, ViewBox.Height));
		}
		float width = ((!num) ? Width.ToDeviceValue(null, UnitRenderingType.Horizontal, this) : ((rectangleF.Width + rectangleF.X) * (Width.Value * 0.01f)));
		float height = ((!flag) ? Height.ToDeviceValue(null, UnitRenderingType.Vertical, this) : ((rectangleF.Height + rectangleF.Y) * (Height.Value * 0.01f)));
		return new SizeF(width, height);
	}
}
