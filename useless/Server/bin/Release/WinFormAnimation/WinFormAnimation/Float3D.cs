using System;
using System.Drawing;

namespace WinFormAnimation;

public class Float3D : IConvertible, IEquatable<Float3D>, IEquatable<Color>
{
	public float X { get; set; }

	public float Y { get; set; }

	public float Z { get; set; }

	public Float3D(float x, float y, float z)
	{
		X = x;
		Y = y;
		Z = z;
	}

	public Float3D()
		: this(0f, 0f, 0f)
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
		if (conversionType == typeof(Color))
		{
			return (Color)this;
		}
		throw new InvalidCastException();
	}

	public bool Equals(Color other)
	{
		return this == other;
	}

	public bool Equals(Float3D other)
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
		if (obj.GetType() == typeof(Color))
		{
			return this == (Color)obj;
		}
		if (obj.GetType() == GetType())
		{
			return Equals((Float3D)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (((X.GetHashCode() * 397) ^ Y.GetHashCode()) * 397) ^ Z.GetHashCode();
	}

	public static bool operator ==(Float3D left, Float3D right)
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
			if ((double)left.X == (double)right.X && (double)left.Y == (double)right.Y)
			{
				return (double)left.Z == (double)right.Z;
			}
			return false;
		}
		return true;
	}

	public static bool operator !=(Float3D left, Float3D right)
	{
		return !(left == right);
	}

	public static implicit operator Color(Float3D float3D)
	{
		return Color.FromArgb((int)float3D.X, (int)float3D.Y, (int)float3D.Z);
	}

	public override string ToString()
	{
		return "(" + X.ToString("0.00") + "," + Y.ToString("0.00") + "," + Z.ToString("0.00") + ")";
	}

	public static Float3D FromColor(Color color)
	{
		return new Float3D((int)color.R, (int)color.G, (int)color.B);
	}
}
