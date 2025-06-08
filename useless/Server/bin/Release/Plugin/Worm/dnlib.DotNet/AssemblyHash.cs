using System;
using System.IO;
using System.Security.Cryptography;

namespace dnlib.DotNet;

internal readonly struct AssemblyHash : IDisposable
{
	private readonly HashAlgorithm hasher;

	public AssemblyHash(AssemblyHashAlgorithm hashAlgo)
	{
		hasher = hashAlgo switch
		{
			AssemblyHashAlgorithm.MD5 => MD5.Create(), 
			AssemblyHashAlgorithm.SHA_256 => SHA256.Create(), 
			AssemblyHashAlgorithm.SHA_384 => SHA384.Create(), 
			AssemblyHashAlgorithm.SHA_512 => SHA512.Create(), 
			_ => SHA1.Create(), 
		};
	}

	public void Dispose()
	{
		if (hasher != null)
		{
			((IDisposable)hasher).Dispose();
		}
	}

	public static byte[] Hash(byte[] data, AssemblyHashAlgorithm hashAlgo)
	{
		if (data == null)
		{
			return null;
		}
		using AssemblyHash assemblyHash = new AssemblyHash(hashAlgo);
		assemblyHash.Hash(data);
		return assemblyHash.ComputeHash();
	}

	public void Hash(byte[] data)
	{
		Hash(data, 0, data.Length);
	}

	public void Hash(byte[] data, int offset, int length)
	{
		if (hasher.TransformBlock(data, offset, length, data, offset) != length)
		{
			throw new IOException("Could not calculate hash");
		}
	}

	public void Hash(Stream stream, uint length, byte[] buffer)
	{
		while (length != 0)
		{
			int num = ((length > (uint)buffer.Length) ? buffer.Length : ((int)length));
			if (stream.Read(buffer, 0, num) != num)
			{
				throw new IOException("Could not read data");
			}
			Hash(buffer, 0, num);
			length -= (uint)num;
		}
	}

	public byte[] ComputeHash()
	{
		hasher.TransformFinalBlock(Array2.Empty<byte>(), 0, 0);
		return hasher.Hash;
	}

	public static PublicKeyToken CreatePublicKeyToken(byte[] publicKeyData)
	{
		if (publicKeyData == null)
		{
			return new PublicKeyToken();
		}
		byte[] array = Hash(publicKeyData, AssemblyHashAlgorithm.SHA1);
		byte[] array2 = new byte[8];
		for (int i = 0; i < array2.Length && i < array.Length; i++)
		{
			array2[i] = array[array.Length - i - 1];
		}
		return new PublicKeyToken(array2);
	}
}
