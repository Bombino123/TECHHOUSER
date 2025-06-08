using System;
using System.Drawing;
using System.Windows.Forms;

namespace AntdUI;

public class Cube
{
	public int width;

	public int height;

	public int depth;

	private double xRotation;

	private double yRotation;

	private double zRotation;

	private Math3D.Camera camera1 = new Math3D.Camera();

	private Math3D.Point3D cubeOrigin;

	public Point a;

	public Point b;

	public Point c;

	public Point d;

	public double RotateX
	{
		get
		{
			return xRotation;
		}
		set
		{
			xRotation = value;
		}
	}

	public double RotateY
	{
		get
		{
			return yRotation;
		}
		set
		{
			yRotation = value;
		}
	}

	public double RotateZ
	{
		get
		{
			return zRotation;
		}
		set
		{
			zRotation = value;
		}
	}

	public float CY
	{
		get
		{
			if (a.Y < d.Y)
			{
				return (float)a.Y + (float)height / 2f;
			}
			return (float)d.Y + (float)height / 2f;
		}
	}

	public float CX
	{
		get
		{
			if (c.X < d.X)
			{
				return (float)c.X + (float)width / 2f;
			}
			return (float)d.X + (float)width / 2f;
		}
	}

	public Cube(int Width, int Height, int Depth)
	{
		width = Width;
		height = Height;
		depth = Depth;
		cubeOrigin = new Math3D.Point3D(width / 2, height / 2, depth / 2);
	}

	public static Rectangle getBounds(PointF[] points)
	{
		double num = points[0].X;
		double num2 = points[0].X;
		double num3 = points[0].Y;
		double num4 = points[0].Y;
		for (int i = 1; i < points.Length; i++)
		{
			if ((double)points[i].X < num)
			{
				num = points[i].X;
			}
			if ((double)points[i].X > num2)
			{
				num2 = points[i].X;
			}
			if ((double)points[i].Y < num3)
			{
				num3 = points[i].Y;
			}
			if ((double)points[i].Y > num4)
			{
				num4 = points[i].Y;
			}
		}
		return new Rectangle(0, 0, (int)Math.Round(num2 - num), (int)Math.Round(num4 - num3));
	}

	public void calcCube(Point drawOrigin)
	{
		PointF[] array = new PointF[24];
		Point point = new Point(0, 0);
		Math3D.Point3D point3D = new Math3D.Point3D(0, 0, 0);
		double num = (double)Screen.PrimaryScreen.Bounds.Width / 1.5;
		Math3D.Point3D[] array2 = fillCubeVertices(width, height, depth);
		Math3D.Point3D point3D2 = array2[4];
		double z = 0.0 - (point3D2.X - cubeOrigin.X) * num / cubeOrigin.X + point3D2.Z;
		camera1.Position = new Math3D.Point3D(cubeOrigin.X, cubeOrigin.Y, z);
		array2 = Math3D.Translate(array2, cubeOrigin, point3D);
		array2 = Math3D.RotateX(array2, xRotation);
		array2 = Math3D.RotateY(array2, yRotation);
		array2 = Math3D.RotateZ(array2, zRotation);
		array2 = Math3D.Translate(array2, point3D, cubeOrigin);
		for (int i = 0; i < array.Length; i++)
		{
			Math3D.Point3D point3D3 = array2[i];
			if (point3D3.Z - camera1.Position.Z >= 0.0)
			{
				array[i].X = (int)((0.0 - (point3D3.X - camera1.Position.X)) / -0.10000000149011612 * num) + drawOrigin.X;
				array[i].Y = (int)((point3D3.Y - camera1.Position.Y) / -0.10000000149011612 * num) + drawOrigin.Y;
				continue;
			}
			point.X = (int)((cubeOrigin.X - camera1.Position.X) / (cubeOrigin.Z - camera1.Position.Z) * num) + drawOrigin.X;
			point.Y = (int)((0.0 - (cubeOrigin.Y - camera1.Position.Y)) / (cubeOrigin.Z - camera1.Position.Z) * num) + drawOrigin.Y;
			array[i].X = (float)((point3D3.X - camera1.Position.X) / (point3D3.Z - camera1.Position.Z) * num + (double)drawOrigin.X);
			array[i].Y = (float)((0.0 - (point3D3.Y - camera1.Position.Y)) / (point3D3.Z - camera1.Position.Z) * num + (double)drawOrigin.Y);
			array[i].X = (int)array[i].X;
			array[i].Y = (int)array[i].Y;
		}
		a = Point.Round(array[4]);
		b = Point.Round(array[5]);
		c = Point.Round(array[6]);
		d = Point.Round(array[7]);
	}

	public PointF Centre()
	{
		return new PointF(CX, CY);
	}

	public PointF CentreY()
	{
		return new PointF((float)d.X + (float)width / 2f, CY);
	}

	public PointF CentreX()
	{
		return new PointF(CX, (float)d.Y + (float)height / 2f);
	}

	public static Math3D.Point3D[] fillCubeVertices(int width, int height, int depth)
	{
		return new Math3D.Point3D[24]
		{
			new Math3D.Point3D(0, 0, 0),
			new Math3D.Point3D(0, height, 0),
			new Math3D.Point3D(width, height, 0),
			new Math3D.Point3D(width, 0, 0),
			new Math3D.Point3D(0, 0, depth),
			new Math3D.Point3D(0, height, depth),
			new Math3D.Point3D(width, height, depth),
			new Math3D.Point3D(width, 0, depth),
			new Math3D.Point3D(0, 0, 0),
			new Math3D.Point3D(0, 0, depth),
			new Math3D.Point3D(0, height, depth),
			new Math3D.Point3D(0, height, 0),
			new Math3D.Point3D(width, 0, 0),
			new Math3D.Point3D(width, 0, depth),
			new Math3D.Point3D(width, height, depth),
			new Math3D.Point3D(width, height, 0),
			new Math3D.Point3D(0, height, 0),
			new Math3D.Point3D(0, height, depth),
			new Math3D.Point3D(width, height, depth),
			new Math3D.Point3D(width, height, 0),
			new Math3D.Point3D(0, 0, 0),
			new Math3D.Point3D(0, 0, depth),
			new Math3D.Point3D(width, 0, depth),
			new Math3D.Point3D(width, 0, 0)
		};
	}

	public Bitmap ToBitmap(Bitmap bmp)
	{
		PointF[] array = new PointF[4] { d, a, b, c };
		using ImageData imageData = new ImageData();
		imageData.FromBitmap(bmp);
		int num = ((Image)bmp).Height;
		int num2 = ((Image)bmp).Width;
		float num3 = float.MaxValue;
		float num4 = num3;
		float num5 = float.MinValue;
		float num6 = num5;
		for (int i = 0; i < 4; i++)
		{
			num5 = Math.Max(num5, array[i].X);
			num6 = Math.Max(num6, array[i].Y);
			num3 = Math.Min(num3, array[i].X);
			num4 = Math.Min(num4, array[i].Y);
		}
		Rectangle rectangle = new Rectangle((int)num3, (int)num4, (int)(num5 - num3), (int)(num6 - num4));
		Vector v = new Vector(array[0], array[1]);
		Vector v2 = new Vector(array[1], array[2]);
		Vector v3 = new Vector(array[2], array[3]);
		Vector v4 = new Vector(array[3], array[0]);
		v /= v.Magnitude;
		v2 /= v2.Magnitude;
		v3 /= v3.Magnitude;
		v4 /= v4.Magnitude;
		using ImageData imageData2 = new ImageData();
		imageData2.A = new byte[rectangle.Width, rectangle.Height];
		imageData2.B = new byte[rectangle.Width, rectangle.Height];
		imageData2.G = new byte[rectangle.Width, rectangle.Height];
		imageData2.R = new byte[rectangle.Width, rectangle.Height];
		PointF pointF = default(PointF);
		int num7 = 0;
		while (++num7 < rectangle.Height)
		{
			int num8 = 0;
			while (++num8 < rectangle.Width)
			{
				Point point = new Point(num8, num7);
				point.Offset(rectangle.Location);
				if (Vector.IsCCW(point, array[0], array[1]) || Vector.IsCCW(point, array[1], array[2]) || Vector.IsCCW(point, array[2], array[3]) || Vector.IsCCW(point, array[3], array[0]))
				{
					continue;
				}
				double num9 = Math.Abs(new Vector(array[0], point).CrossProduct(v));
				double num10 = Math.Abs(new Vector(array[1], point).CrossProduct(v2));
				double num11 = Math.Abs(new Vector(array[2], point).CrossProduct(v3));
				double num12 = Math.Abs(new Vector(array[3], point).CrossProduct(v4));
				pointF.X = (float)((double)num2 * (num12 / (num12 + num10)));
				pointF.Y = (float)((double)num * (num9 / (num9 + num11)));
				int num13 = (int)pointF.X;
				int num14 = (int)pointF.Y;
				if (num13 >= 0 && num13 < num2 && num14 >= 0 && num14 < num)
				{
					int num15 = ((num13 == num2 - 1) ? num13 : (num13 + 1));
					int num16 = ((num14 == num - 1) ? num14 : (num14 + 1));
					float num17 = pointF.X - (float)num13;
					if (num17 < 0f)
					{
						num17 = 0f;
					}
					num17 = 1f - num17;
					float num18 = 1f - num17;
					float num19 = pointF.Y - (float)num14;
					if (num19 < 0f)
					{
						num19 = 0f;
					}
					num19 = 1f - num19;
					float num20 = 1f - num19;
					float num21 = num17 * num19;
					float num22 = num17 * num20;
					float num23 = num18 * num19;
					float num24 = num18 * num20;
					float num25 = (float)(int)imageData.A[num13, num14] * num21 + (float)(int)imageData.A[num15, num14] * num23 + (float)(int)imageData.A[num13, num16] * num22 + (float)(int)imageData.A[num15, num16] * num24;
					imageData2.A[num8, num7] = (byte)num25;
					num25 = (float)(int)imageData.B[num13, num14] * num21 + (float)(int)imageData.B[num15, num14] * num23 + (float)(int)imageData.B[num13, num16] * num22 + (float)(int)imageData.B[num15, num16] * num24;
					imageData2.B[num8, num7] = (byte)num25;
					num25 = (float)(int)imageData.G[num13, num14] * num21 + (float)(int)imageData.G[num15, num14] * num23 + (float)(int)imageData.G[num13, num16] * num22 + (float)(int)imageData.G[num15, num16] * num24;
					imageData2.G[num8, num7] = (byte)num25;
					num25 = (float)(int)imageData.R[num13, num14] * num21 + (float)(int)imageData.R[num15, num14] * num23 + (float)(int)imageData.R[num13, num16] * num22 + (float)(int)imageData.R[num15, num16] * num24;
					imageData2.R[num8, num7] = (byte)num25;
				}
			}
		}
		return imageData2.ToBitmap();
	}
}
