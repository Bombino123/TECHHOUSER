using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Serialization;

namespace GMap.NET.WindowsForms.ToolTips;

[Serializable]
public class GMapRoundedToolTip : GMapToolTip, ISerializable
{
	public float Radius = 10f;

	public GMapRoundedToolTip(GMapMarker marker)
		: base(marker)
	{
		TextPadding = new Size((int)Radius, (int)Radius);
	}

	public new void DrawRoundRectangle(Graphics g, Pen pen, float h, float v, float width, float height, float radius)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		GraphicsPath val = new GraphicsPath();
		try
		{
			val.AddLine(h + radius, v, h + width - radius * 2f, v);
			val.AddArc(h + width - radius * 2f, v, radius * 2f, radius * 2f, 270f, 90f);
			val.AddLine(h + width, v + radius, h + width, v + height - radius * 2f);
			val.AddArc(h + width - radius * 2f, v + height - radius * 2f, radius * 2f, radius * 2f, 0f, 90f);
			val.AddLine(h + width - radius * 2f, v + height, h + radius, v + height);
			val.AddArc(h, v + height - radius * 2f, radius * 2f, radius * 2f, 90f, 90f);
			val.AddLine(h, v + height - radius * 2f, h, v + radius);
			val.AddArc(h, v, radius * 2f, radius * 2f, 180f, 90f);
			val.CloseFigure();
			g.FillPath(Fill, val);
			g.DrawPath(pen, val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public override void OnRender(Graphics g)
	{
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		Size size = g.MeasureString(base.Marker.ToolTipText, Font).ToSize();
		Rectangle rectangle = new Rectangle(base.Marker.ToolTipPosition.X, base.Marker.ToolTipPosition.Y - size.Height, size.Width + TextPadding.Width * 2, size.Height + TextPadding.Height);
		rectangle.Offset(Offset.X, Offset.Y);
		int num = 0;
		if (!g.VisibleClipBounds.Contains(rectangle))
		{
			Point pos = default(Point);
			if ((float)rectangle.Right > g.VisibleClipBounds.Right)
			{
				pos.X = -((rectangle.Left - base.Marker.LocalPosition.X) / 2 + rectangle.Width);
				num = -(rectangle.Width - (int)Radius);
			}
			if ((float)rectangle.Top < g.VisibleClipBounds.Top)
			{
				pos.Y = rectangle.Bottom - base.Marker.LocalPosition.Y + rectangle.Height * 2;
			}
			rectangle.Offset(pos);
		}
		g.DrawLine(Stroke, (float)base.Marker.ToolTipPosition.X, (float)base.Marker.ToolTipPosition.Y, (float)(rectangle.X - num) + Radius / 2f, (float)(rectangle.Y + rectangle.Height) - Radius / 2f);
		DrawRoundRectangle(g, Stroke, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, Radius);
		if ((int)Format.Alignment == 0)
		{
			rectangle.Offset(TextPadding.Width, 0);
		}
		g.DrawString(base.Marker.ToolTipText, Font, Foreground, (RectangleF)rectangle, Format);
	}

	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("Radius", Radius);
		GetObjectData(info, context);
	}

	protected GMapRoundedToolTip(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		Radius = Extensions.GetStruct<float>(info, "Radius", 10f);
	}
}
