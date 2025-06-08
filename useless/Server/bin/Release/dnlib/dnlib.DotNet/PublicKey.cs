using System.Threading;

namespace dnlib.DotNet;

public sealed class PublicKey : PublicKeyBase
{
	private const AssemblyHashAlgorithm DEFAULT_ALGORITHM = AssemblyHashAlgorithm.SHA1;

	private PublicKeyToken publicKeyToken;

	public override PublicKeyToken Token
	{
		get
		{
			if (publicKeyToken == null && !base.IsNullOrEmpty)
			{
				Interlocked.CompareExchange(ref publicKeyToken, AssemblyHash.CreatePublicKeyToken(data), null);
			}
			return publicKeyToken;
		}
	}

	public override byte[] Data => data;

	public PublicKey()
		: base((byte[])null)
	{
	}

	public PublicKey(byte[] data)
		: base(data)
	{
	}

	public PublicKey(string hexString)
		: base(hexString)
	{
	}

	public override bool Equals(object obj)
	{
		if (this == obj)
		{
			return true;
		}
		if (!(obj is PublicKey publicKey))
		{
			return false;
		}
		return Utils.Equals(Data, publicKey.Data);
	}

	public override int GetHashCode()
	{
		return Utils.GetHashCode(Data);
	}
}
