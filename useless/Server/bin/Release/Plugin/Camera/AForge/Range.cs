using System;
using System.Globalization;

namespace AForge;

[Serializable]
public struct Range
{
	private float min;

	private float max;

	public float Min
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

	public float Max
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

	public float Length => max - min;

	public Range(float min, float max)
	{
		this.min = min;
		this.max = max;
	}

	public bool IsInside(float x)
	{
		if (x >= min)
		{
			return x <= max;
		}
		return false;
	}

	public bool IsInside(Range range)
	{
		if (IsInside(range.min))
		{
			return IsInside(range.max);
		}
		return false;
	}

	public bool IsOverlapping(Range range)
	{
		if (!IsInside(range.min) && !IsInside(range.max) && !range.IsInside(min))
		{
			return range.IsInside(max);
		}
		return true;
	}

	public IntRange ToIntRange(bool provideInnerRange)
	{
		int num;
		int num2;
		if (provideInnerRange)
		{
			num = (int)Math.Ceiling(min);
			num2 = (int)Math.Floor(max);
		}
		else
		{
			num = (int)Math.Floor(min);
			num2 = (int)Math.Ceiling(max);
		}
		return new IntRange(num, num2);
	}

	public static bool operator ==(Range range1, Range range2)
	{
		if (range1.min == range2.min)
		{
			return range1.max == range2.max;
		}
		return false;
	}

	public static bool operator !=(Range range1, Range range2)
	{
		if (range1.min == range2.min)
		{
			return range1.max != range2.max;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is Range))
		{
			return false;
		}
		return this == (Range)obj;
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
