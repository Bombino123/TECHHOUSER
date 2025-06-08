using System.Collections.Generic;

namespace GMap.NET.Entity;

public class OpenStreetMapGraphHopperGeocodeEntity
{
	public class Hit
	{
		public Point point { get; set; }

		public List<double> extent { get; set; }

		public string name { get; set; }

		public string country { get; set; }

		public string countrycode { get; set; }

		public string city { get; set; }

		public string state { get; set; }

		public long osm_id { get; set; }

		public string osm_type { get; set; }

		public string osm_key { get; set; }

		public string osm_value { get; set; }

		public string postcode { get; set; }
	}

	public class Point
	{
		public double lat { get; set; }

		public double lng { get; set; }
	}

	public List<Hit> hits { get; set; }

	public string locale { get; set; }
}
