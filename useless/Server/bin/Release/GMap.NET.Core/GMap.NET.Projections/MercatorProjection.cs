using System;

namespace GMap.NET.Projections;

public class MercatorProjection : PureProjection
{
	public static readonly MercatorProjection Instance = new MercatorProjection();

	private static readonly double MinLatitude = -85.05112878;

	private static readonly double MaxLatitude = 85.05112878;

	private static readonly double MinLongitude = -180.0;

	private static readonly double MaxLongitude = 180.0;

	public override RectLatLng Bounds => RectLatLng.FromLTRB(MinLongitude, MaxLatitude, MaxLongitude, MinLatitude);

	public override GSize TileSize { get; } = new GSize(256L, 256L);


	public override double Axis => 6378137.0;

	public override double Flattening => 0.0033528106647474805;

	public override GPoint FromLatLngToPixel(double lat, double lng, int zoom)
	{
		GPoint empty = GPoint.Empty;
		lat = PureProjection.Clip(lat, MinLatitude, MaxLatitude);
		lng = PureProjection.Clip(lng, MinLongitude, MaxLongitude);
		double num = (lng + 180.0) / 360.0;
		double num2 = Math.Sin(lat * Math.PI / 180.0);
		double num3 = 0.5 - Math.Log((1.0 + num2) / (1.0 - num2)) / (Math.PI * 4.0);
		GSize tileMatrixSizePixel = GetTileMatrixSizePixel(zoom);
		long width = tileMatrixSizePixel.Width;
		long height = tileMatrixSizePixel.Height;
		empty.X = (long)PureProjection.Clip(num * (double)width + 0.5, 0.0, width - 1);
		empty.Y = (long)PureProjection.Clip(num3 * (double)height + 0.5, 0.0, height - 1);
		return empty;
	}

	public override PointLatLng FromPixelToLatLng(long x, long y, int zoom)
	{
		PointLatLng empty = PointLatLng.Empty;
		GSize tileMatrixSizePixel = GetTileMatrixSizePixel(zoom);
		double num = tileMatrixSizePixel.Width;
		double num2 = tileMatrixSizePixel.Height;
		double num3 = PureProjection.Clip(x, 0.0, num - 1.0) / num - 0.5;
		double num4 = 0.5 - PureProjection.Clip(y, 0.0, num2 - 1.0) / num2;
		empty.Lat = 90.0 - 360.0 * Math.Atan(Math.Exp((0.0 - num4) * 2.0 * Math.PI)) / Math.PI;
		empty.Lng = 360.0 * num3;
		return empty;
	}

	public override GSize GetTileMatrixMinXY(int zoom)
	{
		return new GSize(0L, 0L);
	}

	public override GSize GetTileMatrixMaxXY(int zoom)
	{
		long num = 1 << zoom;
		return new GSize(num - 1, num - 1);
	}
}
