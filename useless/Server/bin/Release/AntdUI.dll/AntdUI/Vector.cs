using System;
using System.Drawing;

namespace AntdUI;

public struct Vector
{
	private double _x;

	private double _y;

	public double X
	{
		get
		{
			return _x;
		}
		set
		{
			_x = value;
		}
	}

	public double Y
	{
		get
		{
			return _y;
		}
		set
		{
			_y = value;
		}
	}

	public double Magnitude => Math.Sqrt(X * X + Y * Y);

	public Vector(double x, double y)
	{
		_x = x;
		_y = y;
	}

	public Vector(PointF pt)
	{
		_x = pt.X;
		_y = pt.Y;
	}

	public Vector(PointF st, PointF end)
	{
		_x = end.X - st.X;
		_y = end.Y - st.Y;
	}

	public static Vector operator +(Vector v1, Vector v2)
	{
		return new Vector(v1.X + v2.X, v1.Y + v2.Y);
	}

	public static Vector operator -(Vector v1, Vector v2)
	{
		return new Vector(v1.X - v2.X, v1.Y - v2.Y);
	}

	public static Vector operator -(Vector v)
	{
		return new Vector(0.0 - v.X, 0.0 - v.Y);
	}

	public static Vector operator *(double c, Vector v)
	{
		return new Vector(c * v.X, c * v.Y);
	}

	public static Vector operator *(Vector v, double c)
	{
		return new Vector(c * v.X, c * v.Y);
	}

	public static Vector operator /(Vector v, double c)
	{
		return new Vector(v.X / c, v.Y / c);
	}

	public double CrossProduct(Vector v)
	{
		return _x * v.Y - v.X * _y;
	}

	public double DotProduct(Vector v)
	{
		return _x * v.X + _y * v.Y;
	}

	public static bool IsClockwise(PointF pt1, PointF pt2, PointF pt3)
	{
		Vector vector = new Vector(pt2, pt1);
		Vector v = new Vector(pt2, pt3);
		return vector.CrossProduct(v) < 0.0;
	}

	public static bool IsCCW(PointF pt1, PointF pt2, PointF pt3)
	{
		Vector vector = new Vector(pt2, pt1);
		Vector v = new Vector(pt2, pt3);
		return vector.CrossProduct(v) > 0.0;
	}

	public static double DistancePointLine(PointF pt, PointF lnA, PointF lnB)
	{
		Vector v = new Vector(lnA, lnB);
		Vector vector = new Vector(lnA, pt);
		v /= v.Magnitude;
		return Math.Abs(vector.CrossProduct(v));
	}

	public void Rotate(int degree)
	{
		double num = (double)degree * Math.PI / 180.0;
		double num2 = Math.Sin(num);
		double num3 = Math.Cos(num);
		double x = _x * num3 - _y * num2;
		double y = _x * num2 + _y * num3;
		_x = x;
		_y = y;
	}

	public PointF ToPointF()
	{
		return new PointF((float)_x, (float)_y);
	}
}
