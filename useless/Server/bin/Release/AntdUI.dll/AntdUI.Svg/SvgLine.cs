using System.Drawing;
using System.Drawing.Drawing2D;

namespace AntdUI.Svg;

public class SvgLine : SvgMarkerElement
{
	private SvgUnit _startX;

	private SvgUnit _startY;

	private SvgUnit _endX;

	private SvgUnit _endY;

	private GraphicsPath _path;

	public override string ClassName => "line";

	[SvgAttribute("x1")]
	public SvgUnit StartX
	{
		get
		{
			return _startX;
		}
		set
		{
			if (_startX != value)
			{
				_startX = value;
				IsPathDirty = true;
				OnAttributeChanged(new AttributeEventArgs
				{
					Attribute = "x1",
					Value = value
				});
			}
		}
	}

	[SvgAttribute("y1")]
	public SvgUnit StartY
	{
		get
		{
			return _startY;
		}
		set
		{
			if (_startY != value)
			{
				_startY = value;
				IsPathDirty = true;
				OnAttributeChanged(new AttributeEventArgs
				{
					Attribute = "y1",
					Value = value
				});
			}
		}
	}

	[SvgAttribute("x2")]
	public SvgUnit EndX
	{
		get
		{
			return _endX;
		}
		set
		{
			if (_endX != value)
			{
				_endX = value;
				IsPathDirty = true;
				OnAttributeChanged(new AttributeEventArgs
				{
					Attribute = "x2",
					Value = value
				});
			}
		}
	}

	[SvgAttribute("y2")]
	public SvgUnit EndY
	{
		get
		{
			return _endY;
		}
		set
		{
			if (_endY != value)
			{
				_endY = value;
				IsPathDirty = true;
				OnAttributeChanged(new AttributeEventArgs
				{
					Attribute = "y2",
					Value = value
				});
			}
		}
	}

	public override SvgPaintServer Fill
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public override GraphicsPath Path(ISvgRenderer renderer)
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Expected O, but got Unknown
		if ((_path == null || IsPathDirty) && (float)base.StrokeWidth > 0f)
		{
			PointF pointF = new PointF(StartX.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this), StartY.ToDeviceValue(renderer, UnitRenderingType.Vertical, this));
			PointF pointF2 = new PointF(EndX.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this), EndY.ToDeviceValue(renderer, UnitRenderingType.Vertical, this));
			_path = new GraphicsPath();
			if (renderer != null)
			{
				_path.AddLine(pointF, pointF2);
				IsPathDirty = false;
			}
			else
			{
				_path.StartFigure();
				float num = (float)base.StrokeWidth / 2f;
				_path.AddEllipse(pointF.X - num, pointF.Y - num, 2f * num, 2f * num);
				_path.AddEllipse(pointF2.X - num, pointF2.Y - num, 2f * num, 2f * num);
				_path.CloseFigure();
			}
		}
		return _path;
	}
}
