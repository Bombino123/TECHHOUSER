using System;
using System.IO;
using System.Security.Cryptography;

namespace dnlib.DotNet.Writer;

internal static class Hasher
{
	private static HashAlgorithm CreateHasher(ChecksumAlgorithm checksumAlgorithm)
	{
		return checksumAlgorithm switch
		{
			ChecksumAlgorithm.SHA1 => SHA1.Create(), 
			ChecksumAlgorithm.SHA256 => SHA256.Create(), 
			ChecksumAlgorithm.SHA384 => SHA384.Create(), 
			ChecksumAlgorithm.SHA512 => SHA512.Create(), 
			_ => throw new ArgumentOutOfRangeException("checksumAlgorithm"), 
		};
	}

	public static string GetChecksumName(ChecksumAlgorithm checksumAlgorithm)
	{
		return checksumAlgorithm switch
		{
			ChecksumAlgorithm.SHA1 => "SHA1", 
			ChecksumAlgorithm.SHA256 => "SHA256", 
			ChecksumAlgorithm.SHA384 => "SHA384", 
			ChecksumAlgorithm.SHA512 => "SHA512", 
			_ => throw new ArgumentOutOfRangeException("checksumAlgorithm"), 
		};
	}

	public static bool TryGetChecksumAlgorithm(string checksumName, out ChecksumAlgorithm pdbChecksumAlgorithm, out int checksumSize)
	{
		switch (checksumName)
		{
		case "SHA1":
			pdbChecksumAlgorithm = ChecksumAlgorithm.SHA1;
			checksumSize = 20;
			return true;
		case "SHA256":
			pdbChecksumAlgorithm = ChecksumAlgorithm.SHA256;
			checksumSize = 32;
			return true;
		case "SHA384":
			pdbChecksumAlgorithm = ChecksumAlgorithm.SHA384;
			checksumSize = 48;
			return true;
		case "SHA512":
			pdbChecksumAlgorithm = ChecksumAlgorithm.SHA512;
			checksumSize = 64;
			return true;
		default:
			pdbChecksumAlgorithm = ChecksumAlgorithm.SHA1;
			checksumSize = -1;
			return false;
		}
	}

	public static byte[] Hash(ChecksumAlgorithm checksumAlgorithm, Stream stream, long length)
	{
		byte[] array = new byte[(int)Math.Min(8192L, length)];
		using HashAlgorithm hashAlgorithm = CreateHasher(checksumAlgorithm);
		while (length > 0)
		{
			int count = (int)Math.Min(length, array.Length);
			int num = stream.Read(array, 0, count);
			if (num == 0)
			{
				throw new InvalidOperationException("Couldn't read all bytes");
			}
			hashAlgorithm.TransformBlock(array, 0, num, array, 0);
			length -= num;
		}
		hashAlgorithm.TransformFinalBlock(Array2.Empty<byte>(), 0, 0);
		return hashAlgorithm.Hash;
	}
}
