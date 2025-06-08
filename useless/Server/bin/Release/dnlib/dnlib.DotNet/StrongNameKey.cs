using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;

namespace dnlib.DotNet;

public sealed class StrongNameKey
{
	private const uint RSA2_SIG = 843141970u;

	private byte[] publicKey;

	private readonly AssemblyHashAlgorithm hashAlg;

	private readonly byte[] publicExponent;

	private readonly byte[] modulus;

	private readonly byte[] prime1;

	private readonly byte[] prime2;

	private readonly byte[] exponent1;

	private readonly byte[] exponent2;

	private readonly byte[] coefficient;

	private readonly byte[] privateExponent;

	public byte[] PublicKey
	{
		get
		{
			if (publicKey == null)
			{
				Interlocked.CompareExchange(ref publicKey, CreatePublicKey(), null);
			}
			return publicKey;
		}
	}

	public int SignatureSize => modulus.Length;

	public AssemblyHashAlgorithm HashAlgorithm => hashAlg;

	public byte[] PublicExponent => publicExponent;

	public byte[] Modulus => modulus;

	public byte[] Prime1 => prime1;

	public byte[] Prime2 => prime2;

	public byte[] Exponent1 => exponent1;

	public byte[] Exponent2 => exponent2;

	public byte[] Coefficient => coefficient;

	public byte[] PrivateExponent => privateExponent;

	public StrongNameKey(byte[] keyData)
		: this(new BinaryReader(new MemoryStream(keyData)))
	{
	}

	public StrongNameKey(string filename)
		: this(File.ReadAllBytes(filename))
	{
	}

	public StrongNameKey(Stream stream)
		: this(new BinaryReader(stream))
	{
	}

	public StrongNameKey(BinaryReader reader)
	{
		try
		{
			publicKey = null;
			if (reader.ReadByte() != 7)
			{
				throw new InvalidKeyException("Not a public/private key pair");
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
			if (reader.ReadUInt32() != 843141970)
			{
				throw new InvalidKeyException("Invalid RSA2 magic");
			}
			uint num = reader.ReadUInt32();
			publicExponent = reader.ReadBytesReverse(4);
			int len = (int)(num / 8);
			int len2 = (int)(num / 16);
			modulus = reader.ReadBytesReverse(len);
			prime1 = reader.ReadBytesReverse(len2);
			prime2 = reader.ReadBytesReverse(len2);
			exponent1 = reader.ReadBytesReverse(len2);
			exponent2 = reader.ReadBytesReverse(len2);
			coefficient = reader.ReadBytesReverse(len2);
			privateExponent = reader.ReadBytesReverse(len);
		}
		catch (IOException innerException)
		{
			throw new InvalidKeyException("Couldn't read strong name key", innerException);
		}
	}

	private StrongNameKey(AssemblyHashAlgorithm hashAlg, byte[] publicExponent, byte[] modulus, byte[] prime1, byte[] prime2, byte[] exponent1, byte[] exponent2, byte[] coefficient, byte[] privateExponent)
	{
		this.hashAlg = hashAlg;
		this.publicExponent = publicExponent;
		this.modulus = modulus;
		this.prime1 = prime1;
		this.prime2 = prime2;
		this.exponent1 = exponent1;
		this.exponent2 = exponent2;
		this.coefficient = coefficient;
		this.privateExponent = privateExponent;
	}

	public StrongNameKey WithHashAlgorithm(AssemblyHashAlgorithm hashAlgorithm)
	{
		if (hashAlg == hashAlgorithm)
		{
			return this;
		}
		return new StrongNameKey(hashAlgorithm, publicExponent, modulus, prime1, prime2, exponent1, exponent2, coefficient, privateExponent);
	}

	private byte[] CreatePublicKey()
	{
		AssemblyHashAlgorithm assemblyHashAlgorithm = ((hashAlg == AssemblyHashAlgorithm.None) ? AssemblyHashAlgorithm.SHA1 : hashAlg);
		return StrongNamePublicKey.CreatePublicKey(SignatureAlgorithm.CALG_RSA_SIGN, assemblyHashAlgorithm, modulus, publicExponent);
	}

	public RSA CreateRSA()
	{
		RSAParameters rSAParameters = default(RSAParameters);
		rSAParameters.Exponent = publicExponent;
		rSAParameters.Modulus = modulus;
		rSAParameters.P = prime1;
		rSAParameters.Q = prime2;
		rSAParameters.DP = exponent1;
		rSAParameters.DQ = exponent2;
		rSAParameters.InverseQ = coefficient;
		rSAParameters.D = privateExponent;
		RSAParameters parameters = rSAParameters;
		RSA rSA = RSA.Create();
		try
		{
			rSA.ImportParameters(parameters);
			return rSA;
		}
		catch
		{
			((IDisposable)rSA).Dispose();
			throw;
		}
	}

	public byte[] CreateStrongName()
	{
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write((byte)7);
		binaryWriter.Write((byte)2);
		binaryWriter.Write((ushort)0);
		binaryWriter.Write(9216u);
		binaryWriter.Write(843141970u);
		binaryWriter.Write(modulus.Length * 8);
		binaryWriter.WriteReverse(publicExponent);
		binaryWriter.WriteReverse(modulus);
		binaryWriter.WriteReverse(prime1);
		binaryWriter.WriteReverse(prime2);
		binaryWriter.WriteReverse(exponent1);
		binaryWriter.WriteReverse(exponent2);
		binaryWriter.WriteReverse(coefficient);
		binaryWriter.WriteReverse(privateExponent);
		return memoryStream.ToArray();
	}

	public static string CreateCounterSignatureAsString(StrongNamePublicKey identityPubKey, StrongNameKey identityKey, StrongNamePublicKey signaturePubKey)
	{
		return Utils.ToHex(CreateCounterSignature(identityPubKey, identityKey, signaturePubKey), upper: false);
	}

	public static byte[] CreateCounterSignature(StrongNamePublicKey identityPubKey, StrongNameKey identityKey, StrongNamePublicKey signaturePubKey)
	{
		byte[] rgbHash = AssemblyHash.Hash(signaturePubKey.CreatePublicKey(), identityPubKey.HashAlgorithm);
		using RSA key = identityKey.CreateRSA();
		RSAPKCS1SignatureFormatter rSAPKCS1SignatureFormatter = new RSAPKCS1SignatureFormatter(key);
		string name = identityPubKey.HashAlgorithm.GetName();
		rSAPKCS1SignatureFormatter.SetHashAlgorithm(name);
		byte[] array = rSAPKCS1SignatureFormatter.CreateSignature(rgbHash);
		Array.Reverse((Array)array);
		return array;
	}
}
