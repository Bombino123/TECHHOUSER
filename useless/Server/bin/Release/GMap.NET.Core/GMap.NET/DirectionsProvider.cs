using System.Collections.Generic;

namespace GMap.NET;

public interface DirectionsProvider
{
	DirectionsStatusCode GetDirections(out GDirections direction, PointLatLng start, PointLatLng end, bool avoidHighways, bool avoidTolls, bool walkingMode, bool sensor, bool metric);

	DirectionsStatusCode GetDirections(out GDirections direction, string start, string end, bool avoidHighways, bool avoidTolls, bool walkingMode, bool sensor, bool metric);

	IEnumerable<GDirections> GetDirections(out DirectionsStatusCode status, string start, string end, bool avoidHighways, bool avoidTolls, bool walkingMode, bool sensor, bool metric);

	IEnumerable<GDirections> GetDirections(out DirectionsStatusCode status, PointLatLng start, PointLatLng end, bool avoidHighways, bool avoidTolls, bool walkingMode, bool sensor, bool metric);

	DirectionsStatusCode GetDirections(out GDirections direction, PointLatLng start, IEnumerable<PointLatLng> wayPoints, PointLatLng end, bool avoidHighways, bool avoidTolls, bool walkingMode, bool sensor, bool metric);

	DirectionsStatusCode GetDirections(out GDirections direction, string start, IEnumerable<string> wayPoints, string end, bool avoidHighways, bool avoidTolls, bool walkingMode, bool sensor, bool metric);
}
