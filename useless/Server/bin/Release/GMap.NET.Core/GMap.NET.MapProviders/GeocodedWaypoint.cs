using System.Collections.Generic;

namespace GMap.NET.MapProviders;

public class GeocodedWaypoint
{
	public string geocoder_status { get; set; }

	public string place_id { get; set; }

	public List<string> types { get; set; }
}
