using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class DiscriminatorMapInfo
{
	internal EntityTypeBase RootEntityType;

	internal bool IncludesSubTypes;

	internal ExplicitDiscriminatorMap DiscriminatorMap;

	internal DiscriminatorMapInfo(EntityTypeBase rootEntityType, bool includesSubTypes, ExplicitDiscriminatorMap discriminatorMap)
	{
		RootEntityType = rootEntityType;
		IncludesSubTypes = includesSubTypes;
		DiscriminatorMap = discriminatorMap;
	}

	internal void Merge(EntityTypeBase neededRootEntityType, bool includesSubtypes, ExplicitDiscriminatorMap discriminatorMap)
	{
		if (RootEntityType != neededRootEntityType || IncludesSubTypes != includesSubtypes)
		{
			if (!IncludesSubTypes || !includesSubtypes)
			{
				DiscriminatorMap = null;
			}
			if (TypeSemantics.IsSubTypeOf(RootEntityType, neededRootEntityType))
			{
				RootEntityType = neededRootEntityType;
				DiscriminatorMap = discriminatorMap;
			}
			if (!TypeSemantics.IsSubTypeOf(neededRootEntityType, RootEntityType))
			{
				DiscriminatorMap = null;
			}
		}
	}
}
