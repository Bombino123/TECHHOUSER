namespace GMap.NET;

public interface RoutingProvider
{
	MapRoute GetRoute(PointLatLng start, PointLatLng end, bool avoidHighways, bool walkingMode, int zoom);

	MapRoute GetRoute(string start, string end, bool avoidHighways, bool walkingMode, int zoom);
}
