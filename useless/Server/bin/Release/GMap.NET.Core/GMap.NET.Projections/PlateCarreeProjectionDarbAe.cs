using System;

namespace GMap.NET.Projections;

public class PlateCarreeProjectionDarbAe : PureProjection
{
	public static readonly PlateCarreeProjectionDarbAe Instance = new PlateCarreeProjectionDarbAe();

	public static readonly double MinLatitude = 18.7071563263201;

	public static readonly double MaxLatitude = 29.4052130085331;

	public static readonly double MinLongitude = 41.522866508209;

	public static readonly double MaxLongitude = 66.2882966568906;

	private static readonly double orignX = -400.0;

	private static readonly double orignY = 400.0;

	public override RectLatLng Bounds => RectLatLng.FromLTRB(MinLongitude, MaxLatitude, MaxLongitude, MinLatitude);

	public override GSize TileSize { get; } = new GSize(256L, 256L);


	public override double Axis => 6378137.0;

	public override double Flattening => 0.0033528106647474805;

	public override GPoint FromLatLngToPixel(double lat, double lng, int zoom)
	{
		GPoint empty = GPoint.Empty;
		lat = PureProjection.Clip(lat, MinLatitude, MaxLatitude);
		lng = PureProjection.Clip(lng, MinLongitude, MaxLongitude);
		double tileMatrixResolution = GetTileMatrixResolution(zoom);
		empty.X = (long)Math.Floor((lng - orignX) / tileMatrixResolution);
		empty.Y = (long)Math.Floor((orignY - lat) / tileMatrixResolution);
		return empty;
	}

	public override PointLatLng FromPixelToLatLng(long x, long y, int zoom)
	{
		PointLatLng empty = PointLatLng.Empty;
		double tileMatrixResolution = GetTileMatrixResolution(zoom);
		empty.Lat = orignY - (double)y * tileMatrixResolution;
		empty.Lng = (double)x * tileMatrixResolution + orignX;
		return empty;
	}

	public static double GetTileMatrixResolution(int zoom)
	{
		double result = 0.0;
		switch (zoom)
		{
		case 0:
			result = 0.0118973050291514;
			break;
		case 1:
			result = 0.0059486525145757;
			break;
		case 2:
			result = 0.00297432625728785;
			break;
		case 3:
			result = 0.00118973050291514;
			break;
		case 4:
			result = 0.00059486525145757;
			break;
		case 5:
			result = 0.000356919150874542;
			break;
		case 6:
			result = 0.000178459575437271;
			break;
		case 7:
			result = 0.000118973050291514;
			break;
		case 8:
			result = 5.9486525145757E-05;
			break;
		case 9:
			result = 3.56919150874542E-05;
			break;
		case 10:
			result = 1.90356880466422E-05;
			break;
		case 11:
			result = 9.51784402332112E-06;
			break;
		case 12:
			result = 4.75892201166056E-06;
			break;
		}
		return result;
	}

	public override double GetGroundResolution(int zoom, double latitude)
	{
		return GetTileMatrixResolution(zoom);
	}

	public override GSize GetTileMatrixMaxXY(int zoom)
	{
		GPoint p = FromLatLngToPixel(MinLatitude, MaxLongitude, zoom);
		return new GSize(FromPixelToTileXY(p));
	}

	public override GSize GetTileMatrixMinXY(int zoom)
	{
		GPoint p = FromLatLngToPixel(MaxLatitude, MinLongitude, zoom);
		return new GSize(FromPixelToTileXY(p));
	}
}
