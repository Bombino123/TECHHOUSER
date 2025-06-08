#define TRACE
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace AntdUI.Svg;

public class SvgPolyline : SvgPolygon
{
	private GraphicsPath _Path;

	public override string ClassName => "polyline";

	public override GraphicsPath Path(ISvgRenderer renderer)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		if ((_Path == null || IsPathDirty) && (float)base.StrokeWidth > 0f)
		{
			_Path = new GraphicsPath();
			try
			{
				for (int i = 0; i + 1 < base.Points.Count; i += 2)
				{
					PointF pointF = new PointF(base.Points[i].ToDeviceValue(renderer, UnitRenderingType.Horizontal, this), base.Points[i + 1].ToDeviceValue(renderer, UnitRenderingType.Vertical, this));
					if (renderer == null)
					{
						float num = (float)base.StrokeWidth / 2f;
						_Path.AddEllipse(pointF.X - num, pointF.Y - num, 2f * num, 2f * num);
					}
					else if (_Path.PointCount == 0)
					{
						_Path.AddLine(pointF, pointF);
					}
					else
					{
						_Path.AddLine(_Path.GetLastPoint(), pointF);
					}
				}
			}
			catch (Exception ex)
			{
				Trace.TraceError("Error rendering points: " + ex.Message);
			}
			if (renderer != null)
			{
				IsPathDirty = false;
			}
		}
		return _Path;
	}
}
