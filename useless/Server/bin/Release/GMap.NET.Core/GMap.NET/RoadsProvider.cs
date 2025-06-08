using System.Collections.Generic;

namespace GMap.NET;

public interface RoadsProvider
{
	MapRoute GetRoadsRoute(List<PointLatLng> points, bool interpolate);

	MapRoute GetRoadsRoute(string points, bool interpolate);
}
