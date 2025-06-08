using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Data.Entity.Infrastructure;

[Serializable]
public sealed class ObjectReferenceEqualityComparer : IEqualityComparer<object>
{
	private static readonly ObjectReferenceEqualityComparer _default = new ObjectReferenceEqualityComparer();

	public static ObjectReferenceEqualityComparer Default => _default;

	bool IEqualityComparer<object>.Equals(object x, object y)
	{
		return x == y;
	}

	int IEqualityComparer<object>.GetHashCode(object obj)
	{
		return RuntimeHelpers.GetHashCode(obj);
	}
}
