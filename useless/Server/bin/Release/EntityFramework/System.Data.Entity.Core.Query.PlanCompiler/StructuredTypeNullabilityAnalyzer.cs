using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class StructuredTypeNullabilityAnalyzer : ColumnMapVisitor<HashSet<string>>
{
	internal static StructuredTypeNullabilityAnalyzer Instance = new StructuredTypeNullabilityAnalyzer();

	internal override void Visit(VarRefColumnMap columnMap, HashSet<string> typesNeedingNullSentinel)
	{
		AddTypeNeedingNullSentinel(typesNeedingNullSentinel, columnMap.Type);
		base.Visit(columnMap, typesNeedingNullSentinel);
	}

	private static void AddTypeNeedingNullSentinel(HashSet<string> typesNeedingNullSentinel, TypeUsage typeUsage)
	{
		if (TypeSemantics.IsCollectionType(typeUsage))
		{
			AddTypeNeedingNullSentinel(typesNeedingNullSentinel, TypeHelpers.GetElementTypeUsage(typeUsage));
			return;
		}
		if (TypeSemantics.IsRowType(typeUsage) || TypeSemantics.IsComplexType(typeUsage))
		{
			MarkAsNeedingNullSentinel(typesNeedingNullSentinel, typeUsage);
		}
		foreach (EdmMember allStructuralMember in TypeHelpers.GetAllStructuralMembers(typeUsage))
		{
			AddTypeNeedingNullSentinel(typesNeedingNullSentinel, allStructuralMember.TypeUsage);
		}
	}

	internal static void MarkAsNeedingNullSentinel(HashSet<string> typesNeedingNullSentinel, TypeUsage typeUsage)
	{
		typesNeedingNullSentinel.Add(typeUsage.EdmType.Identity);
	}
}
