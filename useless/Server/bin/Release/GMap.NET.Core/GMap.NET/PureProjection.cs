using System;
using System.Collections.Generic;
using System.Text;

namespace GMap.NET;

public abstract class PureProjection
{
	private readonly List<Dictionary<PointLatLng, GPoint>> _fromLatLngToPixelCache = new List<Dictionary<PointLatLng, GPoint>>(33);

	private readonly List<Dictionary<GPoint, PointLatLng>> _fromPixelToLatLngCache = new List<Dictionary<GPoint, PointLatLng>>(33);

	protected static readonly double HalfPi = Math.PI / 2.0;

	protected static readonly double TwoPi = Math.PI * 2.0;

	protected static readonly double Epsilon = 1E-10;

	protected const double MaxVal = 4.0;

	protected static readonly double MaxLong = 2147483647.0;

	protected static readonly double DblLong = 4.61168601E+18;

	private static readonly double R2D = 180.0 / Math.PI;

	private static readonly double D2R = Math.PI / 180.0;

	public abstract GSize TileSize { get; }

	public abstract double Axis { get; }

	public abstract double Flattening { get; }

	public virtual RectLatLng Bounds => RectLatLng.FromLTRB(-180.0, 90.0, 180.0, -90.0);

	public PureProjection()
	{
		for (int i = 0; i < _fromLatLngToPixelCache.Capacity; i++)
		{
			_fromLatLngToPixelCache.Add(new Dictionary<PointLatLng, GPoint>());
			_fromPixelToLatLngCache.Add(new Dictionary<GPoint, PointLatLng>());
		}
	}

	public abstract GPoint FromLatLngToPixel(double lat, double lng, int zoom);

	public abstract PointLatLng FromPixelToLatLng(long x, long y, int zoom);

	public GPoint FromLatLngToPixel(PointLatLng p, int zoom)
	{
		return FromLatLngToPixel(p, zoom, useCache: false);
	}

	public GPoint FromLatLngToPixel(PointLatLng p, int zoom, bool useCache)
	{
		if (useCache)
		{
			GPoint value = GPoint.Empty;
			if (!_fromLatLngToPixelCache[zoom].TryGetValue(p, out value))
			{
				value = FromLatLngToPixel(p.Lat, p.Lng, zoom);
				_fromLatLngToPixelCache[zoom].Add(p, value);
				if (!_fromPixelToLatLngCache[zoom].ContainsKey(value))
				{
					_fromPixelToLatLngCache[zoom].Add(value, p);
				}
			}
			return value;
		}
		return FromLatLngToPixel(p.Lat, p.Lng, zoom);
	}

	public PointLatLng FromPixelToLatLng(GPoint p, int zoom)
	{
		return FromPixelToLatLng(p, zoom, useCache: false);
	}

	public PointLatLng FromPixelToLatLng(GPoint p, int zoom, bool useCache)
	{
		if (useCache)
		{
			PointLatLng value = PointLatLng.Empty;
			if (!_fromPixelToLatLngCache[zoom].TryGetValue(p, out value))
			{
				value = FromPixelToLatLng(p.X, p.Y, zoom);
				_fromPixelToLatLngCache[zoom].Add(p, value);
				if (!_fromLatLngToPixelCache[zoom].ContainsKey(value))
				{
					_fromLatLngToPixelCache[zoom].Add(value, p);
				}
			}
			return value;
		}
		return FromPixelToLatLng(p.X, p.Y, zoom);
	}

	public virtual GPoint FromPixelToTileXY(GPoint p)
	{
		return new GPoint(p.X / TileSize.Width, p.Y / TileSize.Height);
	}

	public virtual GPoint FromTileXYToPixel(GPoint p)
	{
		return new GPoint(p.X * TileSize.Width, p.Y * TileSize.Height);
	}

	public abstract GSize GetTileMatrixMinXY(int zoom);

	public abstract GSize GetTileMatrixMaxXY(int zoom);

	public virtual GSize GetTileMatrixSizeXY(int zoom)
	{
		GSize tileMatrixMinXY = GetTileMatrixMinXY(zoom);
		GSize tileMatrixMaxXY = GetTileMatrixMaxXY(zoom);
		return new GSize(tileMatrixMaxXY.Width - tileMatrixMinXY.Width + 1, tileMatrixMaxXY.Height - tileMatrixMinXY.Height + 1);
	}

	public long GetTileMatrixItemCount(int zoom)
	{
		GSize tileMatrixSizeXY = GetTileMatrixSizeXY(zoom);
		return tileMatrixSizeXY.Width * tileMatrixSizeXY.Height;
	}

	public virtual GSize GetTileMatrixSizePixel(int zoom)
	{
		GSize tileMatrixSizeXY = GetTileMatrixSizeXY(zoom);
		return new GSize(tileMatrixSizeXY.Width * TileSize.Width, tileMatrixSizeXY.Height * TileSize.Height);
	}

	public List<GPoint> GetAreaTileList(RectLatLng rect, int zoom, int padding)
	{
		GPoint gPoint = FromPixelToTileXY(FromLatLngToPixel(rect.LocationTopLeft, zoom));
		GPoint gPoint2 = FromPixelToTileXY(FromLatLngToPixel(rect.LocationRightBottom, zoom));
		long num = Math.Max(0L, gPoint.X - padding);
		long num2 = gPoint2.X + padding;
		long num3 = Math.Max(0L, gPoint.Y - padding);
		long num4 = gPoint2.Y + padding;
		List<GPoint> list = new List<GPoint>((int)((num2 - num + 1) * (num4 - num3 + 1)));
		for (; num <= num2; num++)
		{
			for (long num5 = num3; num5 <= num4; num5++)
			{
				list.Add(new GPoint(num, num5));
			}
		}
		return list;
	}

	public virtual double GetGroundResolution(int zoom, double latitude)
	{
		return Math.Cos(latitude * (Math.PI / 180.0)) * 2.0 * Math.PI * Axis / (double)GetTileMatrixSizePixel(zoom).Width;
	}

	public static double DegreesToRadians(double deg)
	{
		return D2R * deg;
	}

	public static double RadiansToDegrees(double rad)
	{
		return R2D * rad;
	}

	protected static double Sign(double x)
	{
		return (!(x < 0.0)) ? 1 : (-1);
	}

	protected static double AdjustLongitude(double x)
	{
		long num = 0L;
		while (!(Math.Abs(x) <= Math.PI))
		{
			x = (((long)Math.Abs(x / Math.PI) < 2) ? (x - Sign(x) * TwoPi) : (((double)(long)Math.Abs(x / TwoPi) < MaxLong) ? (x - (double)(long)(x / TwoPi) * TwoPi) : (((double)(long)Math.Abs(x / (MaxLong * TwoPi)) < MaxLong) ? (x - (double)(long)(x / (MaxLong * TwoPi)) * (TwoPi * MaxLong)) : ((!((double)(long)Math.Abs(x / (DblLong * TwoPi)) < MaxLong)) ? (x - Sign(x) * TwoPi) : (x - (double)(long)(x / (DblLong * TwoPi)) * (TwoPi * DblLong))))));
			num++;
			if ((double)num > 4.0)
			{
				break;
			}
		}
		return x;
	}

	protected static void SinCos(double val, out double sin, out double cos)
	{
		sin = Math.Sin(val);
		cos = Math.Cos(val);
	}

	protected static double E0Fn(double x)
	{
		return 1.0 - 0.25 * x * (1.0 + x / 16.0 * (3.0 + 1.25 * x));
	}

	protected static double E1Fn(double x)
	{
		return 0.375 * x * (1.0 + 0.25 * x * (1.0 + 15.0 / 32.0 * x));
	}

	protected static double E2Fn(double x)
	{
		return 15.0 / 256.0 * x * x * (1.0 + 0.75 * x);
	}

	protected static double E3Fn(double x)
	{
		return x * x * x * 0.011393229166666666;
	}

	protected static double Mlfn(double e0, double e1, double e2, double e3, double phi)
	{
		return e0 * phi - e1 * Math.Sin(2.0 * phi) + e2 * Math.Sin(4.0 * phi) - e3 * Math.Sin(6.0 * phi);
	}

	protected static long GetUTMZone(double lon)
	{
		return (long)((lon + 180.0) / 6.0 + 1.0);
	}

	protected static double Clip(double n, double minValue, double maxValue)
	{
		return Math.Min(Math.Max(n, minValue), maxValue);
	}

	public double GetDistance(PointLatLng p1, PointLatLng p2)
	{
		double num = p1.Lat * (Math.PI / 180.0);
		double num2 = p1.Lng * (Math.PI / 180.0);
		double num3 = p2.Lat * (Math.PI / 180.0);
		double num4 = p2.Lng * (Math.PI / 180.0) - num2;
		double num5 = Math.Pow(Math.Sin((num3 - num) / 2.0), 2.0) + Math.Cos(num) * Math.Cos(num3) * Math.Pow(Math.Sin(num4 / 2.0), 2.0);
		double num6 = 2.0 * Math.Atan2(Math.Sqrt(num5), Math.Sqrt(1.0 - num5));
		return Axis / 1000.0 * num6;
	}

	public double GetDistanceInPixels(GPoint point1, GPoint point2)
	{
		double num = point2.X - point1.X;
		double num2 = point2.Y - point1.Y;
		return Math.Sqrt(num * num + num2 * num2);
	}

	public double GetBearing(PointLatLng p1, PointLatLng p2)
	{
		double num = DegreesToRadians(p1.Lat);
		double num2 = DegreesToRadians(p2.Lat);
		double num3 = DegreesToRadians(p2.Lng - p1.Lng);
		double y = Math.Sin(num3) * Math.Cos(num2);
		double x = Math.Cos(num) * Math.Sin(num2) - Math.Sin(num) * Math.Cos(num2) * Math.Cos(num3);
		return (RadiansToDegrees(Math.Atan2(y, x)) + 360.0) % 360.0;
	}

	public void FromGeodeticToCartesian(double lat, double lng, double height, out double x, out double y, out double z)
	{
		lat = Math.PI / 180.0 * lat;
		lng = Math.PI / 180.0 * lng;
		double num = Axis * (1.0 - Flattening);
		double num2 = 1.0 - num / Axis * (num / Axis);
		double num3 = Axis / Math.Sqrt(1.0 - num2 * Math.Sin(lat) * Math.Sin(lat));
		x = (num3 + height) * Math.Cos(lat) * Math.Cos(lng);
		y = (num3 + height) * Math.Cos(lat) * Math.Sin(lng);
		z = (num3 * (num / Axis) * (num / Axis) + height) * Math.Sin(lat);
	}

	public void FromCartesianTGeodetic(double x, double y, double z, out double lat, out double lng)
	{
		double num = Flattening * (2.0 - Flattening);
		lng = Math.Atan2(y, x);
		double num2 = Math.Sqrt(x * x + y * y);
		double num3 = Math.Atan2(z, num2 * (1.0 - Flattening));
		double num4 = Math.Sin(num3);
		double num5 = Math.Cos(num3);
		lat = Math.Atan2(z + num / (1.0 - Flattening) * Axis * num4 * num4 * num4, num2 - num * Axis * num5 * num5 * num5);
		lat /= Math.PI / 180.0;
		lng /= Math.PI / 180.0;
	}

	public static List<PointLatLng> PolylineDecode(string encodedPath)
	{
		List<PointLatLng> list = new List<PointLatLng>();
		int length = encodedPath.Length;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		while (num < length)
		{
			int num4 = 1;
			int num5 = 0;
			int num6;
			do
			{
				num6 = encodedPath[num++] - 63 - 1;
				num4 += num6 << num5;
				num5 += 5;
			}
			while (num6 >= 31 && num < length);
			num2 += ((((uint)num4 & (true ? 1u : 0u)) != 0) ? (~(num4 >> 1)) : (num4 >> 1));
			num4 = 1;
			num5 = 0;
			if (num < length)
			{
				do
				{
					num6 = encodedPath[num++] - 63 - 1;
					num4 += num6 << num5;
					num5 += 5;
				}
				while (num6 >= 31 && num < length);
				num3 += ((((uint)num4 & (true ? 1u : 0u)) != 0) ? (~(num4 >> 1)) : (num4 >> 1));
			}
			list.Add(new PointLatLng((double)num2 * 1E-05, (double)num3 * 1E-05));
		}
		return list;
	}

	public static void PolylineDecode(List<PointLatLng> path, string encodedPath)
	{
		path.AddRange(PolylineDecode(encodedPath));
	}

	public static string PolylineEncode(List<PointLatLng> path)
	{
		long num = 0L;
		long num2 = 0L;
		StringBuilder stringBuilder = new StringBuilder();
		foreach (PointLatLng item in path)
		{
			long num3 = Convert.ToInt64(Math.Round(item.Lat * 100000.0));
			long num4 = Convert.ToInt64(Math.Round(item.Lng * 100000.0));
			long point = num3 - num;
			long point2 = num4 - num2;
			Encode(point, stringBuilder);
			Encode(point2, stringBuilder);
			num = num3;
			num2 = num4;
		}
		return stringBuilder.ToString();
	}

	private static void Encode(long point, StringBuilder result)
	{
		for (point = ((point < 0) ? (~(point << 1)) : (point << 1)); point >= 32; point >>= 5)
		{
			result.Append(Convert.ToChar((int)((0x20 | (point & 0x1F)) + 63)));
		}
		result.Append(Convert.ToChar((int)(point + 63)));
	}
}
