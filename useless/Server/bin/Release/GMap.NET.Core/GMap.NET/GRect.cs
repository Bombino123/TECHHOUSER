using System;
using System.Globalization;

namespace GMap.NET;

public struct GRect
{
	public static readonly GRect Empty;

	public GPoint Location
	{
		get
		{
			return new GPoint(X, Y);
		}
		set
		{
			X = value.X;
			Y = value.Y;
		}
	}

	public GPoint RightBottom => new GPoint(Right, Bottom);

	public GPoint RightTop => new GPoint(Right, Top);

	public GPoint LeftBottom => new GPoint(Left, Bottom);

	public GSize Size
	{
		get
		{
			return new GSize(Width, Height);
		}
		set
		{
			Width = value.Width;
			Height = value.Height;
		}
	}

	public long X { get; set; }

	public long Y { get; set; }

	public long Width { get; set; }

	public long Height { get; set; }

	public long Left => X;

	public long Top => Y;

	public long Right => X + Width;

	public long Bottom => Y + Height;

	public bool IsEmpty
	{
		get
		{
			if (Height == 0L && Width == 0L && X == 0L)
			{
				return Y == 0;
			}
			return false;
		}
	}

	public GRect(long x, long y, long width, long height)
	{
		X = x;
		Y = y;
		Width = width;
		Height = height;
	}

	public GRect(GPoint location, GSize size)
	{
		X = location.X;
		Y = location.Y;
		Width = size.Width;
		Height = size.Height;
	}

	public static GRect FromLTRB(int left, int top, int right, int bottom)
	{
		return new GRect(left, top, right - left, bottom - top);
	}

	public override bool Equals(object obj)
	{
		if (!(obj is GRect gRect))
		{
			return false;
		}
		if (gRect.X == X && gRect.Y == Y && gRect.Width == Width)
		{
			return gRect.Height == Height;
		}
		return false;
	}

	public static bool operator ==(GRect left, GRect right)
	{
		if (left.X == right.X && left.Y == right.Y && left.Width == right.Width)
		{
			return left.Height == right.Height;
		}
		return false;
	}

	public static bool operator !=(GRect left, GRect right)
	{
		return !(left == right);
	}

	public bool Contains(long x, long y)
	{
		if (X <= x && x < X + Width && Y <= y)
		{
			return y < Y + Height;
		}
		return false;
	}

	public bool Contains(GPoint pt)
	{
		return Contains(pt.X, pt.Y);
	}

	public bool Contains(GRect rect)
	{
		if (X <= rect.X && rect.X + rect.Width <= X + Width && Y <= rect.Y)
		{
			return rect.Y + rect.Height <= Y + Height;
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (IsEmpty)
		{
			return 0;
		}
		return (int)(X ^ ((Y << 13) | (Y >> 19)) ^ ((Width << 26) | (Width >> 6)) ^ ((Height << 7) | (Height >> 25)));
	}

	public void Inflate(long width, long height)
	{
		X -= width;
		Y -= height;
		Width += 2 * width;
		Height += 2 * height;
	}

	public void Inflate(GSize size)
	{
		Inflate(size.Width, size.Height);
	}

	public static GRect Inflate(GRect rect, long x, long y)
	{
		GRect result = rect;
		result.Inflate(x, y);
		return result;
	}

	public void Intersect(GRect rect)
	{
		GRect gRect = Intersect(rect, this);
		X = gRect.X;
		Y = gRect.Y;
		Width = gRect.Width;
		Height = gRect.Height;
	}

	public static GRect Intersect(GRect a, GRect b)
	{
		long num = Math.Max(a.X, b.X);
		long num2 = Math.Min(a.X + a.Width, b.X + b.Width);
		long num3 = Math.Max(a.Y, b.Y);
		long num4 = Math.Min(a.Y + a.Height, b.Y + b.Height);
		if (num2 >= num && num4 >= num3)
		{
			return new GRect(num, num3, num2 - num, num4 - num3);
		}
		return Empty;
	}

	public bool IntersectsWith(GRect rect)
	{
		if (rect.X < X + Width && X < rect.X + rect.Width && rect.Y < Y + Height)
		{
			return Y < rect.Y + rect.Height;
		}
		return false;
	}

	public static GRect Union(GRect a, GRect b)
	{
		long num = Math.Min(a.X, b.X);
		long num2 = Math.Max(a.X + a.Width, b.X + b.Width);
		long num3 = Math.Min(a.Y, b.Y);
		long num4 = Math.Max(a.Y + a.Height, b.Y + b.Height);
		return new GRect(num, num3, num2 - num, num4 - num3);
	}

	public void Offset(GPoint pos)
	{
		Offset(pos.X, pos.Y);
	}

	public void OffsetNegative(GPoint pos)
	{
		Offset(-pos.X, -pos.Y);
	}

	public void Offset(long x, long y)
	{
		X += x;
		Y += y;
	}

	public override string ToString()
	{
		return "{X=" + X.ToString(CultureInfo.CurrentCulture) + ",Y=" + Y.ToString(CultureInfo.CurrentCulture) + ",Width=" + Width.ToString(CultureInfo.CurrentCulture) + ",Height=" + Height.ToString(CultureInfo.CurrentCulture) + "}";
	}
}
