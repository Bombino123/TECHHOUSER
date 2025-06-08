using System;
using System.Drawing;
using System.Runtime.Serialization;
using GMap.NET.WindowsForms.ToolTips;

namespace GMap.NET.WindowsForms;

[Serializable]
public abstract class GMapMarker : ISerializable, IDisposable
{
	private GMapOverlay _overlay;

	private PointLatLng _position;

	public object Tag;

	private Point _offset;

	private Rectangle _area;

	public GMapToolTip ToolTip;

	public MarkerTooltipMode ToolTipMode;

	private string _toolTipText;

	private bool _visible = true;

	public bool DisableRegionCheck;

	public bool IsHitTestVisible = true;

	private bool _isMouseOver;

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

	public PointLatLng Position
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _position;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			if (_position != value)
			{
				_position = value;
				if (IsVisible && Overlay != null && Overlay.Control != null)
				{
					Overlay.Control.UpdateMarkerLocalPosition(this);
				}
			}
		}
	}

	public Point Offset
	{
		get
		{
			return _offset;
		}
		set
		{
			if (_offset != value)
			{
				_offset = value;
				if (IsVisible && Overlay != null && Overlay.Control != null)
				{
					Overlay.Control.UpdateMarkerLocalPosition(this);
				}
			}
		}
	}

	public Point LocalPosition
	{
		get
		{
			return _area.Location;
		}
		set
		{
			if (_area.Location != value)
			{
				_area.Location = value;
				if (Overlay != null && Overlay.Control != null && !Overlay.Control.HoldInvalidation)
				{
					Overlay.Control.Invalidate();
				}
			}
		}
	}

	public Point ToolTipPosition
	{
		get
		{
			Point location = _area.Location;
			location.Offset(-Offset.X, -Offset.Y);
			return location;
		}
	}

	public Size Size
	{
		get
		{
			return _area.Size;
		}
		set
		{
			_area.Size = value;
		}
	}

	public Rectangle LocalArea => _area;

	public string ToolTipText
	{
		get
		{
			return _toolTipText;
		}
		set
		{
			if (ToolTip == null && !string.IsNullOrEmpty(value))
			{
				ToolTip = new GMapRoundedToolTip(this);
			}
			_toolTipText = value;
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
					Overlay.Control.UpdateMarkerLocalPosition(this);
				}
				else if (Overlay.Control.IsMouseOverMarker)
				{
					Overlay.Control.IsMouseOverMarker = false;
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

	public GMapMarker(PointLatLng pos)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		Position = pos;
	}

	public virtual void OnRender(Graphics g)
	{
	}

	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		info.AddValue("Position", Position);
		info.AddValue("Tag", Tag);
		info.AddValue("Offset", Offset);
		info.AddValue("Area", _area);
		info.AddValue("ToolTip", ToolTip);
		info.AddValue("ToolTipMode", ToolTipMode);
		info.AddValue("ToolTipText", ToolTipText);
		info.AddValue("Visible", IsVisible);
		info.AddValue("DisableregionCheck", DisableRegionCheck);
		info.AddValue("IsHitTestVisible", IsHitTestVisible);
	}

	protected GMapMarker(SerializationInfo info, StreamingContext context)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		Position = Extensions.GetStruct<PointLatLng>(info, "Position", PointLatLng.Empty);
		Tag = Extensions.GetValue<object>(info, "Tag", (object)null);
		Offset = Extensions.GetStruct<Point>(info, "Offset", Point.Empty);
		_area = Extensions.GetStruct<Rectangle>(info, "Area", Rectangle.Empty);
		ToolTip = Extensions.GetValue<GMapToolTip>(info, "ToolTip", (GMapToolTip)null);
		if (ToolTip != null)
		{
			ToolTip.Marker = this;
		}
		ToolTipMode = Extensions.GetStruct<MarkerTooltipMode>(info, "ToolTipMode", MarkerTooltipMode.OnMouseOver);
		ToolTipText = info.GetString("ToolTipText");
		IsVisible = info.GetBoolean("Visible");
		DisableRegionCheck = info.GetBoolean("DisableregionCheck");
		IsHitTestVisible = info.GetBoolean("IsHitTestVisible");
	}

	public virtual void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			Tag = null;
			if (ToolTip != null)
			{
				_toolTipText = null;
				ToolTip.Dispose();
				ToolTip = null;
			}
		}
	}
}
