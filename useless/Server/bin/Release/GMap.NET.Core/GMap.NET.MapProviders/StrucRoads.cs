using System.Collections.Generic;

namespace GMap.NET.MapProviders;

public class StrucRoads
{
	public class SnappedPoint
	{
		public class Location
		{
			public double latitude { get; set; }

			public double longitude { get; set; }
		}

		public Location location { get; set; }

		public int originalIndex { get; set; }

		public string placeId { get; set; }
	}

	public Error error { get; set; }

	public string warningMessage { get; set; }

	public List<SnappedPoint> snappedPoints { get; set; }
}
