using System.Collections.Generic;

namespace System.Data.Entity.Core.Common.Utils;

internal class TrailingSpaceStringComparer : IEqualityComparer<string>
{
	internal static readonly TrailingSpaceStringComparer Instance = new TrailingSpaceStringComparer();

	private TrailingSpaceStringComparer()
	{
	}

	public bool Equals(string x, string y)
	{
		return StringComparer.OrdinalIgnoreCase.Equals(NormalizeString(x), NormalizeString(y));
	}

	public int GetHashCode(string obj)
	{
		return StringComparer.OrdinalIgnoreCase.GetHashCode(NormalizeString(obj));
	}

	internal static string NormalizeString(string value)
	{
		if (value == null || !value.EndsWith(" ", StringComparison.Ordinal))
		{
			return value;
		}
		return value.TrimEnd(new char[1] { ' ' });
	}
}
