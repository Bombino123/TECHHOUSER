using System;
using System.Globalization;

namespace GMap.NET;

public struct RectLatLng
{
	public static readonly RectLatLng Empty;

	private bool _notEmpty;

	public PointLatLng LocationTopLeft
	{
		get
		{
			return new PointLatLng(Lat, Lng);
		}
		set
		{
			Lng = value.Lng;
			Lat = value.Lat;
		}
	}

	public PointLatLng LocationRightBottom
	{
		get
		{
			PointLatLng result = new PointLatLng(Lat, Lng);
			result.Offset(HeightLat, WidthLng);
			return result;
		}
	}

	public PointLatLng LocationMiddle
	{
		get
		{
			PointLatLng result = new PointLatLng(Lat, Lng);
			result.Offset(HeightLat / 2.0, WidthLng / 2.0);
			return result;
		}
	}

	public SizeLatLng Size
	{
		get
		{
			return new SizeLatLng(HeightLat, WidthLng);
		}
		set
		{
			WidthLng = value.WidthLng;
			HeightLat = value.HeightLat;
		}
	}

	public double Lng { get; set; }

	public double Lat { get; set; }

	public double WidthLng { get; set; }

	public double HeightLat { get; set; }

	public double Left => Lng;

	public double Top => Lat;

	public double Right => Lng + WidthLng;

	public double Bottom => Lat - HeightLat;

	public bool IsEmpty => !_notEmpty;

	public RectLatLng(double lat, double lng, double widthLng, double heightLat)
	{
		Lng = lng;
		Lat = lat;
		WidthLng = widthLng;
		HeightLat = heightLat;
		_notEmpty = true;
	}

	public RectLatLng(PointLatLng location, SizeLatLng size)
	{
		Lng = location.Lng;
		Lat = location.Lat;
		WidthLng = size.WidthLng;
		HeightLat = size.HeightLat;
		_notEmpty = true;
	}

	public static RectLatLng FromLTRB(double leftLng, double topLat, double rightLng, double bottomLat)
	{
		return new RectLatLng(topLat, leftLng, rightLng - leftLng, topLat - bottomLat);
	}

	public override bool Equals(object obj)
	{
		if (!(obj is RectLatLng rectLatLng))
		{
			return false;
		}
		if (rectLatLng.Lng == Lng && rectLatLng.Lat == Lat && rectLatLng.WidthLng == WidthLng)
		{
			return rectLatLng.HeightLat == HeightLat;
		}
		return false;
	}

	public static bool operator ==(RectLatLng left, RectLatLng right)
	{
		if (left.Lng == right.Lng && left.Lat == right.Lat && left.WidthLng == right.WidthLng)
		{
			return left.HeightLat == right.HeightLat;
		}
		return false;
	}

	public static bool operator !=(RectLatLng left, RectLatLng right)
	{
		return !(left == right);
	}

	public bool Contains(double lat, double lng)
	{
		if (Lng <= lng && lng < Lng + WidthLng && Lat >= lat)
		{
			return lat > Lat - HeightLat;
		}
		return false;
	}

	public bool Contains(PointLatLng pt)
	{
		return Contains(pt.Lat, pt.Lng);
	}

	public bool Contains(RectLatLng rect)
	{
		if (Lng <= rect.Lng && rect.Lng + rect.WidthLng <= Lng + WidthLng && Lat >= rect.Lat)
		{
			return rect.Lat - rect.HeightLat >= Lat - HeightLat;
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (IsEmpty)
		{
			return 0;
		}
		return Lng.GetHashCode() ^ Lat.GetHashCode() ^ WidthLng.GetHashCode() ^ HeightLat.GetHashCode();
	}

	public void Inflate(double lat, double lng)
	{
		Lng -= lng;
		Lat += lat;
		WidthLng += 2.0 * lng;
		HeightLat += 2.0 * lat;
	}

	public void Inflate(SizeLatLng size)
	{
		Inflate(size.HeightLat, size.WidthLng);
	}

	public static RectLatLng Inflate(RectLatLng rect, double lat, double lng)
	{
		RectLatLng result = rect;
		result.Inflate(lat, lng);
		return result;
	}

	public void Intersect(RectLatLng rect)
	{
		RectLatLng rectLatLng = Intersect(rect, this);
		Lng = rectLatLng.Lng;
		Lat = rectLatLng.Lat;
		WidthLng = rectLatLng.WidthLng;
		HeightLat = rectLatLng.HeightLat;
	}

	public static RectLatLng Intersect(RectLatLng a, RectLatLng b)
	{
		double num = Math.Max(a.Lng, b.Lng);
		double num2 = Math.Min(a.Lng + a.WidthLng, b.Lng + b.WidthLng);
		double num3 = Math.Max(a.Lat, b.Lat);
		double num4 = Math.Min(a.Lat + a.HeightLat, b.Lat + b.HeightLat);
		if (num2 >= num && num4 >= num3)
		{
			return new RectLatLng(num3, num, num2 - num, num4 - num3);
		}
		return Empty;
	}

	public bool IntersectsWith(RectLatLng a)
	{
		if (Left < a.Right && Top > a.Bottom && Right > a.Left)
		{
			return Bottom < a.Top;
		}
		return false;
	}

	public static RectLatLng Union(RectLatLng a, RectLatLng b)
	{
		return FromLTRB(Math.Min(a.Left, b.Left), Math.Max(a.Top, b.Top), Math.Max(a.Right, b.Right), Math.Min(a.Bottom, b.Bottom));
	}

	public void Offset(PointLatLng pos)
	{
		Offset(pos.Lat, pos.Lng);
	}

	public void Offset(double lat, double lng)
	{
		Lng += lng;
		Lat -= lat;
	}

	public override string ToString()
	{
		return "{Lat=" + Lat.ToString(CultureInfo.CurrentCulture) + ",Lng=" + Lng.ToString(CultureInfo.CurrentCulture) + ",WidthLng=" + WidthLng.ToString(CultureInfo.CurrentCulture) + ",HeightLat=" + HeightLat.ToString(CultureInfo.CurrentCulture) + "}";
	}

	static RectLatLng()
	{
		Empty = default(RectLatLng);
	}
}
