using System;
using System.Drawing;

namespace WinFormAnimation;

public class Float2D : IConvertible, IEquatable<Float2D>, IEquatable<Point>, IEquatable<PointF>, IEquatable<Size>, IEquatable<SizeF>
{
	public float X { get; set; }

	public float Y { get; set; }

	public Float2D(float x, float y)
	{
		X = x;
		Y = y;
	}

	public Float2D()
		: this(0f, 0f)
	{
	}

	public TypeCode GetTypeCode()
	{
		return TypeCode.Object;
	}

	public bool ToBoolean(IFormatProvider provider)
	{
		throw new InvalidCastException();
	}

	public byte ToByte(IFormatProvider provider)
	{
		throw new InvalidCastException();
	}

	public char ToChar(IFormatProvider provider)
	{
		throw new InvalidCastException();
	}

	public DateTime ToDateTime(IFormatProvider provider)
	{
		throw new InvalidCastException();
	}

	public decimal ToDecimal(IFormatProvider provider)
	{
		throw new InvalidCastException();
	}

	public double ToDouble(IFormatProvider provider)
	{
		throw new InvalidCastException();
	}

	public short ToInt16(IFormatProvider provider)
	{
		throw new InvalidCastException();
	}

	public int ToInt32(IFormatProvider provider)
	{
		throw new InvalidCastException();
	}

	public long ToInt64(IFormatProvider provider)
	{
		throw new InvalidCastException();
	}

	public sbyte ToSByte(IFormatProvider provider)
	{
		throw new InvalidCastException();
	}

	public float ToSingle(IFormatProvider provider)
	{
		throw new InvalidCastException();
	}

	public ushort ToUInt16(IFormatProvider provider)
	{
		throw new InvalidCastException();
	}

	public uint ToUInt32(IFormatProvider provider)
	{
		throw new InvalidCastException();
	}

	public ulong ToUInt64(IFormatProvider provider)
	{
		throw new InvalidCastException();
	}

	public string ToString(IFormatProvider provider)
	{
		return ToString();
	}

	public object ToType(Type conversionType, IFormatProvider provider)
	{
		if (conversionType == typeof(Point))
		{
			return (Point)this;
		}
		if (conversionType == typeof(Size))
		{
			return (Size)this;
		}
		if (conversionType == typeof(PointF))
		{
			return (PointF)this;
		}
		if (conversionType == typeof(SizeF))
		{
			return (SizeF)this;
		}
		throw new InvalidCastException();
	}

	public bool Equals(Float2D other)
	{
		return this == other;
	}

	public bool Equals(Point other)
	{
		return this == other;
	}

	public bool Equals(PointF other)
	{
		return this == other;
	}

	public bool Equals(Size other)
	{
		return this == other;
	}

	public bool Equals(SizeF other)
	{
		return this == other;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		Type type = obj.GetType();
		if (type == typeof(Point))
		{
			return this == (Point)obj;
		}
		if (type == typeof(PointF))
		{
			return this == (PointF)obj;
		}
		if (type == typeof(Size))
		{
			return this == (Size)obj;
		}
		if (type == typeof(SizeF))
		{
			return this == (SizeF)obj;
		}
		if (obj.GetType() == GetType())
		{
			return Equals((Float2D)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (X.GetHashCode() * 397) ^ Y.GetHashCode();
	}

	public static bool operator ==(Float2D left, Float2D right)
	{
		if ((object)left == null || (object)right == null)
		{
			if ((object)left == null)
			{
				return (object)right == null;
			}
			return false;
		}
		if ((object)left != right)
		{
			if ((double)left.X == (double)right.X)
			{
				return (double)left.Y == (double)right.Y;
			}
			return false;
		}
		return true;
	}

	public static bool operator !=(Float2D left, Float2D right)
	{
		return !(left == right);
	}

	public static implicit operator Size(Float2D float2D)
	{
		return new Size((int)float2D.X, (int)float2D.Y);
	}

	public static implicit operator Point(Float2D float2D)
	{
		return new Point((int)float2D.X, (int)float2D.Y);
	}

	public static implicit operator SizeF(Float2D float2D)
	{
		return new SizeF(float2D.X, float2D.Y);
	}

	public static implicit operator PointF(Float2D float2D)
	{
		return new PointF(float2D.X, float2D.Y);
	}

	public override string ToString()
	{
		return "(" + X.ToString("0.00") + "," + Y.ToString("0.00") + ")";
	}

	public static Float2D FromPoint(Point point)
	{
		return new Float2D(point.X, point.Y);
	}

	public static Float2D FromPoint(PointF point)
	{
		return new Float2D(point.X, point.Y);
	}

	public static Float2D FromSize(Size size)
	{
		return new Float2D(size.Width, size.Height);
	}

	public static Float2D FromSize(SizeF size)
	{
		return new Float2D(size.Width, size.Height);
	}
}
