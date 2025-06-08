using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Worm2.Files;

internal class ByteToBitMap
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

	public static byte[] BitmapToByteArray(Bitmap bitmap)
	{
		using MemoryStream memoryStream = new MemoryStream();
		((Image)bitmap).Save((Stream)memoryStream, ImageFormat.Bmp);
		return memoryStream.ToArray();
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
				if (num + 1 <= buffer.Length)
				{
					val.SetPixel(i, j, Color.FromArgb(30, buffer[num], 0, 0));
					num++;
					continue;
				}
				val.SetPixel(i, j, Color.FromArgb(100, 0, 0, 0));
				return val;
			}
		}
		return val;
	}
}
