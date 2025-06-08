using System.Collections.Generic;

namespace GMap.NET.MapProviders;

public class StrucGeocode
{
	public List<Result> results { get; set; }

	public GeoCoderStatusCode status { get; set; }
}
