using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Serialization;

namespace GMap.NET.WindowsForms;

[Serializable]
public class GMapRoute : MapRoute, ISerializable, IDeserializationCallback, IDisposable
{
	private GMapOverlay _overlay;

	private bool _visible = true;

	public bool IsHitTestVisible;

	private bool _isMouseOver;

	private GraphicsPath _graphicsPath;

	public static readonly Pen DefaultStroke;

	[NonSerialized]
	public Pen Stroke = DefaultStroke;

	public readonly List<GPoint> LocalPoints = new List<GPoint>();

	private GPoint[] deserializedLocalPoints;

	private bool _disposed;

	public GMapOverlay Overlay
	{
		get
		{
			return _overlay;
		}
		internal set
		{
			_overlay = value;
		}
	}

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
					Overlay.Control.UpdateRouteLocalPosition(this);
				}
				else if (Overlay.Control.IsMouseOverRoute)
				{
					Overlay.Control.IsMouseOverRoute = false;
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

	internal bool IsInside(int x, int y)
	{
		if (_graphicsPath != null)
		{
			return _graphicsPath.IsOutlineVisible(x, y, Stroke);
		}
		return false;
	}

	internal void UpdateGraphicsPath()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (_graphicsPath == null)
		{
			_graphicsPath = new GraphicsPath();
		}
		else
		{
			_graphicsPath.Reset();
		}
		for (int i = 0; i < LocalPoints.Count; i++)
		{
			GPoint val = LocalPoints[i];
			if (i == 0)
			{
				_graphicsPath.AddLine((float)((GPoint)(ref val)).X, (float)((GPoint)(ref val)).Y, (float)((GPoint)(ref val)).X, (float)((GPoint)(ref val)).Y);
				continue;
			}
			PointF lastPoint = _graphicsPath.GetLastPoint();
			_graphicsPath.AddLine(lastPoint.X, lastPoint.Y, (float)((GPoint)(ref val)).X, (float)((GPoint)(ref val)).Y);
		}
	}

	public virtual void OnRender(Graphics g)
	{
		if (IsVisible && _graphicsPath != null)
		{
			g.DrawPath(Stroke, _graphicsPath);
		}
	}

	static GMapRoute()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		DefaultStroke = new Pen(Color.FromArgb(144, Color.MidnightBlue));
		DefaultStroke.LineJoin = (LineJoin)2;
		DefaultStroke.Width = 5f;
	}

	public GMapRoute(string name)
		: base(name)
	{
	}

	public GMapRoute(IEnumerable<PointLatLng> points, string name)
		: base(points, name)
	{
	}

	public GMapRoute(MapRoute oRoute)
		: base(oRoute)
	{
	}

	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		((MapRoute)this).GetObjectData(info, context);
		info.AddValue("Visible", IsVisible);
		info.AddValue("LocalPoints", LocalPoints.ToArray());
	}

	protected GMapRoute(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		IsVisible = Extensions.GetStruct<bool>(info, "Visible", true);
		deserializedLocalPoints = Extensions.GetValue<GPoint[]>(info, "LocalPoints");
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
