using System;
using System.Globalization;

namespace GMap.NET;

[Serializable]
public struct PointLatLng
{
	public static readonly PointLatLng Empty;

	private double _lat;

	private double _lng;

	private bool _notEmpty;

	public bool IsEmpty => !_notEmpty;

	public double Lat
	{
		get
		{
			return _lat;
		}
		set
		{
			_lat = value;
			_notEmpty = true;
		}
	}

	public double Lng
	{
		get
		{
			return _lng;
		}
		set
		{
			_lng = value;
			_notEmpty = true;
		}
	}

	public PointLatLng(double lat, double lng)
	{
		_lat = lat;
		_lng = lng;
		_notEmpty = true;
	}

	public static PointLatLng operator +(PointLatLng pt, SizeLatLng sz)
	{
		return Add(pt, sz);
	}

	public static PointLatLng operator -(PointLatLng pt, SizeLatLng sz)
	{
		return Subtract(pt, sz);
	}

	public static SizeLatLng operator -(PointLatLng pt1, PointLatLng pt2)
	{
		return new SizeLatLng(pt1.Lat - pt2.Lat, pt2.Lng - pt1.Lng);
	}

	public static bool operator ==(PointLatLng left, PointLatLng right)
	{
		if (left.Lng == right.Lng)
		{
			return left.Lat == right.Lat;
		}
		return false;
	}

	public static bool operator !=(PointLatLng left, PointLatLng right)
	{
		return !(left == right);
	}

	public static PointLatLng Add(PointLatLng pt, SizeLatLng sz)
	{
		return new PointLatLng(pt.Lat - sz.HeightLat, pt.Lng + sz.WidthLng);
	}

	public static PointLatLng Subtract(PointLatLng pt, SizeLatLng sz)
	{
		return new PointLatLng(pt.Lat + sz.HeightLat, pt.Lng - sz.WidthLng);
	}

	public override bool Equals(object obj)
	{
		if (!(obj is PointLatLng pointLatLng))
		{
			return false;
		}
		if (pointLatLng.Lng == Lng && pointLatLng.Lat == Lat)
		{
			return pointLatLng.GetType().Equals(GetType());
		}
		return false;
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

	public override int GetHashCode()
	{
		return Lng.GetHashCode() ^ Lat.GetHashCode();
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.CurrentCulture, "{{Lat={0}, Lng={1}}}", Lat, Lng);
	}
}
