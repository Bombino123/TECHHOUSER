using System;
using System.Globalization;

namespace AForge;

[Serializable]
public struct Point
{
	public float X;

	public float Y;

	public Point(float x, float y)
	{
		X = x;
		Y = y;
	}

	public float DistanceTo(Point anotherPoint)
	{
		float num = X - anotherPoint.X;
		float num2 = Y - anotherPoint.Y;
		return (float)Math.Sqrt(num * num + num2 * num2);
	}

	public float SquaredDistanceTo(Point anotherPoint)
	{
		float num = X - anotherPoint.X;
		float num2 = Y - anotherPoint.Y;
		return num * num + num2 * num2;
	}

	public static Point operator +(Point point1, Point point2)
	{
		return new Point(point1.X + point2.X, point1.Y + point2.Y);
	}

	public static Point Add(Point point1, Point point2)
	{
		return new Point(point1.X + point2.X, point1.Y + point2.Y);
	}

	public static Point operator -(Point point1, Point point2)
	{
		return new Point(point1.X - point2.X, point1.Y - point2.Y);
	}

	public static Point Subtract(Point point1, Point point2)
	{
		return new Point(point1.X - point2.X, point1.Y - point2.Y);
	}

	public static Point operator +(Point point, float valueToAdd)
	{
		return new Point(point.X + valueToAdd, point.Y + valueToAdd);
	}

	public static Point Add(Point point, float valueToAdd)
	{
		return new Point(point.X + valueToAdd, point.Y + valueToAdd);
	}

	public static Point operator -(Point point, float valueToSubtract)
	{
		return new Point(point.X - valueToSubtract, point.Y - valueToSubtract);
	}

	public static Point Subtract(Point point, float valueToSubtract)
	{
		return new Point(point.X - valueToSubtract, point.Y - valueToSubtract);
	}

	public static Point operator *(Point point, float factor)
	{
		return new Point(point.X * factor, point.Y * factor);
	}

	public static Point Multiply(Point point, float factor)
	{
		return new Point(point.X * factor, point.Y * factor);
	}

	public static Point operator /(Point point, float factor)
	{
		return new Point(point.X / factor, point.Y / factor);
	}

	public static Point Divide(Point point, float factor)
	{
		return new Point(point.X / factor, point.Y / factor);
	}

	public static bool operator ==(Point point1, Point point2)
	{
		if (point1.X == point2.X)
		{
			return point1.Y == point2.Y;
		}
		return false;
	}

	public static bool operator !=(Point point1, Point point2)
	{
		if (point1.X == point2.X)
		{
			return point1.Y != point2.Y;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is Point))
		{
			return false;
		}
		return this == (Point)obj;
	}

	public override int GetHashCode()
	{
		return X.GetHashCode() + Y.GetHashCode();
	}

	public static explicit operator IntPoint(Point point)
	{
		return new IntPoint((int)point.X, (int)point.Y);
	}

	public static implicit operator DoublePoint(Point point)
	{
		return new DoublePoint(point.X, point.Y);
	}

	public IntPoint Round()
	{
		return new IntPoint((int)Math.Round(X), (int)Math.Round(Y));
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
