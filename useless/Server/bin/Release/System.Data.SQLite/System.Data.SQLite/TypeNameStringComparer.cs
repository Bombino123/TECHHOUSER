using System.Collections.Generic;

namespace System.Data.SQLite;

internal sealed class TypeNameStringComparer : IEqualityComparer<string>, IComparer<string>
{
	public bool Equals(string left, string right)
	{
		return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
	}

	public int GetHashCode(string value)
	{
		if (value != null)
		{
			return StringComparer.OrdinalIgnoreCase.GetHashCode(value);
		}
		throw new ArgumentNullException("value");
	}

	public int Compare(string x, string y)
	{
		if (x == null && y == null)
		{
			return 0;
		}
		if (x == null)
		{
			return -1;
		}
		if (y == null)
		{
			return 1;
		}
		return x.CompareTo(y);
	}
}
