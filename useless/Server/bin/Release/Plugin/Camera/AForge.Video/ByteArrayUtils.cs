using System;

namespace AForge.Video;

internal static class ByteArrayUtils
{
	public static bool Compare(byte[] array, byte[] needle, int startIndex)
	{
		int num = needle.Length;
		int num2 = 0;
		int num3 = startIndex;
		while (num2 < num)
		{
			if (array[num3] != needle[num2])
			{
				return false;
			}
			num2++;
			num3++;
		}
		return true;
	}

	public static int Find(byte[] array, byte[] needle, int startIndex, int sourceLength)
	{
		int num = needle.Length;
		while (sourceLength >= num)
		{
			int num2 = Array.IndexOf(array, needle[0], startIndex, sourceLength - num + 1);
			if (num2 == -1)
			{
				return -1;
			}
			int num3 = 0;
			int num4 = num2;
			while (num3 < num && array[num4] == needle[num3])
			{
				num3++;
				num4++;
			}
			if (num3 == num)
			{
				return num2;
			}
			sourceLength -= num2 - startIndex + 1;
			startIndex = num2 + 1;
		}
		return -1;
	}
}
