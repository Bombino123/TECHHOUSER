using System.Collections.Generic;

namespace GMap.NET;

public struct GDirectionStep
{
	public string TravelMode;

	public PointLatLng StartLocation;

	public PointLatLng EndLocation;

	public string Duration;

	public string Distance;

	public string HtmlInstructions;

	public List<PointLatLng> Points;

	public override string ToString()
	{
		return TravelMode + " | " + Distance + " | " + Duration + " | " + HtmlInstructions;
	}
}
