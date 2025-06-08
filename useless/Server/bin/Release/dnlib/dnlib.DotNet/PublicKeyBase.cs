using System;

namespace dnlib.DotNet;

public abstract class PublicKeyBase
{
	protected readonly byte[] data;

	private static readonly byte[] EmptyByteArray = Array2.Empty<byte>();

	public bool IsNullOrEmpty
	{
		get
		{
			if (data != null)
			{
				return data.Length == 0;
			}
			return true;
		}
	}

	public bool IsNull => Data == null;

	public virtual byte[] Data => data;

	public abstract PublicKeyToken Token { get; }

	protected PublicKeyBase(byte[] data)
	{
		this.data = data;
	}

	protected PublicKeyBase(string hexString)
	{
		data = Parse(hexString);
	}

	private static byte[] Parse(string hexString)
	{
		if (hexString == null || hexString == "null")
		{
			return null;
		}
		return Utils.ParseBytes(hexString);
	}

	public static bool IsNullOrEmpty2(PublicKeyBase a)
	{
		return a?.IsNullOrEmpty ?? true;
	}

	public static PublicKeyToken ToPublicKeyToken(PublicKeyBase pkb)
	{
		if (pkb is PublicKeyToken result)
		{
			return result;
		}
		if (pkb is PublicKey publicKey)
		{
			return publicKey.Token;
		}
		return null;
	}

	public static int TokenCompareTo(PublicKeyBase a, PublicKeyBase b)
	{
		if (a == b)
		{
			return 0;
		}
		return TokenCompareTo(ToPublicKeyToken(a), ToPublicKeyToken(b));
	}

	public static bool TokenEquals(PublicKeyBase a, PublicKeyBase b)
	{
		return TokenCompareTo(a, b) == 0;
	}

	public static int TokenCompareTo(PublicKeyToken a, PublicKeyToken b)
	{
		if (a == b)
		{
			return 0;
		}
		return TokenCompareTo(a?.Data, b?.Data);
	}

	private static int TokenCompareTo(byte[] a, byte[] b)
	{
		return Utils.CompareTo(a ?? EmptyByteArray, b ?? EmptyByteArray);
	}

	public static bool TokenEquals(PublicKeyToken a, PublicKeyToken b)
	{
		return TokenCompareTo(a, b) == 0;
	}

	public static int GetHashCodeToken(PublicKeyBase a)
	{
		return GetHashCode(ToPublicKeyToken(a));
	}

	public static int GetHashCode(PublicKeyToken a)
	{
		if (a == null)
		{
			return 0;
		}
		return Utils.GetHashCode(a.Data);
	}

	public static PublicKey CreatePublicKey(byte[] data)
	{
		if (data == null)
		{
			return null;
		}
		return new PublicKey(data);
	}

	public static PublicKeyToken CreatePublicKeyToken(byte[] data)
	{
		if (data == null)
		{
			return null;
		}
		return new PublicKeyToken(data);
	}

	public static byte[] GetRawData(PublicKeyBase pkb)
	{
		return pkb?.Data;
	}

	public override string ToString()
	{
		byte[] array = Data;
		if (array == null || array.Length == 0)
		{
			return "null";
		}
		return Utils.ToHex(array, upper: false);
	}
}
