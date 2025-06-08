using System;

namespace AntdUI;

public class Math3D
{
	public class Point3D
	{
		public double X;

		public double Y;

		public double Z;

		public Point3D(int x, int y, int z)
		{
			X = x;
			Y = y;
			Z = 1.0;
		}

		public Point3D(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public Point3D(double x, double y, double z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public Point3D()
		{
		}

		public override string ToString()
		{
			return "(" + X + ", " + Y + ", " + Z + ")";
		}
	}

	public class Camera
	{
		public Point3D Position = new Point3D();
	}

	public static Point3D RotateX(Point3D point3D, double degrees)
	{
		double num = Math.PI * degrees / 180.0;
		double num2 = Math.Cos(num);
		double num3 = Math.Sin(num);
		double y = point3D.Y * num2 + point3D.Z * num3;
		double z = point3D.Y * (0.0 - num3) + point3D.Z * num2;
		return new Point3D(point3D.X, y, z);
	}

	public static Point3D RotateY(Point3D point3D, double degrees)
	{
		double num = Math.PI * degrees / 180.0;
		double num2 = Math.Cos(num);
		double num3 = Math.Sin(num);
		return new Point3D(point3D.X * num2 + point3D.Z * num3, z: point3D.X * (0.0 - num3) + point3D.Z * num2, y: point3D.Y);
	}

	public static Point3D RotateZ(Point3D point3D, double degrees)
	{
		double num = Math.PI * degrees / 180.0;
		double num2 = Math.Cos(num);
		double num3 = Math.Sin(num);
		double x = point3D.X * num2 + point3D.Y * num3;
		double y = point3D.X * (0.0 - num3) + point3D.Y * num2;
		return new Point3D(x, y, point3D.Z);
	}

	public static Point3D Translate(Point3D points3D, Point3D oldOrigin, Point3D newOrigin)
	{
		Point3D point3D = new Point3D(newOrigin.X - oldOrigin.X, newOrigin.Y - oldOrigin.Y, newOrigin.Z - oldOrigin.Z);
		points3D.X += point3D.X;
		points3D.Y += point3D.Y;
		points3D.Z += point3D.Z;
		return points3D;
	}

	public static Point3D[] RotateX(Point3D[] points3D, double degrees)
	{
		for (int i = 0; i < points3D.Length; i++)
		{
			points3D[i] = RotateX(points3D[i], degrees);
		}
		return points3D;
	}

	public static Point3D[] RotateY(Point3D[] points3D, double degrees)
	{
		for (int i = 0; i < points3D.Length; i++)
		{
			points3D[i] = RotateY(points3D[i], degrees);
		}
		return points3D;
	}

	public static Point3D[] RotateZ(Point3D[] points3D, double degrees)
	{
		for (int i = 0; i < points3D.Length; i++)
		{
			points3D[i] = RotateZ(points3D[i], degrees);
		}
		return points3D;
	}

	public static Point3D[] Translate(Point3D[] points3D, Point3D oldOrigin, Point3D newOrigin)
	{
		for (int i = 0; i < points3D.Length; i++)
		{
			points3D[i] = Translate(points3D[i], oldOrigin, newOrigin);
		}
		return points3D;
	}
}
