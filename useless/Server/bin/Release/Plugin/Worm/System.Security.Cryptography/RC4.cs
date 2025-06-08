using System.Runtime.InteropServices;

namespace System.Security.Cryptography;

[ComVisible(true)]
public class RC4
{
	public static byte[] Encrypt(byte[] key, byte[] data)
	{
		return EncryptOutput(key, data);
	}

	public static byte[] Decrypt(byte[] key, byte[] data)
	{
		return EncryptOutput(key, data);
	}

	private static byte[] EncryptInitalize(byte[] key)
	{
		byte[] array = new byte[256];
		for (int i = 0; i < 256; i++)
		{
			array[i] = (byte)i;
		}
		int j = 0;
		int num = 0;
		for (; j < 256; j++)
		{
			num = (num + key[j % key.Length] + array[j]) & 0xFF;
			Swap(array, j, num);
		}
		return array;
	}

	private static byte[] EncryptOutput(byte[] key, byte[] data)
	{
		byte[] array = EncryptInitalize(key);
		int num = 0;
		int num2 = 0;
		byte[] array2 = new byte[data.Length];
		for (int i = 0; i < data.Length; i++)
		{
			num = (num + 1) & 0xFF;
			num2 = (num2 + array[num]) & 0xFF;
			Swap(array, num, num2);
			array2[i] = (byte)(data[i] ^ array[(array[num] + array[num2]) & 0xFF]);
		}
		return array2;
	}

	private static void Swap(byte[] s, int i, int j)
	{
		byte b = s[i];
		s[i] = s[j];
		s[j] = b;
	}
}
