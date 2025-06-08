using System.Collections.Generic;

namespace GMap.NET.MapProviders;

public class StrucRute
{
	public List<GeocodedWaypoint> geocoded_waypoints { get; set; }

	public List<Route> routes { get; set; }

	public RouteStatusCode status { get; set; }

	public Error error { get; set; }
}
