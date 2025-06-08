using System;
using System.Collections.Generic;

namespace GMap.NET.Projections;

public class LKS92Projection : PureProjection
{
	public static readonly LKS92Projection Instance = new LKS92Projection();

	private static readonly double MinLatitude = 55.55;

	private static readonly double MaxLatitude = 58.22;

	private static readonly double MinLongitude = 20.22;

	private static readonly double MaxLongitude = 28.28;

	private static readonly double OrignX = -5120900.0;

	private static readonly double OrignY = 3998100.0;

	private static readonly double ScaleFactor = 0.9996;

	private static readonly double CentralMeridian = 0.4188790204786391;

	private static readonly double LatOrigin = 0.0;

	private static readonly double FalseNorthing = -6000000.0;

	private static readonly double FalseEasting = 500000.0;

	private static readonly double SemiMajor = 6378137.0;

	private static readonly double SemiMinor = 6356752.314140356;

	private static readonly double SemiMinor2 = 6356752.314245179;

	private static readonly double MetersPerUnit = 1.0;

	private static readonly double COS_67P5 = 0.3826834323650898;

	private static readonly double AD_C = 1.0026;

	private Dictionary<int, GSize> _extentMatrixMin;

	private Dictionary<int, GSize> _extentMatrixMax;

	public override RectLatLng Bounds => RectLatLng.FromLTRB(MinLongitude, MaxLatitude, MaxLongitude, MinLatitude);

	public override GSize TileSize { get; } = new GSize(256L, 256L);


	public override double Axis => 6378137.0;

	public override double Flattening => 0.003352810681182319;

	public override GPoint FromLatLngToPixel(double lat, double lng, int zoom)
	{
		lat = PureProjection.Clip(lat, MinLatitude, MaxLatitude);
		lng = PureProjection.Clip(lng, MinLongitude, MaxLongitude);
		double[] lonlat = new double[2] { lng, lat };
		lonlat = DTM10(lonlat);
		lonlat = MTD10(lonlat);
		lonlat = DTM00(lonlat);
		double tileMatrixResolution = GetTileMatrixResolution(zoom);
		return LksToPixel(lonlat, tileMatrixResolution);
	}

	private static GPoint LksToPixel(double[] lks, double res)
	{
		return new GPoint((long)Math.Floor((lks[0] - OrignX) / res), (long)Math.Floor((OrignY - lks[1]) / res));
	}

	public override PointLatLng FromPixelToLatLng(long x, long y, int zoom)
	{
		PointLatLng empty = PointLatLng.Empty;
		double tileMatrixResolution = GetTileMatrixResolution(zoom);
		double[] p = new double[2]
		{
			(double)x * tileMatrixResolution + OrignX,
			OrignY - (double)y * tileMatrixResolution
		};
		p = MTD11(p);
		p = DTM10(p);
		p = MTD10(p);
		empty.Lat = PureProjection.Clip(p[1], MinLatitude, MaxLatitude);
		empty.Lng = PureProjection.Clip(p[0], MinLongitude, MaxLongitude);
		return empty;
	}

	private double[] DTM10(double[] lonlat)
	{
		double num = 1.0 - SemiMinor2 * SemiMinor2 / (SemiMajor * SemiMajor);
		_ = (Math.Pow(SemiMajor, 2.0) - Math.Pow(SemiMinor2, 2.0)) / Math.Pow(SemiMinor2, 2.0);
		_ = SemiMinor2 / SemiMajor;
		_ = SemiMajor / SemiMinor2;
		double num2 = PureProjection.DegreesToRadians(lonlat[0]);
		double num3 = PureProjection.DegreesToRadians(lonlat[1]);
		double num4 = ((lonlat.Length < 3) ? 0.0 : (lonlat[2].Equals(double.NaN) ? 0.0 : lonlat[2]));
		double num5 = SemiMajor / Math.Sqrt(1.0 - num * Math.Pow(Math.Sin(num3), 2.0));
		double num6 = (num5 + num4) * Math.Cos(num3) * Math.Cos(num2);
		double num7 = (num5 + num4) * Math.Cos(num3) * Math.Sin(num2);
		double num8 = ((1.0 - num) * num5 + num4) * Math.Sin(num3);
		return new double[3] { num6, num7, num8 };
	}

	private double[] MTD10(double[] pnt)
	{
		double num = 1.0 - SemiMinor * SemiMinor / (SemiMajor * SemiMajor);
		double num2 = (Math.Pow(SemiMajor, 2.0) - Math.Pow(SemiMinor, 2.0)) / Math.Pow(SemiMinor, 2.0);
		_ = SemiMinor / SemiMajor;
		_ = SemiMajor / SemiMinor;
		bool flag = false;
		double num3 = ((pnt.Length < 3) ? 0.0 : (pnt[2].Equals(double.NaN) ? 0.0 : pnt[2]));
		double rad = 0.0;
		double rad2;
		if (pnt[0] != 0.0)
		{
			rad2 = Math.Atan2(pnt[1], pnt[0]);
		}
		else if (pnt[1] > 0.0)
		{
			rad2 = Math.PI / 2.0;
		}
		else if (pnt[1] < 0.0)
		{
			rad2 = -Math.PI / 2.0;
		}
		else
		{
			flag = true;
			rad2 = 0.0;
			if (num3 > 0.0)
			{
				rad = Math.PI / 2.0;
			}
			else
			{
				if (!(num3 < 0.0))
				{
					return new double[3]
					{
						PureProjection.RadiansToDegrees(rad2),
						PureProjection.RadiansToDegrees(Math.PI / 2.0),
						0.0 - SemiMinor
					};
				}
				rad = -Math.PI / 2.0;
			}
		}
		double num4 = pnt[0] * pnt[0] + pnt[1] * pnt[1];
		double num5 = Math.Sqrt(num4);
		double num6 = num3 * AD_C;
		double num7 = Math.Sqrt(num6 * num6 + num4);
		double x = num6 / num7;
		double num8 = num5 / num7;
		double num9 = Math.Pow(x, 3.0);
		double num10 = num3 + SemiMinor * num2 * num9;
		double num11 = num5 - SemiMajor * num * num8 * num8 * num8;
		double num12 = Math.Sqrt(num10 * num10 + num11 * num11);
		double num13 = num10 / num12;
		double num14 = num11 / num12;
		double num15 = SemiMajor / Math.Sqrt(1.0 - num * num13 * num13);
		double num16 = ((num14 >= COS_67P5) ? (num5 / num14 - num15) : ((!(num14 <= 0.0 - COS_67P5)) ? (num3 / num13 + num15 * (num - 1.0)) : (num5 / (0.0 - num14) - num15)));
		if (!flag)
		{
			rad = Math.Atan(num13 / num14);
		}
		return new double[3]
		{
			PureProjection.RadiansToDegrees(rad2),
			PureProjection.RadiansToDegrees(rad),
			num16
		};
	}

	private double[] DTM00(double[] lonlat)
	{
		double num = 1.0 - Math.Pow(SemiMinor / SemiMajor, 2.0);
		Math.Sqrt(num);
		double e = PureProjection.E0Fn(num);
		double e2 = PureProjection.E1Fn(num);
		double e3 = PureProjection.E2Fn(num);
		double e4 = PureProjection.E3Fn(num);
		double num2 = SemiMajor * PureProjection.Mlfn(e, e2, e3, e4, LatOrigin);
		double num3 = num / (1.0 - num);
		double num4 = PureProjection.DegreesToRadians(lonlat[0]);
		double num5 = PureProjection.DegreesToRadians(lonlat[1]);
		double num6 = PureProjection.AdjustLongitude(num4 - CentralMeridian);
		PureProjection.SinCos(num5, out var sin, out var cos);
		double num7 = cos * num6;
		double num8 = Math.Pow(num7, 2.0);
		double num9 = num3 * Math.Pow(cos, 2.0);
		double num10 = Math.Tan(num5);
		double num11 = Math.Pow(num10, 2.0);
		double d = 1.0 - num * Math.Pow(sin, 2.0);
		double num12 = SemiMajor / Math.Sqrt(d);
		double num13 = SemiMajor * PureProjection.Mlfn(e, e2, e3, e4, num5);
		double num14 = ScaleFactor * num12 * num7 * (1.0 + num8 / 6.0 * (1.0 - num11 + num9 + num8 / 20.0 * (5.0 - 18.0 * num11 + Math.Pow(num11, 2.0) + 72.0 * num9 - 58.0 * num3))) + FalseEasting;
		double num15 = ScaleFactor * (num13 - num2 + num12 * num10 * (num8 * (0.5 + num8 / 24.0 * (5.0 - num11 + 9.0 * num9 + 4.0 * Math.Pow(num9, 2.0) + num8 / 30.0 * (61.0 - 58.0 * num11 + Math.Pow(num11, 2.0) + 600.0 * num9 - 330.0 * num3))))) + FalseNorthing;
		if (lonlat.Length >= 3)
		{
			return new double[3]
			{
				num14 / MetersPerUnit,
				num15 / MetersPerUnit,
				lonlat[2]
			};
		}
		return new double[2]
		{
			num14 / MetersPerUnit,
			num15 / MetersPerUnit
		};
	}

	private double[] DTM01(double[] lonlat)
	{
		double num = 1.0 - SemiMinor * SemiMinor / (SemiMajor * SemiMajor);
		_ = (Math.Pow(SemiMajor, 2.0) - Math.Pow(SemiMinor, 2.0)) / Math.Pow(SemiMinor, 2.0);
		_ = SemiMinor / SemiMajor;
		_ = SemiMajor / SemiMinor;
		double num2 = PureProjection.DegreesToRadians(lonlat[0]);
		double num3 = PureProjection.DegreesToRadians(lonlat[1]);
		double num4 = ((lonlat.Length < 3) ? 0.0 : (lonlat[2].Equals(double.NaN) ? 0.0 : lonlat[2]));
		double num5 = SemiMajor / Math.Sqrt(1.0 - num * Math.Pow(Math.Sin(num3), 2.0));
		double num6 = (num5 + num4) * Math.Cos(num3) * Math.Cos(num2);
		double num7 = (num5 + num4) * Math.Cos(num3) * Math.Sin(num2);
		double num8 = ((1.0 - num) * num5 + num4) * Math.Sin(num3);
		return new double[3] { num6, num7, num8 };
	}

	private double[] MTD01(double[] pnt)
	{
		double num = 1.0 - SemiMinor2 * SemiMinor2 / (SemiMajor * SemiMajor);
		double num2 = (Math.Pow(SemiMajor, 2.0) - Math.Pow(SemiMinor2, 2.0)) / Math.Pow(SemiMinor2, 2.0);
		_ = SemiMinor2 / SemiMajor;
		_ = SemiMajor / SemiMinor2;
		bool flag = false;
		double num3 = ((pnt.Length < 3) ? 0.0 : (pnt[2].Equals(double.NaN) ? 0.0 : pnt[2]));
		double rad = 0.0;
		double rad2;
		if (pnt[0] != 0.0)
		{
			rad2 = Math.Atan2(pnt[1], pnt[0]);
		}
		else if (pnt[1] > 0.0)
		{
			rad2 = Math.PI / 2.0;
		}
		else if (pnt[1] < 0.0)
		{
			rad2 = -Math.PI / 2.0;
		}
		else
		{
			flag = true;
			rad2 = 0.0;
			if (num3 > 0.0)
			{
				rad = Math.PI / 2.0;
			}
			else
			{
				if (!(num3 < 0.0))
				{
					return new double[3]
					{
						PureProjection.RadiansToDegrees(rad2),
						PureProjection.RadiansToDegrees(Math.PI / 2.0),
						0.0 - SemiMinor2
					};
				}
				rad = -Math.PI / 2.0;
			}
		}
		double num4 = pnt[0] * pnt[0] + pnt[1] * pnt[1];
		double num5 = Math.Sqrt(num4);
		double num6 = num3 * AD_C;
		double num7 = Math.Sqrt(num6 * num6 + num4);
		double x = num6 / num7;
		double num8 = num5 / num7;
		double num9 = Math.Pow(x, 3.0);
		double num10 = num3 + SemiMinor2 * num2 * num9;
		double num11 = num5 - SemiMajor * num * num8 * num8 * num8;
		double num12 = Math.Sqrt(num10 * num10 + num11 * num11);
		double num13 = num10 / num12;
		double num14 = num11 / num12;
		double num15 = SemiMajor / Math.Sqrt(1.0 - num * num13 * num13);
		double num16 = ((num14 >= COS_67P5) ? (num5 / num14 - num15) : ((!(num14 <= 0.0 - COS_67P5)) ? (num3 / num13 + num15 * (num - 1.0)) : (num5 / (0.0 - num14) - num15)));
		if (!flag)
		{
			rad = Math.Atan(num13 / num14);
		}
		return new double[3]
		{
			PureProjection.RadiansToDegrees(rad2),
			PureProjection.RadiansToDegrees(rad),
			num16
		};
	}

	private double[] MTD11(double[] p)
	{
		double num = 1.0 - Math.Pow(SemiMinor / SemiMajor, 2.0);
		Math.Sqrt(num);
		double num2 = PureProjection.E0Fn(num);
		double num3 = PureProjection.E1Fn(num);
		double num4 = PureProjection.E2Fn(num);
		double num5 = PureProjection.E3Fn(num);
		double num6 = SemiMajor * PureProjection.Mlfn(num2, num3, num4, num5, LatOrigin);
		double num7 = num / (1.0 - num);
		long num8 = 6L;
		double num9 = p[0] * MetersPerUnit - FalseEasting;
		double num10 = p[1] * MetersPerUnit - FalseNorthing;
		double num11 = (num6 + num10 / ScaleFactor) / SemiMajor;
		double num12 = num11;
		long num13 = 0L;
		while (true)
		{
			double num14 = (num11 + num3 * Math.Sin(2.0 * num12) - num4 * Math.Sin(4.0 * num12) + num5 * Math.Sin(6.0 * num12)) / num2 - num12;
			num12 += num14;
			if (Math.Abs(num14) <= PureProjection.Epsilon)
			{
				break;
			}
			if (num13 >= num8)
			{
				throw new ArgumentException("Latitude failed to converge");
			}
			num13++;
		}
		if (Math.Abs(num12) < PureProjection.HalfPi)
		{
			PureProjection.SinCos(num12, out var sin, out var cos);
			double num15 = Math.Tan(num12);
			double num16 = num7 * Math.Pow(cos, 2.0);
			double num17 = Math.Pow(num16, 2.0);
			double num18 = Math.Pow(num15, 2.0);
			double num19 = Math.Pow(num18, 2.0);
			num11 = 1.0 - num * Math.Pow(sin, 2.0);
			double num20 = SemiMajor / Math.Sqrt(num11);
			double num21 = num20 * (1.0 - num) / num11;
			double num22 = num9 / (num20 * ScaleFactor);
			double num23 = Math.Pow(num22, 2.0);
			double rad = num12 - num20 * num15 * num23 / num21 * (0.5 - num23 / 24.0 * (5.0 + 3.0 * num18 + 10.0 * num16 - 4.0 * num17 - 9.0 * num7 - num23 / 30.0 * (61.0 + 90.0 * num18 + 298.0 * num16 + 45.0 * num19 - 252.0 * num7 - 3.0 * num17)));
			double rad2 = PureProjection.AdjustLongitude(CentralMeridian + num22 * (1.0 - num23 / 6.0 * (1.0 + 2.0 * num18 + num16 - num23 / 20.0 * (5.0 - 2.0 * num16 + 28.0 * num18 - 3.0 * num17 + 8.0 * num7 + 24.0 * num19))) / cos);
			if (p.Length >= 3)
			{
				return new double[3]
				{
					PureProjection.RadiansToDegrees(rad2),
					PureProjection.RadiansToDegrees(rad),
					p[2]
				};
			}
			return new double[2]
			{
				PureProjection.RadiansToDegrees(rad2),
				PureProjection.RadiansToDegrees(rad)
			};
		}
		if (p.Length >= 3)
		{
			return new double[3]
			{
				PureProjection.RadiansToDegrees(PureProjection.HalfPi * PureProjection.Sign(num10)),
				PureProjection.RadiansToDegrees(CentralMeridian),
				p[2]
			};
		}
		return new double[2]
		{
			PureProjection.RadiansToDegrees(PureProjection.HalfPi * PureProjection.Sign(num10)),
			PureProjection.RadiansToDegrees(CentralMeridian)
		};
	}

	public static double GetTileMatrixResolution(int zoom)
	{
		double result = 0.0;
		switch (zoom)
		{
		case 0:
			result = 1587.5031750063501;
			break;
		case 1:
			result = 793.7515875031751;
			break;
		case 2:
			result = 529.1677250021168;
			break;
		case 3:
			result = 264.5838625010584;
			break;
		case 4:
			result = 132.2919312505292;
			break;
		case 5:
			result = 52.91677250021167;
			break;
		case 6:
			result = 26.458386250105836;
			break;
		case 7:
			result = 13.229193125052918;
			break;
		case 8:
			result = 6.614596562526459;
			break;
		case 9:
			result = 2.6458386250105836;
			break;
		case 10:
			result = 1.3229193125052918;
			break;
		case 11:
			result = 0.5291677250021167;
			break;
		}
		return result;
	}

	public override double GetGroundResolution(int zoom, double latitude)
	{
		return GetTileMatrixResolution(zoom);
	}

	public override GSize GetTileMatrixMinXY(int zoom)
	{
		if (_extentMatrixMin == null)
		{
			GenerateExtents();
		}
		return _extentMatrixMin[zoom];
	}

	public override GSize GetTileMatrixMaxXY(int zoom)
	{
		if (_extentMatrixMax == null)
		{
			GenerateExtents();
		}
		return _extentMatrixMax[zoom];
	}

	private void GenerateExtents()
	{
		_extentMatrixMin = new Dictionary<int, GSize>();
		_extentMatrixMax = new Dictionary<int, GSize>();
		for (int i = 0; i <= 11; i++)
		{
			GetTileMatrixResolution(i);
			_extentMatrixMin.Add(i, new GSize(FromPixelToTileXY(FromLatLngToPixel(Bounds.LocationTopLeft, i))));
			_extentMatrixMax.Add(i, new GSize(FromPixelToTileXY(FromLatLngToPixel(Bounds.LocationRightBottom, i))));
		}
	}
}
