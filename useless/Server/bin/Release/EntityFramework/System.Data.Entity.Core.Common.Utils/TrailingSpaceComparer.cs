using System.Collections.Generic;

namespace System.Data.Entity.Core.Common.Utils;

internal class TrailingSpaceComparer : IEqualityComparer<object>
{
	internal static readonly TrailingSpaceComparer Instance = new TrailingSpaceComparer();

	private static readonly IEqualityComparer<object> _template = EqualityComparer<object>.Default;

	private TrailingSpaceComparer()
	{
	}

	bool IEqualityComparer<object>.Equals(object x, object y)
	{
		if (x is string x2 && y is string y2)
		{
			return TrailingSpaceStringComparer.Instance.Equals(x2, y2);
		}
		return _template.Equals(x, y);
	}

	int IEqualityComparer<object>.GetHashCode(object obj)
	{
		if (obj is string obj2)
		{
			return TrailingSpaceStringComparer.Instance.GetHashCode(obj2);
		}
		return _template.GetHashCode(obj);
	}
}
