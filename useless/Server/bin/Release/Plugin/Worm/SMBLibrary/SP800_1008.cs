using System;
using System.Security.Cryptography;
using Utilities;

namespace SMBLibrary;

internal class SP800_1008
{
	public static byte[] DeriveKey(HMAC hmac, byte[] label, byte[] context, int keyLengthInBits)
	{
		int num = ((label != null) ? label.Length : 0);
		int num2 = ((context != null) ? context.Length : 0);
		byte[] array = new byte[4 + num + 1 + num2 + 4];
		if (num != 0)
		{
			Buffer.BlockCopy(label, 0, array, 4, num);
		}
		if (num2 != 0)
		{
			Buffer.BlockCopy(context, 0, array, 5 + num, num2);
		}
		BigEndianWriter.WriteUInt32(array, 5 + num + num2, (uint)keyLengthInBits);
		int num3 = 0;
		int num4 = keyLengthInBits / 8;
		byte[] array2 = new byte[num4];
		uint num5 = 1u;
		while (num4 > 0)
		{
			BigEndianWriter.WriteUInt32(array, 0, num5);
			byte[] array3 = hmac.ComputeHash(array);
			int num6 = Math.Min(num4, array3.Length);
			Buffer.BlockCopy(array3, 0, array2, num3, num6);
			num3 += num6;
			num4 -= num6;
			num5++;
		}
		return array2;
	}
}
