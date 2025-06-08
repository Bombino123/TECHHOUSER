using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace dnlib.DotNet;

[ComVisible(true)]
public readonly struct StrongNameSigner
{
	private readonly Stream stream;

	private readonly long baseOffset;

	public StrongNameSigner(Stream stream)
		: this(stream, 0L)
	{
	}

	public StrongNameSigner(Stream stream, long baseOffset)
	{
		this.stream = stream;
		this.baseOffset = baseOffset;
	}

	public byte[] WriteSignature(StrongNameKey snk, long snSigOffset)
	{
		byte[] array = CalculateSignature(snk, snSigOffset);
		stream.Position = baseOffset + snSigOffset;
		stream.Write(array, 0, array.Length);
		return array;
	}

	public byte[] CalculateSignature(StrongNameKey snk, long snSigOffset)
	{
		uint signatureSize = (uint)snk.SignatureSize;
		AssemblyHashAlgorithm hashAlg = ((snk.HashAlgorithm == AssemblyHashAlgorithm.None) ? AssemblyHashAlgorithm.SHA1 : snk.HashAlgorithm);
		byte[] hash = StrongNameHashData(hashAlg, snSigOffset, signatureSize);
		byte[] strongNameSignature = GetStrongNameSignature(snk, hashAlg, hash);
		if (strongNameSignature.Length != signatureSize)
		{
			throw new InvalidOperationException("Invalid strong name signature size");
		}
		return strongNameSignature;
	}

	private byte[] StrongNameHashData(AssemblyHashAlgorithm hashAlg, long snSigOffset, uint snSigSize)
	{
		BinaryReader binaryReader = new BinaryReader(stream);
		snSigOffset += baseOffset;
		long num = snSigOffset + snSigSize;
		using AssemblyHash assemblyHash = new AssemblyHash(hashAlg);
		byte[] array = new byte[32768];
		stream.Position = baseOffset + 60;
		uint length = binaryReader.ReadUInt32();
		stream.Position = baseOffset;
		assemblyHash.Hash(stream, length, array);
		stream.Position += 6L;
		int num2 = binaryReader.ReadUInt16();
		stream.Position -= 8L;
		assemblyHash.Hash(stream, 24u, array);
		bool num3 = binaryReader.ReadUInt16() == 267;
		stream.Position -= 2L;
		int num4 = (num3 ? 96 : 112);
		if (stream.Read(array, 0, num4) != num4)
		{
			throw new IOException("Could not read data");
		}
		for (int i = 0; i < 4; i++)
		{
			array[64 + i] = 0;
		}
		assemblyHash.Hash(array, 0, num4);
		if (stream.Read(array, 0, 128) != 128)
		{
			throw new IOException("Could not read data");
		}
		for (int j = 0; j < 8; j++)
		{
			array[32 + j] = 0;
		}
		assemblyHash.Hash(array, 0, 128);
		long position = stream.Position;
		assemblyHash.Hash(stream, (uint)(num2 * 40), array);
		for (int k = 0; k < num2; k++)
		{
			stream.Position = position + k * 40 + 16;
			uint num5 = binaryReader.ReadUInt32();
			uint num6 = binaryReader.ReadUInt32();
			stream.Position = baseOffset + num6;
			while (num5 != 0)
			{
				long position2 = stream.Position;
				if (snSigOffset <= position2 && position2 < num)
				{
					uint num7 = (uint)(num - position2);
					if (num7 >= num5)
					{
						break;
					}
					num5 -= num7;
					stream.Position += num7;
					continue;
				}
				if (position2 >= num)
				{
					assemblyHash.Hash(stream, num5, array);
					break;
				}
				uint num8 = (uint)Math.Min(snSigOffset - position2, num5);
				assemblyHash.Hash(stream, num8, array);
				num5 -= num8;
			}
		}
		return assemblyHash.ComputeHash();
	}

	private byte[] GetStrongNameSignature(StrongNameKey snk, AssemblyHashAlgorithm hashAlg, byte[] hash)
	{
		using RSA key = snk.CreateRSA();
		RSAPKCS1SignatureFormatter rSAPKCS1SignatureFormatter = new RSAPKCS1SignatureFormatter(key);
		string hashAlgorithm = hashAlg.GetName() ?? AssemblyHashAlgorithm.SHA1.GetName();
		rSAPKCS1SignatureFormatter.SetHashAlgorithm(hashAlgorithm);
		byte[] array = rSAPKCS1SignatureFormatter.CreateSignature(hash);
		Array.Reverse((Array)array);
		return array;
	}
}
