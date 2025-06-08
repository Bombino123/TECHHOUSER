using System.Collections.Generic;

namespace GMap.NET;

public class GDirections
{
	public string Summary;

	public string Duration;

	public uint DurationValue;

	public string Distance;

	public uint DistanceValue;

	public PointLatLng StartLocation;

	public PointLatLng EndLocation;

	public string StartAddress;

	public string EndAddress;

	public string Copyrights;

	public List<GDirectionStep> Steps;

	public List<PointLatLng> Route;

	public override string ToString()
	{
		return Summary + " | " + Distance + " | " + Duration;
	}
}
