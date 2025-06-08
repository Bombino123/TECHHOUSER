using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Serialization;

namespace GMap.NET.WindowsForms.ToolTips;

[Serializable]
public class GMapBaloonToolTip : GMapToolTip, ISerializable
{
	public float Radius = 10f;

	public new static readonly Pen DefaultStroke;

	static GMapBaloonToolTip()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		DefaultStroke = new Pen(Color.FromArgb(140, Color.Navy));
		DefaultStroke.Width = 3f;
		DefaultStroke.LineJoin = (LineJoin)2;
		DefaultStroke.StartCap = (LineCap)18;
	}

	public GMapBaloonToolTip(GMapMarker marker)
		: base(marker)
	{
		Stroke = DefaultStroke;
		Fill = Brushes.Yellow;
	}

	public override void OnRender(Graphics g)
	{
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		Size size = g.MeasureString(base.Marker.ToolTipText, Font).ToSize();
		Rectangle rectangle = new Rectangle(base.Marker.ToolTipPosition.X, base.Marker.ToolTipPosition.Y - size.Height, size.Width + TextPadding.Width, size.Height + TextPadding.Height);
		rectangle.Offset(Offset.X, Offset.Y);
		GraphicsPath val = new GraphicsPath();
		try
		{
			val.AddLine((float)rectangle.X + 2f * Radius, (float)(rectangle.Y + rectangle.Height), (float)rectangle.X + Radius, (float)(rectangle.Y + rectangle.Height) + Radius);
			val.AddLine((float)rectangle.X + Radius, (float)(rectangle.Y + rectangle.Height) + Radius, (float)rectangle.X + Radius, (float)(rectangle.Y + rectangle.Height));
			val.AddArc((float)rectangle.X, (float)(rectangle.Y + rectangle.Height) - Radius * 2f, Radius * 2f, Radius * 2f, 90f, 90f);
			val.AddLine((float)rectangle.X, (float)(rectangle.Y + rectangle.Height) - Radius * 2f, (float)rectangle.X, (float)rectangle.Y + Radius);
			val.AddArc((float)rectangle.X, (float)rectangle.Y, Radius * 2f, Radius * 2f, 180f, 90f);
			val.AddLine((float)rectangle.X + Radius, (float)rectangle.Y, (float)(rectangle.X + rectangle.Width) - Radius * 2f, (float)rectangle.Y);
			val.AddArc((float)(rectangle.X + rectangle.Width) - Radius * 2f, (float)rectangle.Y, Radius * 2f, Radius * 2f, 270f, 90f);
			val.AddLine((float)(rectangle.X + rectangle.Width), (float)rectangle.Y + Radius, (float)(rectangle.X + rectangle.Width), (float)(rectangle.Y + rectangle.Height) - Radius * 2f);
			val.AddArc((float)(rectangle.X + rectangle.Width) - Radius * 2f, (float)(rectangle.Y + rectangle.Height) - Radius * 2f, Radius * 2f, Radius * 2f, 0f, 90f);
			val.CloseFigure();
			g.FillPath(Fill, val);
			g.DrawPath(Stroke, val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		g.DrawString(base.Marker.ToolTipText, Font, Foreground, (RectangleF)rectangle, Format);
	}

	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("Radius", Radius);
		GetObjectData(info, context);
	}

	protected GMapBaloonToolTip(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		Radius = Extensions.GetStruct<float>(info, "Radius", 10f);
	}
}
