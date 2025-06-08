using System;

namespace GMap.NET.Projections;

internal class MercatorProjectionYandex : PureProjection
{
	public static readonly MercatorProjectionYandex Instance = new MercatorProjectionYandex();

	private static readonly double MinLatitude = -85.05112878;

	private static readonly double MaxLatitude = 85.05112878;

	private static readonly double MinLongitude = -177.0;

	private static readonly double MaxLongitude = 177.0;

	private static readonly double RAD_DEG = 180.0 / Math.PI;

	private static readonly double DEG_RAD = Math.PI / 180.0;

	private static readonly double MathPiDiv4 = Math.PI / 4.0;

	public override RectLatLng Bounds => RectLatLng.FromLTRB(MinLongitude, MaxLatitude, MaxLongitude, MinLatitude);

	public override GSize TileSize { get; } = new GSize(256L, 256L);


	public override double Axis => 6356752.3142;

	public override double Flattening => 0.0033528106647474805;

	public override GPoint FromLatLngToPixel(double lat, double lng, int zoom)
	{
		lat = PureProjection.Clip(lat, MinLatitude, MaxLatitude);
		lng = PureProjection.Clip(lng, MinLongitude, MaxLongitude);
		double num = lng * DEG_RAD;
		double num2 = lat * DEG_RAD;
		double num3 = 6378137.0;
		double num4 = 0.0818191908426;
		double d = Math.Tan(MathPiDiv4 + num2 / 2.0) / Math.Pow(Math.Tan(MathPiDiv4 + Math.Asin(num4 * Math.Sin(num2)) / 2.0), num4);
		double num5 = Math.Pow(2.0, 23 - zoom);
		double num6 = (20037508.342789 + num3 * num) * 53.5865938 / num5;
		double num7 = (20037508.342789 - num3 * Math.Log(d)) * 53.5865938 / num5;
		return new GPoint((long)num6, (long)num7);
	}

	public override PointLatLng FromPixelToLatLng(long x, long y, int zoom)
	{
		GSize tileMatrixSizePixel = GetTileMatrixSizePixel(zoom);
		_ = tileMatrixSizePixel.Width;
		_ = tileMatrixSizePixel.Height;
		double num = 6378137.0;
		double num2 = 0.00335655146887969;
		double num3 = 6.57187271079536E-06;
		double num4 = 1.764564338702E-08;
		double num5 = 5.328478445E-11;
		double y2 = 23 - zoom;
		double num6 = (double)x * Math.Pow(2.0, y2) / 53.5865938 - 20037508.342789;
		double num7 = 20037508.342789 - (double)y * Math.Pow(2.0, y2) / 53.5865938;
		double num8 = Math.PI / 2.0 - 2.0 * Math.Atan(1.0 / Math.Exp(num7 / num));
		double num9 = num8 + num2 * Math.Sin(2.0 * num8) + num3 * Math.Sin(4.0 * num8) + num4 * Math.Sin(6.0 * num8) + num5 * Math.Sin(8.0 * num8);
		PointLatLng empty = PointLatLng.Empty;
		empty.Lat = num9 * RAD_DEG;
		empty.Lng = num6 / num * RAD_DEG;
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
