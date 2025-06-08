using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Utilities;

[ComVisible(true)]
public static class AesCcm
{
	private static byte[] CalculateMac(byte[] key, byte[] nonce, byte[] data, byte[] associatedData, int signatureLength)
	{
		byte[] a = BuildB0Block(nonce, hasAssociatedData: true, signatureLength, data.Length);
		if (associatedData.Length != 0)
		{
			if (associatedData.Length >= 65280)
			{
				throw new NotSupportedException("Associated data length of 65280 or more is not supported");
			}
			byte[] bytes = BigEndianConverter.GetBytes((ushort)associatedData.Length);
			a = ByteUtils.Concatenate(a, bytes);
			a = ByteUtils.Concatenate(a, associatedData);
			int num = (16 - a.Length % 16) % 16;
			a = ByteUtils.Concatenate(a, new byte[num]);
		}
		a = ByteUtils.Concatenate(a, data);
		int num2 = (16 - a.Length % 16) % 16;
		a = ByteUtils.Concatenate(a, new byte[num2]);
		return ByteReader.ReadBytes(AesEncrypt(key, new byte[16], a, CipherMode.CBC), a.Length - 16, signatureLength);
	}

	public static byte[] Encrypt(byte[] key, byte[] nonce, byte[] data, byte[] associatedData, int signatureLength, out byte[] signature)
	{
		if (nonce.Length < 7 || nonce.Length > 13)
		{
			throw new ArgumentException("nonce length must be between 7 and 13 bytes");
		}
		if (signatureLength < 4 || signatureLength > 16 || signatureLength % 2 == 1)
		{
			throw new ArgumentException("signature length must be an even number between 4 and 16 bytes");
		}
		byte[] array = BuildKeyStream(key, nonce, data.Length);
		byte[] array2 = CalculateMac(key, nonce, data, associatedData, signatureLength);
		signature = ByteUtils.XOR(array, 0, array2, 0, array2.Length);
		return ByteUtils.XOR(data, 0, array, 16, data.Length);
	}

	public static byte[] DecryptAndAuthenticate(byte[] key, byte[] nonce, byte[] encryptedData, byte[] associatedData, byte[] signature)
	{
		if (nonce.Length < 7 || nonce.Length > 13)
		{
			throw new ArgumentException("nonce length must be between 7 and 13 bytes");
		}
		if (signature.Length < 4 || signature.Length > 16 || signature.Length % 2 == 1)
		{
			throw new ArgumentException("signature length must be an even number between 4 and 16 bytes");
		}
		byte[] array = BuildKeyStream(key, nonce, encryptedData.Length);
		byte[] array2 = ByteUtils.XOR(encryptedData, 0, array, 16, encryptedData.Length);
		byte[] array3 = CalculateMac(key, nonce, array2, associatedData, signature.Length);
		if (!ByteUtils.AreByteArraysEqual(ByteUtils.XOR(array, 0, array3, 0, array3.Length), signature))
		{
			throw new CryptographicException("The computed authentication value did not match the input");
		}
		return array2;
	}

	private static byte[] BuildKeyStream(byte[] key, byte[] nonce, int dataLength)
	{
		int num = 16 - dataLength % 16 % 16;
		int num2 = 16 + dataLength + num;
		int num3 = num2 / 16;
		byte[] array = new byte[num2];
		for (int i = 0; i < num3; i++)
		{
			byte[] bytes = BuildABlock(nonce, i);
			ByteWriter.WriteBytes(array, i * 16, bytes);
		}
		return AesEncrypt(key, new byte[16], array, CipherMode.ECB);
	}

	private static byte[] BuildB0Block(byte[] nonce, bool hasAssociatedData, int signatureLength, int messageLength)
	{
		byte[] array = new byte[16];
		Array.Copy(nonce, 0, array, 1, nonce.Length);
		int num = 15 - nonce.Length;
		array[0] = ComputeFlagsByte(hasAssociatedData, signatureLength, num);
		int num2 = messageLength;
		for (int num3 = 15; num3 > 15 - num; num3--)
		{
			array[num3] = (byte)(num2 % 256);
			num2 /= 256;
		}
		return array;
	}

	private static byte[] BuildABlock(byte[] nonce, int blockIndex)
	{
		byte[] array = new byte[16];
		Array.Copy(nonce, 0, array, 1, nonce.Length);
		int num = 15 - nonce.Length;
		array[0] = (byte)(num - 1);
		int num2 = blockIndex;
		for (int num3 = 15; num3 > 15 - num; num3--)
		{
			array[num3] = (byte)(num2 % 256);
			num2 /= 256;
		}
		return array;
	}

	private static byte ComputeFlagsByte(bool hasAssociatedData, int signatureLength, int lengthFieldLength)
	{
		byte b = 0;
		if (hasAssociatedData)
		{
			b = (byte)(b | 0x40u);
		}
		b |= (byte)(lengthFieldLength - 1);
		return (byte)(b | (byte)((signatureLength - 2) / 2 << 3));
	}

	private static byte[] AesEncrypt(byte[] key, byte[] iv, byte[] data, CipherMode cipherMode)
	{
		using MemoryStream memoryStream = new MemoryStream();
		RijndaelManaged rijndaelManaged = new RijndaelManaged();
		rijndaelManaged.Mode = cipherMode;
		rijndaelManaged.Padding = PaddingMode.None;
		using CryptoStream cryptoStream = new CryptoStream(memoryStream, rijndaelManaged.CreateEncryptor(key, iv), CryptoStreamMode.Write);
		cryptoStream.Write(data, 0, data.Length);
		cryptoStream.FlushFinalBlock();
		return memoryStream.ToArray();
	}
}
