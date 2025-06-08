using System.Drawing;
using System.Drawing.Drawing2D;

namespace AntdUI.Svg;

public class SvgEllipse : SvgPathBasedElement
{
	private GraphicsPath _path;

	public override string ClassName => "ellipse";

	[SvgAttribute("cx")]
	public virtual SvgUnit CenterX
	{
		get
		{
			return Attributes.GetAttribute<SvgUnit>("cx");
		}
		set
		{
			Attributes["cx"] = value;
			IsPathDirty = true;
		}
	}

	[SvgAttribute("cy")]
	public virtual SvgUnit CenterY
	{
		get
		{
			return Attributes.GetAttribute<SvgUnit>("cy");
		}
		set
		{
			Attributes["cy"] = value;
			IsPathDirty = true;
		}
	}

	[SvgAttribute("rx")]
	public virtual SvgUnit RadiusX
	{
		get
		{
			return Attributes.GetAttribute<SvgUnit>("rx");
		}
		set
		{
			Attributes["rx"] = value;
			IsPathDirty = true;
		}
	}

	[SvgAttribute("ry")]
	public virtual SvgUnit RadiusY
	{
		get
		{
			return Attributes.GetAttribute<SvgUnit>("ry");
		}
		set
		{
			Attributes["ry"] = value;
			IsPathDirty = true;
		}
	}

	public override GraphicsPath Path(ISvgRenderer renderer)
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		if (_path == null || IsPathDirty)
		{
			float num = (float)base.StrokeWidth / 2f;
			if (renderer != null)
			{
				num = 0f;
				IsPathDirty = false;
			}
			PointF devicePoint = SvgUnit.GetDevicePoint(CenterX, CenterY, renderer, this);
			float num2 = RadiusX.ToDeviceValue(renderer, UnitRenderingType.Other, this) + num;
			float num3 = RadiusY.ToDeviceValue(renderer, UnitRenderingType.Other, this) + num;
			_path = new GraphicsPath();
			_path.StartFigure();
			_path.AddEllipse(devicePoint.X - num2, devicePoint.Y - num3, 2f * num2, 2f * num3);
			_path.CloseFigure();
		}
		return _path;
	}

	protected override void Render(ISvgRenderer renderer)
	{
		if (RadiusX.Value > 0f && RadiusY.Value > 0f)
		{
			base.Render(renderer);
		}
	}
}
