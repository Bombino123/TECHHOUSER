using System.Drawing;
using System.Drawing.Drawing2D;

namespace AntdUI.Svg;

public class SvgCircle : SvgPathBasedElement
{
	private GraphicsPath _path;

	public override string ClassName => "circle";

	public SvgPoint Center => new SvgPoint(CenterX, CenterY);

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

	[SvgAttribute("r")]
	public virtual SvgUnit Radius
	{
		get
		{
			return Attributes.GetAttribute<SvgUnit>("r");
		}
		set
		{
			Attributes["r"] = value;
			IsPathDirty = true;
		}
	}

	public override GraphicsPath Path(ISvgRenderer renderer)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		if (_path == null || IsPathDirty)
		{
			float num = (float)base.StrokeWidth / 2f;
			if (renderer != null)
			{
				num = 0f;
				IsPathDirty = false;
			}
			_path = new GraphicsPath();
			_path.StartFigure();
			PointF pointF = Center.ToDeviceValue(renderer, this);
			float num2 = Radius.ToDeviceValue(renderer, UnitRenderingType.Other, this) + num;
			_path.AddEllipse(pointF.X - num2, pointF.Y - num2, 2f * num2, 2f * num2);
			_path.CloseFigure();
		}
		return _path;
	}

	protected override void Render(ISvgRenderer renderer)
	{
		if (Radius.Value > 0f)
		{
			base.Render(renderer);
		}
	}
}
