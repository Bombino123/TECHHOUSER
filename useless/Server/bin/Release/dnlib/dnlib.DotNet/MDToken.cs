using System;
using System.Diagnostics;
using dnlib.DotNet.MD;

namespace dnlib.DotNet;

[DebuggerDisplay("{Table} {Rid}")]
public readonly struct MDToken : IEquatable<MDToken>, IComparable<MDToken>
{
	public const uint RID_MASK = 16777215u;

	public const uint RID_MAX = 16777215u;

	public const int TABLE_SHIFT = 24;

	private readonly uint token;

	public Table Table => ToTable(token);

	public uint Rid => ToRID(token);

	public uint Raw => token;

	public bool IsNull => Rid == 0;

	public MDToken(uint token)
	{
		this.token = token;
	}

	public MDToken(int token)
		: this((uint)token)
	{
	}

	public MDToken(Table table, uint rid)
		: this(((uint)table << 24) | rid)
	{
	}

	public MDToken(Table table, int rid)
		: this(((uint)table << 24) | (uint)rid)
	{
	}

	public static uint ToRID(uint token)
	{
		return token & 0xFFFFFFu;
	}

	public static uint ToRID(int token)
	{
		return ToRID((uint)token);
	}

	public static Table ToTable(uint token)
	{
		return (Table)(token >> 24);
	}

	public static Table ToTable(int token)
	{
		return ToTable((uint)token);
	}

	public int ToInt32()
	{
		return (int)token;
	}

	public uint ToUInt32()
	{
		return token;
	}

	public static bool operator ==(MDToken left, MDToken right)
	{
		return left.CompareTo(right) == 0;
	}

	public static bool operator !=(MDToken left, MDToken right)
	{
		return left.CompareTo(right) != 0;
	}

	public static bool operator <(MDToken left, MDToken right)
	{
		return left.CompareTo(right) < 0;
	}

	public static bool operator >(MDToken left, MDToken right)
	{
		return left.CompareTo(right) > 0;
	}

	public static bool operator <=(MDToken left, MDToken right)
	{
		return left.CompareTo(right) <= 0;
	}

	public static bool operator >=(MDToken left, MDToken right)
	{
		return left.CompareTo(right) >= 0;
	}

	public int CompareTo(MDToken other)
	{
		uint num = token;
		return num.CompareTo(other.token);
	}

	public bool Equals(MDToken other)
	{
		return CompareTo(other) == 0;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is MDToken))
		{
			return false;
		}
		return Equals((MDToken)obj);
	}

	public override int GetHashCode()
	{
		return (int)token;
	}

	public override string ToString()
	{
		uint num = token;
		return num.ToString("X8");
	}
}
