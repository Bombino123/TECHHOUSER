using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public sealed class PublicKeyToken : PublicKeyBase
{
	public override PublicKeyToken Token => this;

	public PublicKeyToken()
		: base((byte[])null)
	{
	}

	public PublicKeyToken(byte[] data)
		: base(data)
	{
	}

	public PublicKeyToken(string hexString)
		: base(hexString)
	{
	}

	public override bool Equals(object obj)
	{
		if (this == obj)
		{
			return true;
		}
		if (!(obj is PublicKeyToken publicKeyToken))
		{
			return false;
		}
		return Utils.Equals(Data, publicKeyToken.Data);
	}

	public override int GetHashCode()
	{
		return Utils.GetHashCode(Data);
	}
}
