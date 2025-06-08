using System;
using System.Globalization;

namespace AForge;

[Serializable]
public struct IntRange
{
	private int min;

	private int max;

	public int Min
	{
		get
		{
			return min;
		}
		set
		{
			min = value;
		}
	}

	public int Max
	{
		get
		{
			return max;
		}
		set
		{
			max = value;
		}
	}

	public int Length => max - min;

	public IntRange(int min, int max)
	{
		this.min = min;
		this.max = max;
	}

	public bool IsInside(int x)
	{
		if (x >= min)
		{
			return x <= max;
		}
		return false;
	}

	public bool IsInside(IntRange range)
	{
		if (IsInside(range.min))
		{
			return IsInside(range.max);
		}
		return false;
	}

	public bool IsOverlapping(IntRange range)
	{
		if (!IsInside(range.min) && !IsInside(range.max) && !range.IsInside(min))
		{
			return range.IsInside(max);
		}
		return true;
	}

	public static implicit operator Range(IntRange range)
	{
		return new Range(range.Min, range.Max);
	}

	public static bool operator ==(IntRange range1, IntRange range2)
	{
		if (range1.min == range2.min)
		{
			return range1.max == range2.max;
		}
		return false;
	}

	public static bool operator !=(IntRange range1, IntRange range2)
	{
		if (range1.min == range2.min)
		{
			return range1.max != range2.max;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is IntRange))
		{
			return false;
		}
		return this == (IntRange)obj;
	}

	public override int GetHashCode()
	{
		return min.GetHashCode() + max.GetHashCode();
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "{0}, {1}", new object[2] { min, max });
	}
}
