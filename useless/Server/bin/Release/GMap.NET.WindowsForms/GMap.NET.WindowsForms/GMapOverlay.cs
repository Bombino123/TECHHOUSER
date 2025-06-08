using System;
using System.Drawing;
using System.Runtime.Serialization;
using System.Windows.Forms;
using GMap.NET.ObjectModel;

namespace GMap.NET.WindowsForms;

[Serializable]
public class GMapOverlay : ISerializable, IDeserializationCallback, IDisposable
{
	private bool _isVisibile = true;

	private bool _isHitTestVisible = true;

	private bool _isZoomSignificant = true;

	public string Id;

	public readonly ObservableCollectionThreadSafe<GMapMarker> Markers = new ObservableCollectionThreadSafe<GMapMarker>();

	public readonly ObservableCollectionThreadSafe<GMapRoute> Routes = new ObservableCollectionThreadSafe<GMapRoute>();

	public readonly ObservableCollectionThreadSafe<GMapPolygon> Polygons = new ObservableCollectionThreadSafe<GMapPolygon>();

	private GMapControl _control;

	private GMapMarker[] deserializedMarkerArray;

	private GMapRoute[] deserializedRouteArray;

	private GMapPolygon[] deserializedPolygonArray;

	private bool _disposed;

	public bool IsVisibile
	{
		get
		{
			return _isVisibile;
		}
		set
		{
			if (value == _isVisibile)
			{
				return;
			}
			_isVisibile = value;
			if (Control == null)
			{
				return;
			}
			if (_isVisibile)
			{
				Control.HoldInvalidation = true;
				ForceUpdate();
				((Control)Control).Refresh();
				return;
			}
			if (Control.IsMouseOverMarker)
			{
				Control.IsMouseOverMarker = false;
			}
			if (Control.IsMouseOverPolygon)
			{
				Control.IsMouseOverPolygon = false;
			}
			if (Control.IsMouseOverRoute)
			{
				Control.IsMouseOverRoute = false;
			}
			Control.RestoreCursorOnLeave();
			if (!Control.HoldInvalidation)
			{
				Control.Invalidate();
			}
		}
	}

	public bool IsHitTestVisible
	{
		get
		{
			return _isHitTestVisible;
		}
		set
		{
			_isHitTestVisible = value;
		}
	}

	public bool IsZoomSignificant
	{
		get
		{
			return _isZoomSignificant;
		}
		set
		{
			_isZoomSignificant = value;
		}
	}

	public GMapControl Control
	{
		get
		{
			return _control;
		}
		internal set
		{
			_control = value;
		}
	}

	public GMapOverlay()
	{
		CreateEvents();
	}

	public GMapOverlay(string id)
	{
		Id = id;
		CreateEvents();
	}

	private void CreateEvents()
	{
		Markers.CollectionChanged += Markers_CollectionChanged;
		Routes.CollectionChanged += Routes_CollectionChanged;
		Polygons.CollectionChanged += Polygons_CollectionChanged;
	}

	private void ClearEvents()
	{
		Markers.CollectionChanged -= Markers_CollectionChanged;
		Routes.CollectionChanged -= Routes_CollectionChanged;
		Polygons.CollectionChanged -= Polygons_CollectionChanged;
	}

	public void Clear()
	{
		Markers.Clear();
		Routes.Clear();
		Polygons.Clear();
	}

	private void Polygons_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		if (e.NewItems != null)
		{
			foreach (GMapPolygon newItem in e.NewItems)
			{
				if (newItem != null)
				{
					newItem.Overlay = this;
					if (Control != null)
					{
						Control.UpdatePolygonLocalPosition(newItem);
					}
				}
			}
		}
		if (Control != null)
		{
			if ((e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Reset) && Control.IsMouseOverPolygon)
			{
				Control.IsMouseOverPolygon = false;
				Control.RestoreCursorOnLeave();
			}
			if (!Control.HoldInvalidation)
			{
				Control.Invalidate();
			}
		}
	}

	private void Routes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		if (e.NewItems != null)
		{
			foreach (GMapRoute newItem in e.NewItems)
			{
				if (newItem != null)
				{
					newItem.Overlay = this;
					if (Control != null)
					{
						Control.UpdateRouteLocalPosition(newItem);
					}
				}
			}
		}
		if (Control != null)
		{
			if ((e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Reset) && Control.IsMouseOverRoute)
			{
				Control.IsMouseOverRoute = false;
				Control.RestoreCursorOnLeave();
			}
			if (!Control.HoldInvalidation)
			{
				Control.Invalidate();
			}
		}
	}

	private void Markers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		if (e.NewItems != null)
		{
			foreach (GMapMarker newItem in e.NewItems)
			{
				if (newItem != null)
				{
					newItem.Overlay = this;
					if (Control != null)
					{
						Control.UpdateMarkerLocalPosition(newItem);
					}
				}
			}
		}
		if (Control != null)
		{
			if ((e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Reset) && Control.IsMouseOverMarker)
			{
				Control.IsMouseOverMarker = false;
				Control.RestoreCursorOnLeave();
			}
			if (!Control.HoldInvalidation)
			{
				Control.Invalidate();
			}
		}
	}

	internal void ForceUpdate()
	{
		if (Control == null)
		{
			return;
		}
		foreach (GMapMarker marker in Markers)
		{
			if (marker.IsVisible)
			{
				Control.UpdateMarkerLocalPosition(marker);
			}
		}
		foreach (GMapPolygon polygon in Polygons)
		{
			if (polygon.IsVisible)
			{
				Control.UpdatePolygonLocalPosition(polygon);
			}
		}
		foreach (GMapRoute route in Routes)
		{
			if (route.IsVisible)
			{
				Control.UpdateRouteLocalPosition(route);
			}
		}
	}

	public virtual void OnRender(Graphics g)
	{
		if (Control == null)
		{
			return;
		}
		if (Control.RoutesEnabled)
		{
			foreach (GMapRoute route in Routes)
			{
				if (route.IsVisible)
				{
					route.OnRender(g);
				}
			}
		}
		if (Control.PolygonsEnabled)
		{
			foreach (GMapPolygon polygon in Polygons)
			{
				if (polygon.IsVisible)
				{
					polygon.OnRender(g);
				}
			}
		}
		if (!Control.MarkersEnabled)
		{
			return;
		}
		foreach (GMapMarker marker in Markers)
		{
			if (marker.IsVisible || marker.DisableRegionCheck)
			{
				marker.OnRender(g);
			}
		}
	}

	public virtual void OnRenderToolTips(Graphics g)
	{
		if (Control == null || !Control.MarkersEnabled)
		{
			return;
		}
		foreach (GMapMarker marker in Markers)
		{
			if (marker.ToolTip != null && marker.IsVisible && !string.IsNullOrEmpty(marker.ToolTipText) && (marker.ToolTipMode == MarkerTooltipMode.Always || (marker.ToolTipMode == MarkerTooltipMode.OnMouseOver && marker.IsMouseOver)))
			{
				marker.ToolTip.OnRender(g);
			}
		}
	}

	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("Id", Id);
		info.AddValue("IsVisible", IsVisibile);
		GMapMarker[] array = new GMapMarker[Markers.Count];
		Markers.CopyTo(array, 0);
		info.AddValue("Markers", array);
		GMapRoute[] array2 = new GMapRoute[Routes.Count];
		Routes.CopyTo(array2, 0);
		info.AddValue("Routes", array2);
		GMapPolygon[] array3 = new GMapPolygon[Polygons.Count];
		Polygons.CopyTo(array3, 0);
		info.AddValue("Polygons", array3);
	}

	protected GMapOverlay(SerializationInfo info, StreamingContext context)
	{
		Id = info.GetString("Id");
		IsVisibile = info.GetBoolean("IsVisible");
		deserializedMarkerArray = Extensions.GetValue<GMapMarker[]>(info, "Markers", new GMapMarker[0]);
		deserializedRouteArray = Extensions.GetValue<GMapRoute[]>(info, "Routes", new GMapRoute[0]);
		deserializedPolygonArray = Extensions.GetValue<GMapPolygon[]>(info, "Polygons", new GMapPolygon[0]);
		CreateEvents();
	}

	public void OnDeserialization(object sender)
	{
		GMapMarker[] array = deserializedMarkerArray;
		foreach (GMapMarker gMapMarker in array)
		{
			gMapMarker.Overlay = this;
			Markers.Add(gMapMarker);
		}
		GMapRoute[] array2 = deserializedRouteArray;
		foreach (GMapRoute gMapRoute in array2)
		{
			gMapRoute.Overlay = this;
			Routes.Add(gMapRoute);
		}
		GMapPolygon[] array3 = deserializedPolygonArray;
		foreach (GMapPolygon gMapPolygon in array3)
		{
			gMapPolygon.Overlay = this;
			Polygons.Add(gMapPolygon);
		}
	}

	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}
		_disposed = true;
		ClearEvents();
		foreach (GMapMarker marker in Markers)
		{
			marker.Dispose();
		}
		foreach (GMapRoute route in Routes)
		{
			route.Dispose();
		}
		foreach (GMapPolygon polygon in Polygons)
		{
			polygon.Dispose();
		}
		Clear();
	}
}
