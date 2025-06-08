using System.Collections.Generic;

namespace GMap.NET.MapProviders;

public class StrucDirection
{
	public List<GeocodedWaypoint> geocoded_waypoints { get; set; }

	public List<Route> routes { get; set; }

	public DirectionsStatusCode status { get; set; }
}
