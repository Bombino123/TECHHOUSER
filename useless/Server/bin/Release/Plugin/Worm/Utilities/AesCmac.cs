using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Utilities;

[ComVisible(true)]
public static class AesCmac
{
	public static byte[] CalculateAesCmac(byte[] key, byte[] buffer, int offset, int length)
	{
		byte[] data = ByteReader.ReadBytes(buffer, offset, length);
		return CalculateAesCmac(key, data);
	}

	public static byte[] CalculateAesCmac(byte[] key, byte[] data)
	{
		byte[] array = AESEncrypt(key, new byte[16], new byte[16]);
		byte[] array2 = Rol(array);
		if ((array[0] & 0x80) == 128)
		{
			array2[15] ^= 135;
		}
		byte[] array3 = Rol(array2);
		if ((array2[0] & 0x80) == 128)
		{
			array3[15] ^= 135;
		}
		if (data.Length != 0 && data.Length % 16 == 0)
		{
			for (int i = 0; i < array2.Length; i++)
			{
				data[data.Length - 16 + i] ^= array2[i];
			}
		}
		else
		{
			byte[] array4 = new byte[16 - data.Length % 16];
			array4[0] = 128;
			data = ByteUtils.Concatenate(data, array4);
			for (int j = 0; j < array3.Length; j++)
			{
				data[data.Length - 16 + j] ^= array3[j];
			}
		}
		byte[] array5 = AESEncrypt(key, new byte[16], data);
		byte[] array6 = new byte[16];
		Array.Copy(array5, array5.Length - array6.Length, array6, 0, array6.Length);
		return array6;
	}

	private static byte[] AESEncrypt(byte[] key, byte[] iv, byte[] data)
	{
		using MemoryStream memoryStream = new MemoryStream();
		RijndaelManaged rijndaelManaged = new RijndaelManaged();
		rijndaelManaged.Mode = CipherMode.CBC;
		rijndaelManaged.Padding = PaddingMode.None;
		using CryptoStream cryptoStream = new CryptoStream(memoryStream, rijndaelManaged.CreateEncryptor(key, iv), CryptoStreamMode.Write);
		cryptoStream.Write(data, 0, data.Length);
		cryptoStream.FlushFinalBlock();
		return memoryStream.ToArray();
	}

	private static byte[] Rol(byte[] b)
	{
		byte[] array = new byte[b.Length];
		byte b2 = 0;
		for (int num = b.Length - 1; num >= 0; num--)
		{
			ushort num2 = (ushort)(b[num] << 1);
			array[num] = (byte)((num2 & 0xFF) + b2);
			b2 = (byte)((num2 & 0xFF00) >> 8);
		}
		return array;
	}
}
