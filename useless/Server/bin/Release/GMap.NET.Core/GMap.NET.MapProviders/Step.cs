namespace GMap.NET.MapProviders;

public class Step
{
	public Distance2 distance { get; set; }

	public Duration2 duration { get; set; }

	public EndLocation2 end_location { get; set; }

	public string html_instructions { get; set; }

	public Polyline polyline { get; set; }

	public StartLocation2 start_location { get; set; }

	public string travel_mode { get; set; }

	public string maneuver { get; set; }
}
