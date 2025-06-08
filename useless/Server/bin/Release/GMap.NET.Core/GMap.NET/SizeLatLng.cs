using System.Globalization;

namespace GMap.NET;

public struct SizeLatLng
{
	public static readonly SizeLatLng Empty;

	public bool IsEmpty
	{
		get
		{
			if (WidthLng == 0.0)
			{
				return HeightLat == 0.0;
			}
			return false;
		}
	}

	public double WidthLng { get; set; }

	public double HeightLat { get; set; }

	public SizeLatLng(SizeLatLng size)
	{
		WidthLng = size.WidthLng;
		HeightLat = size.HeightLat;
	}

	public SizeLatLng(PointLatLng pt)
	{
		HeightLat = pt.Lat;
		WidthLng = pt.Lng;
	}

	public SizeLatLng(double heightLat, double widthLng)
	{
		HeightLat = heightLat;
		WidthLng = widthLng;
	}

	public static SizeLatLng operator +(SizeLatLng sz1, SizeLatLng sz2)
	{
		return Add(sz1, sz2);
	}

	public static SizeLatLng operator -(SizeLatLng sz1, SizeLatLng sz2)
	{
		return Subtract(sz1, sz2);
	}

	public static bool operator ==(SizeLatLng sz1, SizeLatLng sz2)
	{
		if (sz1.WidthLng == sz2.WidthLng)
		{
			return sz1.HeightLat == sz2.HeightLat;
		}
		return false;
	}

	public static bool operator !=(SizeLatLng sz1, SizeLatLng sz2)
	{
		return !(sz1 == sz2);
	}

	public static explicit operator PointLatLng(SizeLatLng size)
	{
		return new PointLatLng(size.HeightLat, size.WidthLng);
	}

	public static SizeLatLng Add(SizeLatLng sz1, SizeLatLng sz2)
	{
		return new SizeLatLng(sz1.HeightLat + sz2.HeightLat, sz1.WidthLng + sz2.WidthLng);
	}

	public static SizeLatLng Subtract(SizeLatLng sz1, SizeLatLng sz2)
	{
		return new SizeLatLng(sz1.HeightLat - sz2.HeightLat, sz1.WidthLng - sz2.WidthLng);
	}

	public override bool Equals(object obj)
	{
		if (!(obj is SizeLatLng sizeLatLng))
		{
			return false;
		}
		if (sizeLatLng.WidthLng == WidthLng && sizeLatLng.HeightLat == HeightLat)
		{
			return sizeLatLng.GetType().Equals(GetType());
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (IsEmpty)
		{
			return 0;
		}
		return WidthLng.GetHashCode() ^ HeightLat.GetHashCode();
	}

	public PointLatLng ToPointLatLng()
	{
		return (PointLatLng)this;
	}

	public override string ToString()
	{
		return "{WidthLng=" + WidthLng.ToString(CultureInfo.CurrentCulture) + ", HeightLng=" + HeightLat.ToString(CultureInfo.CurrentCulture) + "}";
	}

	static SizeLatLng()
	{
		Empty = default(SizeLatLng);
	}
}
