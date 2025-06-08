using System;
using System.Drawing;

namespace Server.Helper
{
	// Token: 0x0200006C RID: 108
	internal class BitmapCoding
	{
		// Token: 0x06000216 RID: 534 RVA: 0x00021F08 File Offset: 0x00020108
		public static int[] WHGet(int Length)
		{
			int[] array = new int[]
			{
				3,
				3
			};
			while (array[0] * array[1] <= Length)
			{
				array[0]++;
				array[1]++;
			}
			return array;
		}

		// Token: 0x06000217 RID: 535 RVA: 0x00021F4C File Offset: 0x0002014C
		public static Bitmap ByteToBitmap(byte[] buffer)
		{
			int[] array = BitmapCoding.WHGet(buffer.Length);
			int num = 0;
			Bitmap bitmap = new Bitmap(array[0], array[1]);
			for (int i = 0; i < array[0]; i++)
			{
				for (int j = 0; j < array[1]; j++)
				{
					if (num + 3 <= buffer.Length)
					{
						bitmap.SetPixel(i, j, Color.FromArgb(255, (int)buffer[num], (int)buffer[num + 1], (int)buffer[num + 2]));
						num += 3;
					}
					else if (num + 2 <= buffer.Length)
					{
						bitmap.SetPixel(i, j, Color.FromArgb(20, (int)buffer[num], (int)buffer[num + 1], 0));
						num += 2;
					}
					else
					{
						if (num + 1 > buffer.Length)
						{
							bitmap.SetPixel(i, j, Color.FromArgb(100, 0, 0, 0));
							return bitmap;
						}
						bitmap.SetPixel(i, j, Color.FromArgb(30, (int)buffer[num], 0, 0));
						num++;
					}
				}
			}
			return bitmap;
		}
	}
}
