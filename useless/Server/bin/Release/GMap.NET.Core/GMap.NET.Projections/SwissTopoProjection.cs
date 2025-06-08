using System;

namespace GMap.NET.Projections;

public class SwissTopoProjection : PureProjection
{
	public static readonly SwissTopoProjection Instance = new SwissTopoProjection();

	private static readonly double MaxLatitude = 85.05112878;

	private static readonly double MaxLongitude = 180.0;

	private static readonly double MinLatitude = -85.05112878;

	private static readonly double MinLongitude = -180.0;

	private readonly GSize tileSize = new GSize(256L, 256L);

	private static GSize[] TileMaxLimitsPerZoom = new GSize[29]
	{
		new GSize(1L, 1L),
		new GSize(1L, 1L),
		new GSize(1L, 1L),
		new GSize(1L, 1L),
		new GSize(1L, 1L),
		new GSize(1L, 1L),
		new GSize(1L, 1L),
		new GSize(1L, 1L),
		new GSize(1L, 1L),
		new GSize(2L, 1L),
		new GSize(2L, 1L),
		new GSize(2L, 1L),
		new GSize(2L, 2L),
		new GSize(3L, 2L),
		new GSize(3L, 2L),
		new GSize(4L, 3L),
		new GSize(8L, 5L),
		new GSize(19L, 13L),
		new GSize(38L, 25L),
		new GSize(94L, 63L),
		new GSize(188L, 125L),
		new GSize(375L, 250L),
		new GSize(750L, 500L),
		new GSize(938L, 625L),
		new GSize(1250L, 834L),
		new GSize(1875L, 1250L),
		new GSize(3750L, 2500L),
		new GSize(7500L, 5000L),
		new GSize(18750L, 12500L)
	};

	public override double Axis => 6378137.0;

	public override RectLatLng Bounds => RectLatLng.FromLTRB(MinLongitude, MaxLatitude, MaxLongitude, MinLatitude);

	public override double Flattening => 0.0033528106647474805;

	public override GSize TileSize => tileSize;

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

	public override GSize GetTileMatrixMaxXY(int zoom)
	{
		return new GSize(TileMaxLimitsPerZoom[zoom].Width - 1, TileMaxLimitsPerZoom[zoom].Height - 1);
	}

	public override GSize GetTileMatrixMinXY(int zoom)
	{
		return new GSize(0L, 0L);
	}
}
