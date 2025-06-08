using System.Collections.Generic;
using Newtonsoft.Json;

namespace GMap.NET.Entity;

public class OpenStreetMapGeocodeEntity
{
	public class Address
	{
		public string road { get; set; }

		public string suburb { get; set; }

		public string city { get; set; }

		public string municipality { get; set; }

		public string county { get; set; }

		public string state { get; set; }

		[JsonProperty("ISO3166-2-lvl4")]
		public string ISO31662Lvl4 { get; set; }

		public string postcode { get; set; }

		public string country { get; set; }

		public string country_code { get; set; }
	}

	public long place_id { get; set; }

	public string licence { get; set; }

	public string osm_type { get; set; }

	public long osm_id { get; set; }

	public double lat { get; set; }

	public double lon { get; set; }

	public string display_name { get; set; }

	public Address address { get; set; }

	public string @class { get; set; }

	public string type { get; set; }

	public double importance { get; set; }

	public List<string> boundingbox { get; set; }
}
