using System;

namespace GMap.NET;

public struct GpsLog
{
	public DateTime TimeUTC;

	public long SessionCounter;

	public double? Delta;

	public double? Speed;

	public double? SeaLevelAltitude;

	public double? EllipsoidAltitude;

	public short? SatellitesInView;

	public short? SatelliteCount;

	public PointLatLng Position;

	public double? PositionDilutionOfPrecision;

	public double? HorizontalDilutionOfPrecision;

	public double? VerticalDilutionOfPrecision;

	public FixQuality FixQuality;

	public FixType FixType;

	public FixSelection FixSelection;

	public override string ToString()
	{
		return $"{SessionCounter}: {TimeUTC}";
	}
}
