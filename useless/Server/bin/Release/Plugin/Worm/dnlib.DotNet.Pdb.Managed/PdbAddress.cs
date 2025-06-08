using System;
using System.Diagnostics;
using dnlib.IO;

namespace dnlib.DotNet.Pdb.Managed;

[DebuggerDisplay("{Section}:{Offset}")]
internal readonly struct PdbAddress : IEquatable<PdbAddress>, IComparable<PdbAddress>
{
	public readonly ushort Section;

	public readonly uint Offset;

	public PdbAddress(ushort section, int offset)
	{
		Section = section;
		Offset = (uint)offset;
	}

	public PdbAddress(ushort section, uint offset)
	{
		Section = section;
		Offset = offset;
	}

	public static bool operator <=(PdbAddress a, PdbAddress b)
	{
		return a.CompareTo(b) <= 0;
	}

	public static bool operator <(PdbAddress a, PdbAddress b)
	{
		return a.CompareTo(b) < 0;
	}

	public static bool operator >=(PdbAddress a, PdbAddress b)
	{
		return a.CompareTo(b) >= 0;
	}

	public static bool operator >(PdbAddress a, PdbAddress b)
	{
		return a.CompareTo(b) > 0;
	}

	public static bool operator ==(PdbAddress a, PdbAddress b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(PdbAddress a, PdbAddress b)
	{
		return !a.Equals(b);
	}

	public int CompareTo(PdbAddress other)
	{
		if (Section != other.Section)
		{
			ushort section = Section;
			return section.CompareTo(other.Section);
		}
		uint offset = Offset;
		return offset.CompareTo(other.Offset);
	}

	public bool Equals(PdbAddress other)
	{
		if (Section == other.Section)
		{
			return Offset == other.Offset;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is PdbAddress))
		{
			return false;
		}
		return Equals((PdbAddress)obj);
	}

	public override int GetHashCode()
	{
		return (Section << 16) ^ (int)Offset;
	}

	public override string ToString()
	{
		return $"{Section:X4}:{Offset:X8}";
	}

	public static PdbAddress ReadAddress(ref DataReader reader)
	{
		uint offset = reader.ReadUInt32();
		return new PdbAddress(reader.ReadUInt16(), offset);
	}
}
