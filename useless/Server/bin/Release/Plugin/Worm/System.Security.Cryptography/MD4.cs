using System.Runtime.InteropServices;
using System.Text;

namespace System.Security.Cryptography;

[ComVisible(true)]
public class MD4
{
	private const int BLOCK_LENGTH = 64;

	private readonly uint[] X = new uint[16];

	private readonly uint[] context = new uint[4];

	private byte[] buffer = new byte[64];

	private long count;

	public MD4()
	{
		EngineReset();
	}

	private MD4(MD4 md)
		: this()
	{
		context = (uint[])md.context.Clone();
		buffer = (byte[])md.buffer.Clone();
		count = md.count;
	}

	public object Clone()
	{
		return new MD4(this);
	}

	private void EngineReset()
	{
		context[0] = 1732584193u;
		context[1] = 4023233417u;
		context[2] = 2562383102u;
		context[3] = 271733878u;
		count = 0L;
		for (int i = 0; i < 64; i++)
		{
			buffer[i] = 0;
		}
	}

	private void EngineUpdate(byte b)
	{
		int num = (int)(count % 64);
		count++;
		buffer[num] = b;
		if (num == 63)
		{
			Transform(ref buffer, 0);
		}
	}

	private void EngineUpdate(byte[] input, int offset, int len)
	{
		if (offset < 0 || len < 0 || (long)offset + (long)len > input.Length)
		{
			throw new ArgumentOutOfRangeException();
		}
		int num = (int)(count % 64);
		count += len;
		int num2 = 64 - num;
		int i = 0;
		if (len >= num2)
		{
			Array.Copy(input, offset + i, buffer, num, num2);
			Transform(ref buffer, 0);
			for (i = num2; i + 64 - 1 < len; i += 64)
			{
				Transform(ref input, offset + i);
			}
			num = 0;
		}
		if (i < len)
		{
			Array.Copy(input, offset + i, buffer, num, len - i);
		}
	}

	private byte[] EngineDigest()
	{
		int num = (int)(count % 64);
		int num2 = ((num < 56) ? (56 - num) : (120 - num));
		byte[] array = new byte[num2 + 8];
		array[0] = 128;
		for (int i = 0; i < 8; i++)
		{
			array[num2 + i] = (byte)(count * 8 >> 8 * i);
		}
		EngineUpdate(array, 0, array.Length);
		byte[] array2 = new byte[16];
		for (int j = 0; j < 4; j++)
		{
			for (int k = 0; k < 4; k++)
			{
				array2[j * 4 + k] = (byte)(context[j] >> 8 * k);
			}
		}
		EngineReset();
		return array2;
	}

	public byte[] GetByteHashFromString(string s)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(s);
		MD4 mD = new MD4();
		mD.EngineUpdate(bytes, 0, bytes.Length);
		return mD.EngineDigest();
	}

	public byte[] GetByteHashFromBytes(byte[] b)
	{
		MD4 mD = new MD4();
		mD.EngineUpdate(b, 0, b.Length);
		return mD.EngineDigest();
	}

	public string GetHexHashFromBytes(byte[] b)
	{
		byte[] byteHashFromBytes = GetByteHashFromBytes(b);
		return BytesToHex(byteHashFromBytes, byteHashFromBytes.Length);
	}

	public byte[] GetByteHashFromByte(byte b)
	{
		MD4 mD = new MD4();
		mD.EngineUpdate(b);
		return mD.EngineDigest();
	}

	public string GetHexHashFromByte(byte b)
	{
		byte[] byteHashFromByte = GetByteHashFromByte(b);
		return BytesToHex(byteHashFromByte, byteHashFromByte.Length);
	}

	public string GetHexHashFromString(string s)
	{
		byte[] byteHashFromString = GetByteHashFromString(s);
		return BytesToHex(byteHashFromString, byteHashFromString.Length);
	}

	private static string BytesToHex(byte[] a, int len)
	{
		string text = BitConverter.ToString(a);
		StringBuilder stringBuilder = new StringBuilder((len - 2) / 2);
		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] != '-')
			{
				stringBuilder.Append(text[i]);
			}
		}
		return stringBuilder.ToString();
	}

	private void Transform(ref byte[] block, int offset)
	{
		for (int i = 0; i < 16; i++)
		{
			X[i] = (block[offset++] & 0xFFu) | (uint)((block[offset++] & 0xFF) << 8) | (uint)((block[offset++] & 0xFF) << 16) | (uint)((block[offset++] & 0xFF) << 24);
		}
		uint a = context[0];
		uint num = context[1];
		uint num2 = context[2];
		uint num3 = context[3];
		a = FF(a, num, num2, num3, X[0], 3);
		num3 = FF(num3, a, num, num2, X[1], 7);
		num2 = FF(num2, num3, a, num, X[2], 11);
		num = FF(num, num2, num3, a, X[3], 19);
		a = FF(a, num, num2, num3, X[4], 3);
		num3 = FF(num3, a, num, num2, X[5], 7);
		num2 = FF(num2, num3, a, num, X[6], 11);
		num = FF(num, num2, num3, a, X[7], 19);
		a = FF(a, num, num2, num3, X[8], 3);
		num3 = FF(num3, a, num, num2, X[9], 7);
		num2 = FF(num2, num3, a, num, X[10], 11);
		num = FF(num, num2, num3, a, X[11], 19);
		a = FF(a, num, num2, num3, X[12], 3);
		num3 = FF(num3, a, num, num2, X[13], 7);
		num2 = FF(num2, num3, a, num, X[14], 11);
		num = FF(num, num2, num3, a, X[15], 19);
		a = GG(a, num, num2, num3, X[0], 3);
		num3 = GG(num3, a, num, num2, X[4], 5);
		num2 = GG(num2, num3, a, num, X[8], 9);
		num = GG(num, num2, num3, a, X[12], 13);
		a = GG(a, num, num2, num3, X[1], 3);
		num3 = GG(num3, a, num, num2, X[5], 5);
		num2 = GG(num2, num3, a, num, X[9], 9);
		num = GG(num, num2, num3, a, X[13], 13);
		a = GG(a, num, num2, num3, X[2], 3);
		num3 = GG(num3, a, num, num2, X[6], 5);
		num2 = GG(num2, num3, a, num, X[10], 9);
		num = GG(num, num2, num3, a, X[14], 13);
		a = GG(a, num, num2, num3, X[3], 3);
		num3 = GG(num3, a, num, num2, X[7], 5);
		num2 = GG(num2, num3, a, num, X[11], 9);
		num = GG(num, num2, num3, a, X[15], 13);
		a = HH(a, num, num2, num3, X[0], 3);
		num3 = HH(num3, a, num, num2, X[8], 9);
		num2 = HH(num2, num3, a, num, X[4], 11);
		num = HH(num, num2, num3, a, X[12], 15);
		a = HH(a, num, num2, num3, X[2], 3);
		num3 = HH(num3, a, num, num2, X[10], 9);
		num2 = HH(num2, num3, a, num, X[6], 11);
		num = HH(num, num2, num3, a, X[14], 15);
		a = HH(a, num, num2, num3, X[1], 3);
		num3 = HH(num3, a, num, num2, X[9], 9);
		num2 = HH(num2, num3, a, num, X[5], 11);
		num = HH(num, num2, num3, a, X[13], 15);
		a = HH(a, num, num2, num3, X[3], 3);
		num3 = HH(num3, a, num, num2, X[11], 9);
		num2 = HH(num2, num3, a, num, X[7], 11);
		num = HH(num, num2, num3, a, X[15], 15);
		context[0] += a;
		context[1] += num;
		context[2] += num2;
		context[3] += num3;
	}

	private uint FF(uint a, uint b, uint c, uint d, uint x, int s)
	{
		uint num = a + ((b & c) | (~b & d)) + x;
		return (num << s) | (num >> 32 - s);
	}

	private uint GG(uint a, uint b, uint c, uint d, uint x, int s)
	{
		uint num = a + ((b & (c | d)) | (c & d)) + x + 1518500249;
		return (num << s) | (num >> 32 - s);
	}

	private uint HH(uint a, uint b, uint c, uint d, uint x, int s)
	{
		uint num = a + (b ^ c ^ d) + x + 1859775393;
		return (num << s) | (num >> 32 - s);
	}
}
