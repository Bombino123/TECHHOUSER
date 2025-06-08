using System.Collections.Generic;

namespace GMap.NET.MapProviders;

public class Route
{
	public Bounds bounds { get; set; }

	public string copyrights { get; set; }

	public List<Leg> legs { get; set; }

	public OverviewPolyline overview_polyline { get; set; }

	public string summary { get; set; }

	public List<object> warnings { get; set; }

	public List<object> waypoint_order { get; set; }
}
