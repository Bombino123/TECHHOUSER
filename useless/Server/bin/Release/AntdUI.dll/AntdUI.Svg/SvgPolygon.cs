#define TRACE
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace AntdUI.Svg;

public class SvgPolygon : SvgMarkerElement
{
	private GraphicsPath _path;

	public override string ClassName => "polygon";

	[SvgAttribute("points")]
	public SvgPointCollection Points
	{
		get
		{
			return Attributes.GetAttribute<SvgPointCollection>("points");
		}
		set
		{
			Attributes["points"] = value;
			IsPathDirty = true;
		}
	}

	public override GraphicsPath Path(ISvgRenderer renderer)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		if (_path == null || IsPathDirty)
		{
			_path = new GraphicsPath();
			_path.StartFigure();
			try
			{
				SvgPointCollection points = Points;
				for (int i = 2; i + 1 < points.Count; i += 2)
				{
					PointF devicePoint = SvgUnit.GetDevicePoint(points[i], points[i + 1], renderer, this);
					if (renderer == null)
					{
						float num = (float)base.StrokeWidth * 2f;
						_path.AddEllipse(devicePoint.X - num, devicePoint.Y - num, 2f * num, 2f * num);
					}
					else if (_path.PointCount == 0)
					{
						_path.AddLine(SvgUnit.GetDevicePoint(points[i - 2], points[i - 1], renderer, this), devicePoint);
					}
					else
					{
						_path.AddLine(_path.GetLastPoint(), devicePoint);
					}
				}
			}
			catch
			{
				Trace.TraceError("Error parsing points");
			}
			_path.CloseFigure();
			if (renderer != null)
			{
				IsPathDirty = false;
			}
		}
		return _path;
	}
}
