using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal static class TypeUtils
{
	internal static bool IsStructuredType(TypeUsage type)
	{
		if (!TypeSemantics.IsReferenceType(type) && !TypeSemantics.IsRowType(type) && !TypeSemantics.IsEntityType(type) && !TypeSemantics.IsRelationshipType(type))
		{
			return TypeSemantics.IsComplexType(type);
		}
		return true;
	}

	internal static bool IsCollectionType(TypeUsage type)
	{
		return TypeSemantics.IsCollectionType(type);
	}

	internal static bool IsEnumerationType(TypeUsage type)
	{
		return TypeSemantics.IsEnumerationType(type);
	}

	internal static TypeUsage CreateCollectionType(TypeUsage elementType)
	{
		return TypeHelpers.CreateCollectionTypeUsage(elementType);
	}
}
