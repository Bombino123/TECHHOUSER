using System;

namespace GMap.NET.Projections;

public class PlateCarreeProjection : PureProjection
{
	public static readonly PlateCarreeProjection Instance = new PlateCarreeProjection();

	private static readonly double MinLatitude = -85.05112878;

	private static readonly double MaxLatitude = 85.05112878;

	private static readonly double MinLongitude = -180.0;

	private static readonly double MaxLongitude = 180.0;

	public override RectLatLng Bounds => RectLatLng.FromLTRB(MinLongitude, MaxLatitude, MaxLongitude, MinLatitude);

	public override GSize TileSize { get; } = new GSize(512L, 512L);


	public override double Axis => 6378137.0;

	public override double Flattening => 0.0033528106647474805;

	public override GPoint FromLatLngToPixel(double lat, double lng, int zoom)
	{
		GPoint empty = GPoint.Empty;
		lat = PureProjection.Clip(lat, MinLatitude, MaxLatitude);
		lng = PureProjection.Clip(lng, MinLongitude, MaxLongitude);
		GSize tileMatrixSizePixel = GetTileMatrixSizePixel(zoom);
		double num = tileMatrixSizePixel.Width;
		_ = tileMatrixSizePixel.Height;
		double num2 = 360.0 / num;
		empty.Y = (long)((90.0 - lat) / num2);
		empty.X = (long)((lng + 180.0) / num2);
		return empty;
	}

	public override PointLatLng FromPixelToLatLng(long x, long y, int zoom)
	{
		PointLatLng empty = PointLatLng.Empty;
		GSize tileMatrixSizePixel = GetTileMatrixSizePixel(zoom);
		double num = tileMatrixSizePixel.Width;
		_ = tileMatrixSizePixel.Height;
		double num2 = 360.0 / num;
		empty.Lat = 90.0 - (double)y * num2;
		empty.Lng = (double)x * num2 - 180.0;
		return empty;
	}

	public override GSize GetTileMatrixMaxXY(int zoom)
	{
		long num = (long)Math.Pow(2.0, zoom);
		return new GSize(2 * num - 1, num - 1);
	}

	public override GSize GetTileMatrixMinXY(int zoom)
	{
		return new GSize(0L, 0L);
	}
}
