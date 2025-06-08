using System.Globalization;

namespace GMap.NET;

public struct GSize
{
	public static readonly GSize Empty;

	public bool IsEmpty
	{
		get
		{
			if (Width == 0L)
			{
				return Height == 0;
			}
			return false;
		}
	}

	public long Width { get; set; }

	public long Height { get; set; }

	public GSize(GPoint pt)
	{
		Width = pt.X;
		Height = pt.Y;
	}

	public GSize(long width, long height)
	{
		Width = width;
		Height = height;
	}

	public static GSize operator +(GSize sz1, GSize sz2)
	{
		return Add(sz1, sz2);
	}

	public static GSize operator -(GSize sz1, GSize sz2)
	{
		return Subtract(sz1, sz2);
	}

	public static bool operator ==(GSize sz1, GSize sz2)
	{
		if (sz1.Width == sz2.Width)
		{
			return sz1.Height == sz2.Height;
		}
		return false;
	}

	public static bool operator !=(GSize sz1, GSize sz2)
	{
		return !(sz1 == sz2);
	}

	public static explicit operator GPoint(GSize size)
	{
		return new GPoint(size.Width, size.Height);
	}

	public static GSize Add(GSize sz1, GSize sz2)
	{
		return new GSize(sz1.Width + sz2.Width, sz1.Height + sz2.Height);
	}

	public static GSize Subtract(GSize sz1, GSize sz2)
	{
		return new GSize(sz1.Width - sz2.Width, sz1.Height - sz2.Height);
	}

	public override bool Equals(object obj)
	{
		if (!(obj is GSize gSize))
		{
			return false;
		}
		if (gSize.Width == Width)
		{
			return gSize.Height == Height;
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (IsEmpty)
		{
			return 0;
		}
		return Width.GetHashCode() ^ Height.GetHashCode();
	}

	public override string ToString()
	{
		return "{Width=" + Width.ToString(CultureInfo.CurrentCulture) + ", Height=" + Height.ToString(CultureInfo.CurrentCulture) + "}";
	}
}
