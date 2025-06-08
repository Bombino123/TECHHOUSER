using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace AntdUI.Svg;

public class SvgRectangle : SvgPathBasedElement
{
	private GraphicsPath _path;

	public override string ClassName => "rect";

	public SvgPoint Location => new SvgPoint(X, Y);

	[SvgAttribute("x")]
	public SvgUnit X
	{
		get
		{
			return Attributes.GetAttribute<SvgUnit>("x");
		}
		set
		{
			Attributes["x"] = value;
			IsPathDirty = true;
		}
	}

	[SvgAttribute("y")]
	public SvgUnit Y
	{
		get
		{
			return Attributes.GetAttribute<SvgUnit>("y");
		}
		set
		{
			Attributes["y"] = value;
			IsPathDirty = true;
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
			IsPathDirty = true;
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
			IsPathDirty = true;
		}
	}

	[SvgAttribute("rx")]
	public SvgUnit CornerRadiusX
	{
		get
		{
			SvgUnit attribute = Attributes.GetAttribute<SvgUnit>("rx");
			SvgUnit attribute2 = Attributes.GetAttribute<SvgUnit>("ry");
			if (attribute.Value != 0f || !(attribute2.Value > 0f))
			{
				return attribute;
			}
			return attribute2;
		}
		set
		{
			Attributes["rx"] = value;
			IsPathDirty = true;
		}
	}

	[SvgAttribute("ry")]
	public SvgUnit CornerRadiusY
	{
		get
		{
			SvgUnit attribute = Attributes.GetAttribute<SvgUnit>("rx");
			SvgUnit attribute2 = Attributes.GetAttribute<SvgUnit>("ry");
			if (attribute2.Value != 0f || !(attribute.Value > 0f))
			{
				return attribute2;
			}
			return attribute;
		}
		set
		{
			Attributes["ry"] = value;
			IsPathDirty = true;
		}
	}

	protected override bool RequiresSmoothRendering
	{
		get
		{
			if (base.RequiresSmoothRendering)
			{
				if (!(CornerRadiusX.Value > 0f))
				{
					return CornerRadiusY.Value > 0f;
				}
				return true;
			}
			return false;
		}
	}

	public override GraphicsPath Path(ISvgRenderer renderer)
	{
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Expected O, but got Unknown
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Expected O, but got Unknown
		if (_path == null || IsPathDirty)
		{
			float num = (float)base.StrokeWidth / 2f;
			if (renderer != null)
			{
				num = 0f;
				IsPathDirty = false;
			}
			if (renderer == null || (CornerRadiusX.Value == 0f && CornerRadiusY.Value == 0f))
			{
				float num2 = Location.Y.ToDeviceValue(renderer, UnitRenderingType.Vertical, this);
				float num3 = Location.X.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this);
				SvgPoint svgPoint = new SvgPoint(num3 - num, num2 - num);
				float width = Width.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this) + num * 2f;
				float height = Height.ToDeviceValue(renderer, UnitRenderingType.Vertical, this) + num * 2f;
				RectangleF rectangleF = new RectangleF(svgPoint.ToDeviceValue(renderer, this), new SizeF(width, height));
				_path = new GraphicsPath();
				_path.StartFigure();
				_path.AddRectangle(rectangleF);
				_path.CloseFigure();
			}
			else
			{
				_path = new GraphicsPath();
				RectangleF rectangleF2 = default(RectangleF);
				PointF pointF = default(PointF);
				PointF pointF2 = default(PointF);
				float num4 = Width.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this);
				float num5 = Height.ToDeviceValue(renderer, UnitRenderingType.Vertical, this);
				float num6 = Math.Min(CornerRadiusX.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this) * 2f, num4);
				float num7 = Math.Min(CornerRadiusY.ToDeviceValue(renderer, UnitRenderingType.Vertical, this) * 2f, num5);
				PointF location = Location.ToDeviceValue(renderer, this);
				_path.StartFigure();
				rectangleF2.Location = location;
				rectangleF2.Width = num6;
				rectangleF2.Height = num7;
				_path.AddArc(rectangleF2, 180f, 90f);
				pointF.X = Math.Min(location.X + num6, location.X + num4 * 0.5f);
				pointF.Y = location.Y;
				pointF2.X = Math.Max(location.X + num4 - num6, location.X + num4 * 0.5f);
				pointF2.Y = pointF.Y;
				_path.AddLine(pointF, pointF2);
				rectangleF2.Location = new PointF(location.X + num4 - num6, location.Y);
				_path.AddArc(rectangleF2, 270f, 90f);
				pointF.X = location.X + num4;
				pointF.Y = Math.Min(location.Y + num7, location.Y + num5 * 0.5f);
				pointF2.X = pointF.X;
				pointF2.Y = Math.Max(location.Y + num5 - num7, location.Y + num5 * 0.5f);
				_path.AddLine(pointF, pointF2);
				rectangleF2.Location = new PointF(location.X + num4 - num6, location.Y + num5 - num7);
				_path.AddArc(rectangleF2, 0f, 90f);
				pointF.X = Math.Max(location.X + num4 - num6, location.X + num4 * 0.5f);
				pointF.Y = location.Y + num5;
				pointF2.X = Math.Min(location.X + num6, location.X + num4 * 0.5f);
				pointF2.Y = pointF.Y;
				_path.AddLine(pointF, pointF2);
				rectangleF2.Location = new PointF(location.X, location.Y + num5 - num7);
				_path.AddArc(rectangleF2, 90f, 90f);
				pointF.X = location.X;
				pointF.Y = Math.Max(location.Y + num5 - num7, location.Y + num5 * 0.5f);
				pointF2.X = pointF.X;
				pointF2.Y = Math.Min(location.Y + num7, location.Y + num5 * 0.5f);
				_path.AddLine(pointF, pointF2);
				_path.CloseFigure();
			}
		}
		return _path;
	}

	protected override void Render(ISvgRenderer renderer)
	{
		if (Width.Value > 0f && Height.Value > 0f)
		{
			base.Render(renderer);
		}
	}
}
