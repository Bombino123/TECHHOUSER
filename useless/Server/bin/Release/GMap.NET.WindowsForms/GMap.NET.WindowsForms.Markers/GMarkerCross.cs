using System;
using System.Drawing;
using System.Runtime.Serialization;

namespace GMap.NET.WindowsForms.Markers;

[Serializable]
public class GMarkerCross : GMapMarker, ISerializable
{
	public static readonly Pen DefaultPen = new Pen(Brushes.Red, 1f);

	[NonSerialized]
	public Pen Pen = DefaultPen;

	public GMarkerCross(PointLatLng p)
		: base(p)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		IsHitTestVisible = false;
	}

	public override void OnRender(Graphics g)
	{
		Point point = new Point(base.LocalPosition.X, base.LocalPosition.Y);
		point.Offset(0, -10);
		Point point2 = new Point(base.LocalPosition.X, base.LocalPosition.Y);
		point2.Offset(0, 10);
		Point point3 = new Point(base.LocalPosition.X, base.LocalPosition.Y);
		point3.Offset(-10, 0);
		Point point4 = new Point(base.LocalPosition.X, base.LocalPosition.Y);
		point4.Offset(10, 0);
		g.DrawLine(Pen, point.X, point.Y, point2.X, point2.Y);
		g.DrawLine(Pen, point3.X, point3.Y, point4.X, point4.Y);
	}

	public override void Dispose()
	{
		base.Dispose();
	}

	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
		GetObjectData(info, context);
	}

	protected GMarkerCross(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
