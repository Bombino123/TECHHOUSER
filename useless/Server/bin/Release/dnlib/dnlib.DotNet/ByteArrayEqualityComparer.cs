using System.Collections.Generic;

namespace dnlib.DotNet;

internal sealed class ByteArrayEqualityComparer : IEqualityComparer<byte[]>
{
	public static readonly ByteArrayEqualityComparer Instance = new ByteArrayEqualityComparer();

	public bool Equals(byte[] x, byte[] y)
	{
		return Utils.Equals(x, y);
	}

	public int GetHashCode(byte[] obj)
	{
		return Utils.GetHashCode(obj);
	}
}
