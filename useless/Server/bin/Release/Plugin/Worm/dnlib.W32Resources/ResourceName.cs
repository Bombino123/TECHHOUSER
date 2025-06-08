using System;
using System.Runtime.InteropServices;

namespace dnlib.W32Resources;

[ComVisible(true)]
public readonly struct ResourceName : IComparable<ResourceName>, IEquatable<ResourceName>
{
	private readonly int id;

	private readonly string name;

	public bool HasId => name == null;

	public bool HasName => name != null;

	public int Id => id;

	public string Name => name;

	public ResourceName(int id)
	{
		this.id = id;
		name = null;
	}

	public ResourceName(string name)
	{
		id = 0;
		this.name = name;
	}

	public static implicit operator ResourceName(int id)
	{
		return new ResourceName(id);
	}

	public static implicit operator ResourceName(string name)
	{
		return new ResourceName(name);
	}

	public static bool operator <(ResourceName left, ResourceName right)
	{
		return left.CompareTo(right) < 0;
	}

	public static bool operator <=(ResourceName left, ResourceName right)
	{
		return left.CompareTo(right) <= 0;
	}

	public static bool operator >(ResourceName left, ResourceName right)
	{
		return left.CompareTo(right) > 0;
	}

	public static bool operator >=(ResourceName left, ResourceName right)
	{
		return left.CompareTo(right) >= 0;
	}

	public static bool operator ==(ResourceName left, ResourceName right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ResourceName left, ResourceName right)
	{
		return !left.Equals(right);
	}

	public int CompareTo(ResourceName other)
	{
		if (HasId != other.HasId)
		{
			if (!HasName)
			{
				return 1;
			}
			return -1;
		}
		if (HasId)
		{
			int num = id;
			return num.CompareTo(other.id);
		}
		return name.ToUpperInvariant().CompareTo(other.name.ToUpperInvariant());
	}

	public bool Equals(ResourceName other)
	{
		return CompareTo(other) == 0;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ResourceName))
		{
			return false;
		}
		return Equals((ResourceName)obj);
	}

	public override int GetHashCode()
	{
		if (HasId)
		{
			return id;
		}
		return name.GetHashCode();
	}

	public override string ToString()
	{
		if (!HasId)
		{
			return name;
		}
		int num = id;
		return num.ToString();
	}
}
