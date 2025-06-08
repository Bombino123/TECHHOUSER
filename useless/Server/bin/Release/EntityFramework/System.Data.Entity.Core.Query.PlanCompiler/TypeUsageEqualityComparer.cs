using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal sealed class TypeUsageEqualityComparer : IEqualityComparer<TypeUsage>
{
	internal static readonly TypeUsageEqualityComparer Instance = new TypeUsageEqualityComparer();

	private TypeUsageEqualityComparer()
	{
	}

	public bool Equals(TypeUsage x, TypeUsage y)
	{
		if (x == null || y == null)
		{
			return false;
		}
		return Equals(x.EdmType, y.EdmType);
	}

	public int GetHashCode(TypeUsage obj)
	{
		return obj.EdmType.Identity.GetHashCode();
	}

	internal static bool Equals(EdmType x, EdmType y)
	{
		return x.Identity.Equals(y.Identity);
	}
}
