using System;
using System.Globalization;

namespace AForge;

[Serializable]
public struct DoublePoint
{
	public double X;

	public double Y;

	public DoublePoint(double x, double y)
	{
		X = x;
		Y = y;
	}

	public double DistanceTo(DoublePoint anotherPoint)
	{
		double num = X - anotherPoint.X;
		double num2 = Y - anotherPoint.Y;
		return Math.Sqrt(num * num + num2 * num2);
	}

	public double SquaredDistanceTo(DoublePoint anotherPoint)
	{
		double num = X - anotherPoint.X;
		double num2 = Y - anotherPoint.Y;
		return num * num + num2 * num2;
	}

	public static DoublePoint operator +(DoublePoint point1, DoublePoint point2)
	{
		return new DoublePoint(point1.X + point2.X, point1.Y + point2.Y);
	}

	public static DoublePoint Add(DoublePoint point1, DoublePoint point2)
	{
		return new DoublePoint(point1.X + point2.X, point1.Y + point2.Y);
	}

	public static DoublePoint operator -(DoublePoint point1, DoublePoint point2)
	{
		return new DoublePoint(point1.X - point2.X, point1.Y - point2.Y);
	}

	public static DoublePoint Subtract(DoublePoint point1, DoublePoint point2)
	{
		return new DoublePoint(point1.X - point2.X, point1.Y - point2.Y);
	}

	public static DoublePoint operator +(DoublePoint point, double valueToAdd)
	{
		return new DoublePoint(point.X + valueToAdd, point.Y + valueToAdd);
	}

	public static DoublePoint Add(DoublePoint point, double valueToAdd)
	{
		return new DoublePoint(point.X + valueToAdd, point.Y + valueToAdd);
	}

	public static DoublePoint operator -(DoublePoint point, double valueToSubtract)
	{
		return new DoublePoint(point.X - valueToSubtract, point.Y - valueToSubtract);
	}

	public static DoublePoint Subtract(DoublePoint point, double valueToSubtract)
	{
		return new DoublePoint(point.X - valueToSubtract, point.Y - valueToSubtract);
	}

	public static DoublePoint operator *(DoublePoint point, double factor)
	{
		return new DoublePoint(point.X * factor, point.Y * factor);
	}

	public static DoublePoint Multiply(DoublePoint point, double factor)
	{
		return new DoublePoint(point.X * factor, point.Y * factor);
	}

	public static DoublePoint operator /(DoublePoint point, double factor)
	{
		return new DoublePoint(point.X / factor, point.Y / factor);
	}

	public static DoublePoint Divide(DoublePoint point, double factor)
	{
		return new DoublePoint(point.X / factor, point.Y / factor);
	}

	public static bool operator ==(DoublePoint point1, DoublePoint point2)
	{
		if (point1.X == point2.X)
		{
			return point1.Y == point2.Y;
		}
		return false;
	}

	public static bool operator !=(DoublePoint point1, DoublePoint point2)
	{
		if (point1.X == point2.X)
		{
			return point1.Y != point2.Y;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is DoublePoint))
		{
			return false;
		}
		return this == (DoublePoint)obj;
	}

	public override int GetHashCode()
	{
		return X.GetHashCode() + Y.GetHashCode();
	}

	public static explicit operator IntPoint(DoublePoint point)
	{
		return new IntPoint((int)point.X, (int)point.Y);
	}

	public static explicit operator Point(DoublePoint point)
	{
		return new Point((float)point.X, (float)point.Y);
	}

	public IntPoint Round()
	{
		return new IntPoint((int)Math.Round(X), (int)Math.Round(Y));
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "{0}, {1}", new object[2] { X, Y });
	}

	public double EuclideanNorm()
	{
		return Math.Sqrt(X * X + Y * Y);
	}
}
