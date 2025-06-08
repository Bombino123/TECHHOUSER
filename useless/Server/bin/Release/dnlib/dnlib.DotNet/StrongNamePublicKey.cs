using System;
using System.IO;

namespace dnlib.DotNet;

public sealed class StrongNamePublicKey
{
	private const uint RSA1_SIG = 826364754u;

	private readonly SignatureAlgorithm signatureAlgorithm;

	private readonly AssemblyHashAlgorithm hashAlgorithm;

	private readonly byte[] modulus;

	private readonly byte[] publicExponent;

	public SignatureAlgorithm SignatureAlgorithm => signatureAlgorithm;

	public AssemblyHashAlgorithm HashAlgorithm => hashAlgorithm;

	public byte[] Modulus => modulus;

	public byte[] PublicExponent => publicExponent;

	public StrongNamePublicKey()
	{
	}

	public StrongNamePublicKey(byte[] modulus, byte[] publicExponent)
		: this(modulus, publicExponent, AssemblyHashAlgorithm.SHA1, SignatureAlgorithm.CALG_RSA_SIGN)
	{
	}

	public StrongNamePublicKey(byte[] modulus, byte[] publicExponent, AssemblyHashAlgorithm hashAlgorithm)
		: this(modulus, publicExponent, hashAlgorithm, SignatureAlgorithm.CALG_RSA_SIGN)
	{
	}

	public StrongNamePublicKey(byte[] modulus, byte[] publicExponent, AssemblyHashAlgorithm hashAlgorithm, SignatureAlgorithm signatureAlgorithm)
	{
		this.signatureAlgorithm = signatureAlgorithm;
		this.hashAlgorithm = hashAlgorithm;
		this.modulus = modulus;
		this.publicExponent = publicExponent;
	}

	public StrongNamePublicKey(PublicKey pk)
		: this(pk.Data)
	{
	}

	public StrongNamePublicKey(byte[] pk)
		: this(new BinaryReader(new MemoryStream(pk)))
	{
	}

	public StrongNamePublicKey(string filename)
		: this(File.ReadAllBytes(filename))
	{
	}

	public StrongNamePublicKey(Stream stream)
		: this(new BinaryReader(stream))
	{
	}

	public StrongNamePublicKey(BinaryReader reader)
	{
		try
		{
			signatureAlgorithm = (SignatureAlgorithm)reader.ReadUInt32();
			hashAlgorithm = (AssemblyHashAlgorithm)reader.ReadUInt32();
			reader.ReadInt32();
			if (reader.ReadByte() != 6)
			{
				throw new InvalidKeyException("Not a public key");
			}
			if (reader.ReadByte() != 2)
			{
				throw new InvalidKeyException("Invalid version");
			}
			reader.ReadUInt16();
			if (reader.ReadUInt32() != 9216)
			{
				throw new InvalidKeyException("Not RSA sign");
			}
			if (reader.ReadUInt32() != 826364754)
			{
				throw new InvalidKeyException("Invalid RSA1 magic");
			}
			uint num = reader.ReadUInt32();
			publicExponent = reader.ReadBytesReverse(4);
			modulus = reader.ReadBytesReverse((int)(num / 8));
		}
		catch (IOException innerException)
		{
			throw new InvalidKeyException("Invalid public key", innerException);
		}
	}

	public byte[] CreatePublicKey()
	{
		return CreatePublicKey(signatureAlgorithm, hashAlgorithm, modulus, publicExponent);
	}

	internal static byte[] CreatePublicKey(SignatureAlgorithm sigAlg, AssemblyHashAlgorithm hashAlg, byte[] modulus, byte[] publicExponent)
	{
		if (sigAlg != SignatureAlgorithm.CALG_RSA_SIGN)
		{
			throw new ArgumentException("Signature algorithm must be RSA");
		}
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write((uint)sigAlg);
		binaryWriter.Write((uint)hashAlg);
		binaryWriter.Write(20 + modulus.Length);
		binaryWriter.Write((byte)6);
		binaryWriter.Write((byte)2);
		binaryWriter.Write((ushort)0);
		binaryWriter.Write((uint)sigAlg);
		binaryWriter.Write(826364754u);
		binaryWriter.Write(modulus.Length * 8);
		binaryWriter.WriteReverse(publicExponent);
		binaryWriter.WriteReverse(modulus);
		return memoryStream.ToArray();
	}

	public override string ToString()
	{
		return Utils.ToHex(CreatePublicKey(), upper: false);
	}
}
