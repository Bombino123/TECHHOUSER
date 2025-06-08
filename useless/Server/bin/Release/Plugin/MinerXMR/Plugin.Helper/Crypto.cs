using System.Collections.Generic;
using System.Drawing;

namespace Plugin.Helper;

internal class Crypto
{
	public static int[] WHGet(int Length)
	{
		int[] array = new int[2] { 3, 3 };
		while (array[0] * array[1] <= Length)
		{
			array[0]++;
			array[1]++;
		}
		return array;
	}

	public static Bitmap ByteToBitmap(byte[] buffer)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		int[] array = WHGet(buffer.Length);
		int num = 0;
		Bitmap val = new Bitmap(array[0], array[1]);
		for (int i = 0; i < array[0]; i++)
		{
			for (int j = 0; j < array[1]; j++)
			{
				if (num + 3 <= buffer.Length)
				{
					val.SetPixel(i, j, Color.FromArgb(255, buffer[num], buffer[num + 1], buffer[num + 2]));
					num += 3;
					continue;
				}
				if (num + 2 <= buffer.Length)
				{
					val.SetPixel(i, j, Color.FromArgb(20, buffer[num], buffer[num + 1], 0));
					num += 2;
					continue;
				}
				if (num + 1 > buffer.Length)
				{
					val.SetPixel(i, j, Color.FromArgb(100, 0, 0, 0));
					return val;
				}
				val.SetPixel(i, j, Color.FromArgb(30, buffer[num], 0, 0));
				num++;
			}
		}
		return val;
	}

	public static byte[] BitmapToByte(Bitmap bitmap)
	{
		List<byte> list = new List<byte>();
		for (int i = 0; i < ((Image)bitmap).Width; i++)
		{
			for (int j = 0; j < ((Image)bitmap).Height; j++)
			{
				Color pixel = bitmap.GetPixel(i, j);
				if (pixel.A == 100)
				{
					return list.ToArray();
				}
				if (pixel.A == 30)
				{
					list.Add(pixel.R);
					return list.ToArray();
				}
				if (pixel.A == 20)
				{
					list.Add(pixel.R);
					list.Add(pixel.G);
					return list.ToArray();
				}
				if (pixel.A == byte.MaxValue)
				{
					list.Add(pixel.R);
					list.Add(pixel.G);
					list.Add(pixel.B);
				}
			}
		}
		return list.ToArray();
	}
}
