using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public sealed class UTF8StringEqualityComparer : IEqualityComparer<UTF8String>
{
	public static readonly UTF8StringEqualityComparer Instance = new UTF8StringEqualityComparer();

	public bool Equals(UTF8String x, UTF8String y)
	{
		return UTF8String.Equals(x, y);
	}

	public int GetHashCode(UTF8String obj)
	{
		return UTF8String.GetHashCode(obj);
	}
}
