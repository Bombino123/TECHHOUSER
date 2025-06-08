using System;

namespace GMap.NET.Projections;

public class MapyCZProjection : PureProjection
{
	public static readonly MapyCZProjection Instance = new MapyCZProjection();

	private static readonly double MinLatitude = 26.0;

	private static readonly double MaxLatitude = 76.0;

	private static readonly double MinLongitude = -26.0;

	private static readonly double MaxLongitude = 38.0;

	private static readonly double UTMSIZE = 2.0;

	private static readonly double UNITS = 1.0;

	public override RectLatLng Bounds => RectLatLng.FromLTRB(MinLongitude, MaxLatitude, MaxLongitude, MinLatitude);

	public override GSize TileSize => new GSize(256L, 256L);

	public override double Axis => 6378137.0;

	public override double Flattening => 0.0033528106647474805;

	private static int GetLCM(int zone)
	{
		if (zone < 1 || zone > 60)
		{
			throw new Exception("MapyCZProjection: UTM Zone number is not between 1 and 60.");
		}
		return zone * 6 - 183;
	}

	private static double Roundoff(double xx, double yy)
	{
		return Math.Round(xx * Math.Pow(10.0, yy)) / Math.Pow(10.0, yy);
	}

	public long[] WGSToPP(double la, double lo)
	{
		double[] array = WGSToUTM(PureProjection.DegreesToRadians(la), PureProjection.DegreesToRadians(lo), 33);
		return UTMEEToPP(array[0], array[1]);
	}

	private static long[] UTMEEToPP(double east, double north)
	{
		double num = (Math.Round(east) - -3700000.0) * Math.Pow(2.0, 5.0);
		double num2 = (Math.Round(north) - 1300000.0) * Math.Pow(2.0, 5.0);
		return new long[2]
		{
			(long)num,
			(long)num2
		};
	}

	private double[] WGSToUTM(double la, double lo, int zone)
	{
		PureProjection.RadiansToDegrees(la);
		double num = PureProjection.RadiansToDegrees(lo);
		float num2 = 0.9996f;
		double axis = Axis;
		double flattening = Flattening;
		double num3 = axis * (1.0 - flattening);
		double num4 = (axis * axis - num3 * num3) / (axis * axis);
		Math.Sqrt(num4);
		Math.Sqrt((axis * axis - num3 * num3) / (num3 * num3));
		double x = (axis - num3) / (axis + num3);
		Math.Pow(x, 4.0);
		double deg = num - (double)(zone * 6 - 183);
		deg = PureProjection.DegreesToRadians(deg);
		double num5 = Math.Tan(la);
		double num6 = axis * (1.0 - num4) / Math.Pow(1.0 - num4 * Math.Sin(la) * Math.Sin(la), 1.5);
		double num7 = axis / Math.Sqrt(1.0 - num4 * Math.Sin(la) * Math.Sin(la));
		double num8 = num7 / num6;
		double num9 = Math.Cos(la);
		double num10 = Math.Sin(la);
		double num11 = 1.0 - num4 / 4.0 - 3.0 * num4 * num4 / 64.0 - 5.0 * Math.Pow(num4, 3.0) / 256.0;
		double num12 = 0.375 * (num4 + num4 * num4 / 4.0 + 15.0 * Math.Pow(num4, 3.0) / 128.0);
		double num13 = 15.0 / 256.0 * (num4 * num4 + 3.0 * Math.Pow(num4, 3.0) / 4.0);
		double num14 = 35.0 * Math.Pow(num4, 3.0) / 3072.0;
		double num15 = axis * (num11 * la - num12 * Math.Sin(2.0 * la) + num13 * Math.Sin(4.0 * la) - num14 * Math.Sin(6.0 * la));
		double num16 = deg * deg / 6.0 * num9 * num9 * (num8 - num5 * num5);
		double num17 = Math.Pow(deg, 4.0) / 120.0 * Math.Pow(num9, 4.0) * (4.0 * Math.Pow(num8, 3.0) * (1.0 - 6.0 * num5 * num5) + num8 * num8 * (1.0 + 8.0 * num5 * num5) - num8 * 2.0 * num5 * num5 + Math.Pow(num5, 4.0));
		double num18 = Math.Pow(deg, 6.0) / 5040.0 * Math.Pow(num9, 6.0) * (61.0 - 479.0 * num5 * num5 + 179.0 * Math.Pow(num5, 4.0) - Math.Pow(num5, 6.0));
		double num19 = (double)num2 * num7 * deg * num9 * (1.0 + num16 + num17 + num18);
		double xx = 500000.0 + num19 / UNITS;
		xx = Roundoff(xx, UTMSIZE);
		double num20 = deg * deg / 2.0 * num7 * num10 * num9;
		double num21 = Math.Pow(deg, 4.0) / 24.0 * num7 * num10 * Math.Pow(num9, 3.0) * (4.0 * num8 * num8 + num8 - num5 * num5);
		double num22 = Math.Pow(deg, 6.0) / 720.0 * num7 * num10 * Math.Pow(num9, 5.0) * (8.0 * Math.Pow(num8, 4.0) * (11.0 - 24.0 * num5 * num5) - 28.0 * Math.Pow(num8, 3.0) * (1.0 - 6.0 * num5 * num5) + num8 * num8 * (1.0 - 32.0 * num5 * num5) - num8 * 2.0 * num5 * num5 + Math.Pow(num5, 4.0));
		double num23 = Math.Pow(deg, 8.0) / 40320.0 * num7 * num10 * Math.Pow(num9, 7.0) * (1385.0 - 3111.0 * num5 * num5 + 543.0 * Math.Pow(num5, 4.0) - Math.Pow(num5, 6.0));
		double num24 = (double)num2 * (num15 + num20 + num21 + num22 + num23);
		double xx2 = 0.0 + num24 / UNITS;
		xx2 = Roundoff(xx2, UTMSIZE);
		return new double[3] { xx, xx2, zone };
	}

	public double[] PPToWGS(double x, double y)
	{
		double[] array = PPToUTMEE(x, y);
		return UTMToWGS(array[0], array[1], 33);
	}

	private double[] PPToUTMEE(double x, double y)
	{
		double xx = y * Math.Pow(2.0, -5.0) + 1300000.0;
		double xx2 = x * Math.Pow(2.0, -5.0) + -3700000.0;
		xx2 = Roundoff(xx2, UTMSIZE);
		xx = Roundoff(xx, UTMSIZE);
		return new double[2] { xx2, xx };
	}

	private double[] UTMToWGS(double eastIn, double northIn, int zone)
	{
		float num = 0.9996f;
		double axis = Axis;
		double flattening = Flattening;
		double num2 = axis * (1.0 - flattening);
		double num3 = (axis * axis - num2 * num2) / (axis * axis);
		Math.Sqrt(num3);
		Math.Sqrt((axis * axis - num2 * num2) / (num2 * num2));
		double num4 = (axis - num2) / (axis + num2);
		double num5 = axis * (1.0 - num4) * (1.0 - num4 * num4) * (1.0 + 2.25 * num4 * num4 + 3.984375 * Math.Pow(num4, 4.0)) * (Math.PI / 180.0);
		double num6 = (northIn - 0.0) * UNITS;
		double num7 = (eastIn - 500000.0) * UNITS;
		double num8 = num6 / (double)num * Math.PI / (180.0 * num5);
		double num9 = num8 + (3.0 * num4 / 2.0 - 27.0 * Math.Pow(num4, 3.0) / 32.0) * Math.Sin(2.0 * num8) + (21.0 * num4 * num4 / 16.0 - 55.0 * Math.Pow(num4, 4.0) / 32.0) * Math.Sin(4.0 * num8) + 151.0 * Math.Pow(num4, 3.0) / 96.0 * Math.Sin(6.0 * num8) + 1097.0 * Math.Pow(num4, 4.0) / 512.0 * Math.Sin(8.0 * num8);
		double num10 = axis * (1.0 - num3) / Math.Pow(1.0 - num3 * Math.Sin(num9) * Math.Sin(num9), 1.5);
		double num11 = axis / Math.Sqrt(1.0 - num3 * Math.Sin(num9) * Math.Sin(num9));
		double num12 = num11 / num10;
		double num13 = Math.Tan(num9);
		double num14 = num7 / ((double)num * num11);
		double num15 = num13 / ((double)num * num10) * (num7 * num14 / 2.0);
		double num16 = num13 / ((double)num * num10) * (num7 * Math.Pow(num14, 3.0) / 24.0) * (-4.0 * num12 * num12 + 9.0 * num12 * (1.0 - num13 * num13) + 12.0 * num13 * num13);
		double num17 = num13 / ((double)num * num10) * (num7 * Math.Pow(num14, 5.0) / 720.0) * (8.0 * Math.Pow(num12, 4.0) * (11.0 - 24.0 * num13 * num13) - 12.0 * Math.Pow(num12, 3.0) * (21.0 - 71.0 * num13 * num13) + 15.0 * num12 * num12 * (15.0 - 98.0 * num13 * num13 + 15.0 * Math.Pow(num13, 4.0)) + 180.0 * num12 * (5.0 * num13 * num13 - 3.0 * Math.Pow(num13, 4.0)) + 360.0 * Math.Pow(num13, 4.0));
		double num18 = num13 / ((double)num * num10) * (num7 * Math.Pow(num14, 7.0) / 40320.0) * (1385.0 + 3633.0 * num13 * num13 + 4095.0 * Math.Pow(num13, 4.0) + 1575.0 * Math.Pow(num13, 6.0));
		double num19 = num9 - num15 + num16 - num17 + num18;
		double num20 = PureProjection.RadiansToDegrees(num19);
		double num21 = 1.0 / Math.Cos(num9);
		double num22 = num14 * num21;
		double num23 = Math.Pow(num14, 3.0) / 6.0 * num21 * (num12 + 2.0 * num13 * num13);
		double num24 = Math.Pow(num14, 5.0) / 120.0 * num21 * (-4.0 * Math.Pow(num12, 3.0) * (1.0 - 6.0 * num13 * num13) + num12 * num12 * (9.0 - 68.0 * num13 * num13) + 72.0 * num12 * num13 * num13 + 24.0 * Math.Pow(num13, 4.0));
		double num25 = Math.Pow(num14, 7.0) / 5040.0 * num21 * (61.0 + 662.0 * num13 * num13 + 1320.0 * Math.Pow(num13, 4.0) + 720.0 * Math.Pow(num13, 6.0));
		double num26 = num22 - num23 + num24 - num25;
		double num27 = PureProjection.DegreesToRadians(GetLCM(zone)) + num26;
		double num28 = PureProjection.RadiansToDegrees(num27);
		return new double[4] { num20, num28, num19, num27 };
	}

	public override GPoint FromLatLngToPixel(double lat, double lng, int zoom)
	{
		GPoint empty = GPoint.Empty;
		lat = PureProjection.Clip(lat, MinLatitude, MaxLatitude);
		lng = PureProjection.Clip(lng, MinLongitude, MaxLongitude);
		GSize tileMatrixSizePixel = GetTileMatrixSizePixel(zoom);
		long[] array = WGSToPP(lat, lng);
		empty.X = array[0] >> 20 - zoom;
		empty.Y = tileMatrixSizePixel.Height - (array[1] >> 20 - zoom);
		return empty;
	}

	public override PointLatLng FromPixelToLatLng(long x, long y, int zoom)
	{
		PointLatLng empty = PointLatLng.Empty;
		GSize tileMatrixSizePixel = GetTileMatrixSizePixel(zoom);
		long num = x << 20 - zoom;
		long num2 = tileMatrixSizePixel.Height - y << 20 - zoom;
		double[] array = PPToWGS(num, num2);
		empty.Lat = PureProjection.Clip(array[0], MinLatitude, MaxLatitude);
		empty.Lng = PureProjection.Clip(array[1], MinLongitude, MaxLongitude);
		return empty;
	}

	public override GSize GetTileMatrixSizeXY(int zoom)
	{
		return new GSize((long)Math.Pow(2.0, zoom), (long)Math.Pow(2.0, zoom));
	}

	public override GSize GetTileMatrixSizePixel(int zoom)
	{
		GSize tileMatrixSizeXY = GetTileMatrixSizeXY(zoom);
		return new GSize(tileMatrixSizeXY.Width << 8, tileMatrixSizeXY.Height << 8);
	}

	public override GSize GetTileMatrixMinXY(int zoom)
	{
		long num = ((zoom > 3) ? (3 * (long)Math.Pow(2.0, zoom - 4)) : 1);
		return new GSize(num, num);
	}

	public override GSize GetTileMatrixMaxXY(int zoom)
	{
		long num = (long)Math.Pow(2.0, zoom) - (long)Math.Pow(2.0, zoom - 2);
		return new GSize(num, num);
	}
}
