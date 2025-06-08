using System;
using System.Globalization;

namespace AForge;

[Serializable]
public struct IntPoint
{
	public int X;

	public int Y;

	public IntPoint(int x, int y)
	{
		X = x;
		Y = y;
	}

	public float DistanceTo(IntPoint anotherPoint)
	{
		int num = X - anotherPoint.X;
		int num2 = Y - anotherPoint.Y;
		return (float)Math.Sqrt(num * num + num2 * num2);
	}

	public float SquaredDistanceTo(Point anotherPoint)
	{
		float num = (float)X - anotherPoint.X;
		float num2 = (float)Y - anotherPoint.Y;
		return num * num + num2 * num2;
	}

	public static IntPoint operator +(IntPoint point1, IntPoint point2)
	{
		return new IntPoint(point1.X + point2.X, point1.Y + point2.Y);
	}

	public static IntPoint Add(IntPoint point1, IntPoint point2)
	{
		return new IntPoint(point1.X + point2.X, point1.Y + point2.Y);
	}

	public static IntPoint operator -(IntPoint point1, IntPoint point2)
	{
		return new IntPoint(point1.X - point2.X, point1.Y - point2.Y);
	}

	public static IntPoint Subtract(IntPoint point1, IntPoint point2)
	{
		return new IntPoint(point1.X - point2.X, point1.Y - point2.Y);
	}

	public static IntPoint operator +(IntPoint point, int valueToAdd)
	{
		return new IntPoint(point.X + valueToAdd, point.Y + valueToAdd);
	}

	public static IntPoint Add(IntPoint point, int valueToAdd)
	{
		return new IntPoint(point.X + valueToAdd, point.Y + valueToAdd);
	}

	public static IntPoint operator -(IntPoint point, int valueToSubtract)
	{
		return new IntPoint(point.X - valueToSubtract, point.Y - valueToSubtract);
	}

	public static IntPoint Subtract(IntPoint point, int valueToSubtract)
	{
		return new IntPoint(point.X - valueToSubtract, point.Y - valueToSubtract);
	}

	public static IntPoint operator *(IntPoint point, int factor)
	{
		return new IntPoint(point.X * factor, point.Y * factor);
	}

	public static IntPoint Multiply(IntPoint point, int factor)
	{
		return new IntPoint(point.X * factor, point.Y * factor);
	}

	public static IntPoint operator /(IntPoint point, int factor)
	{
		return new IntPoint(point.X / factor, point.Y / factor);
	}

	public static IntPoint Divide(IntPoint point, int factor)
	{
		return new IntPoint(point.X / factor, point.Y / factor);
	}

	public static bool operator ==(IntPoint point1, IntPoint point2)
	{
		if (point1.X == point2.X)
		{
			return point1.Y == point2.Y;
		}
		return false;
	}

	public static bool operator !=(IntPoint point1, IntPoint point2)
	{
		if (point1.X == point2.X)
		{
			return point1.Y != point2.Y;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is IntPoint))
		{
			return false;
		}
		return this == (IntPoint)obj;
	}

	public override int GetHashCode()
	{
		return X.GetHashCode() + Y.GetHashCode();
	}

	public static implicit operator Point(IntPoint point)
	{
		return new Point(point.X, point.Y);
	}

	public static implicit operator DoublePoint(IntPoint point)
	{
		return new DoublePoint(point.X, point.Y);
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "{0}, {1}", new object[2] { X, Y });
	}

	public float EuclideanNorm()
	{
		return (float)Math.Sqrt(X * X + Y * Y);
	}
}
