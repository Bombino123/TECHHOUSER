using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Serialization;

namespace GMap.NET.WindowsForms;

[Serializable]
public class GMapPolygon : MapRoute, ISerializable, IDeserializationCallback, IDisposable
{
	private bool _visible = true;

	public bool IsHitTestVisible;

	private bool _isMouseOver;

	private GraphicsPath _graphicsPath;

	public static readonly Pen DefaultStroke;

	[NonSerialized]
	public Pen Stroke = DefaultStroke;

	public static readonly Brush DefaultFill;

	[NonSerialized]
	public Brush Fill = DefaultFill;

	public readonly List<GPoint> LocalPoints = new List<GPoint>();

	private GPoint[] deserializedLocalPoints;

	private bool _disposed;

	public bool IsVisible
	{
		get
		{
			return _visible;
		}
		set
		{
			if (value == _visible)
			{
				return;
			}
			_visible = value;
			if (Overlay != null && Overlay.Control != null)
			{
				if (_visible)
				{
					Overlay.Control.UpdatePolygonLocalPosition(this);
				}
				else if (Overlay.Control.IsMouseOverPolygon)
				{
					Overlay.Control.IsMouseOverPolygon = false;
					Overlay.Control.RestoreCursorOnLeave();
				}
				if (!Overlay.Control.HoldInvalidation)
				{
					Overlay.Control.Invalidate();
				}
			}
		}
	}

	public bool IsMouseOver
	{
		get
		{
			return _isMouseOver;
		}
		internal set
		{
			_isMouseOver = value;
		}
	}

	public GMapOverlay Overlay { get; set; }

	internal bool IsInsideLocal(int x, int y)
	{
		if (_graphicsPath != null)
		{
			return _graphicsPath.IsVisible(x, y);
		}
		return false;
	}

	internal void UpdateGraphicsPath()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (_graphicsPath == null)
		{
			_graphicsPath = new GraphicsPath();
		}
		else
		{
			_graphicsPath.Reset();
		}
		Point[] array = new Point[LocalPoints.Count];
		for (int i = 0; i < LocalPoints.Count; i++)
		{
			GPoint val = LocalPoints[i];
			int x = (int)((GPoint)(ref val)).X;
			val = LocalPoints[i];
			Point point = new Point(x, (int)((GPoint)(ref val)).Y);
			array[array.Length - 1 - i] = point;
		}
		if (array.Length > 2)
		{
			_graphicsPath.AddPolygon(array);
		}
		else if (array.Length == 2)
		{
			_graphicsPath.AddLines(array);
		}
	}

	public virtual void OnRender(Graphics g)
	{
		if (IsVisible && IsVisible && _graphicsPath != null)
		{
			g.FillPath(Fill, _graphicsPath);
			g.DrawPath(Stroke, _graphicsPath);
		}
	}

	static GMapPolygon()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		DefaultStroke = new Pen(Color.FromArgb(155, Color.MidnightBlue));
		DefaultFill = (Brush)new SolidBrush(Color.FromArgb(155, Color.AliceBlue));
		DefaultStroke.LineJoin = (LineJoin)2;
		DefaultStroke.Width = 5f;
	}

	public GMapPolygon(List<PointLatLng> points, string name)
		: base((IEnumerable<PointLatLng>)points, name)
	{
		LocalPoints.Capacity = base.Points.Count;
	}

	public bool IsInside(PointLatLng p)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		int count = base.Points.Count;
		if (count < 3)
		{
			return false;
		}
		bool flag = false;
		int i = 0;
		int index = count - 1;
		for (; i < count; i++)
		{
			PointLatLng val = base.Points[i];
			PointLatLng val2 = base.Points[index];
			if (((((PointLatLng)(ref val)).Lat < ((PointLatLng)(ref p)).Lat && ((PointLatLng)(ref val2)).Lat >= ((PointLatLng)(ref p)).Lat) || (((PointLatLng)(ref val2)).Lat < ((PointLatLng)(ref p)).Lat && ((PointLatLng)(ref val)).Lat >= ((PointLatLng)(ref p)).Lat)) && ((PointLatLng)(ref val)).Lng + (((PointLatLng)(ref p)).Lat - ((PointLatLng)(ref val)).Lat) / (((PointLatLng)(ref val2)).Lat - ((PointLatLng)(ref val)).Lat) * (((PointLatLng)(ref val2)).Lng - ((PointLatLng)(ref val)).Lng) < ((PointLatLng)(ref p)).Lng)
			{
				flag = !flag;
			}
			index = i;
		}
		return flag;
	}

	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		((MapRoute)this).GetObjectData(info, context);
		info.AddValue("LocalPoints", LocalPoints.ToArray());
		info.AddValue("Visible", IsVisible);
	}

	protected GMapPolygon(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		deserializedLocalPoints = Extensions.GetValue<GPoint[]>(info, "LocalPoints");
		IsVisible = Extensions.GetStruct<bool>(info, "Visible", true);
	}

	public override void OnDeserialization(object sender)
	{
		((MapRoute)this).OnDeserialization(sender);
		LocalPoints.AddRange(deserializedLocalPoints);
		LocalPoints.Capacity = base.Points.Count;
	}

	public virtual void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			LocalPoints.Clear();
			if (_graphicsPath != null)
			{
				_graphicsPath.Dispose();
				_graphicsPath = null;
			}
			((MapRoute)this).Clear();
		}
	}
}
