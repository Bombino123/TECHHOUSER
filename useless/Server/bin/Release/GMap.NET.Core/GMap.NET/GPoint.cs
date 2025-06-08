using System;
using System.Globalization;

namespace GMap.NET;

[Serializable]
public struct GPoint
{
	public static readonly GPoint Empty;

	public bool IsEmpty
	{
		get
		{
			if (X == 0L)
			{
				return Y == 0;
			}
			return false;
		}
	}

	public long X { get; set; }

	public long Y { get; set; }

	public GPoint(long x, long y)
	{
		X = x;
		Y = y;
	}

	public GPoint(GSize sz)
	{
		X = sz.Width;
		Y = sz.Height;
	}

	public static explicit operator GSize(GPoint p)
	{
		return new GSize(p.X, p.Y);
	}

	public static GPoint operator +(GPoint pt, GSize sz)
	{
		return Add(pt, sz);
	}

	public static GPoint operator -(GPoint pt, GSize sz)
	{
		return Subtract(pt, sz);
	}

	public static bool operator ==(GPoint left, GPoint right)
	{
		if (left.X == right.X)
		{
			return left.Y == right.Y;
		}
		return false;
	}

	public static bool operator !=(GPoint left, GPoint right)
	{
		return !(left == right);
	}

	public static GPoint Add(GPoint pt, GSize sz)
	{
		return new GPoint(pt.X + sz.Width, pt.Y + sz.Height);
	}

	public static GPoint Subtract(GPoint pt, GSize sz)
	{
		return new GPoint(pt.X - sz.Width, pt.Y - sz.Height);
	}

	public override bool Equals(object obj)
	{
		if (!(obj is GPoint gPoint))
		{
			return false;
		}
		if (gPoint.X == X)
		{
			return gPoint.Y == Y;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (int)(X ^ Y);
	}

	public void Offset(long dx, long dy)
	{
		X += dx;
		Y += dy;
	}

	public void Offset(GPoint p)
	{
		Offset(p.X, p.Y);
	}

	public void OffsetNegative(GPoint p)
	{
		Offset(-p.X, -p.Y);
	}

	public override string ToString()
	{
		return "{X=" + X.ToString(CultureInfo.CurrentCulture) + ",Y=" + Y.ToString(CultureInfo.CurrentCulture) + "}";
	}
}
