using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;

namespace System.Data.Entity.Core.Mapping.Update.Internal;

internal sealed class AssociationSetMetadata
{
	internal readonly Set<AssociationEndMember> RequiredEnds;

	internal readonly Set<AssociationEndMember> OptionalEnds;

	internal readonly Set<AssociationEndMember> IncludedValueEnds;

	internal bool HasEnds
	{
		get
		{
			if (0 >= RequiredEnds.Count && 0 >= OptionalEnds.Count)
			{
				return 0 < IncludedValueEnds.Count;
			}
			return true;
		}
	}

	internal AssociationSetMetadata(Set<EntitySet> affectedTables, AssociationSet associationSet, MetadataWorkspace workspace)
	{
		bool flag = 1 < affectedTables.Count;
		ReadOnlyMetadataCollection<AssociationSetEnd> associationSetEnds = associationSet.AssociationSetEnds;
		foreach (EntitySet affectedTable in affectedTables)
		{
			foreach (EntitySet item in MetadataHelper.GetInfluencingEntitySetsForTable(affectedTable, workspace))
			{
				foreach (AssociationSetEnd item2 in associationSetEnds)
				{
					if (item2.EntitySet.EdmEquals(item))
					{
						if (flag)
						{
							AddEnd(ref RequiredEnds, item2.CorrespondingAssociationEndMember);
						}
						else if (RequiredEnds == null || !RequiredEnds.Contains(item2.CorrespondingAssociationEndMember))
						{
							AddEnd(ref OptionalEnds, item2.CorrespondingAssociationEndMember);
						}
					}
				}
			}
		}
		FixSet(ref RequiredEnds);
		FixSet(ref OptionalEnds);
		foreach (ReferentialConstraint referentialConstraint in associationSet.ElementType.ReferentialConstraints)
		{
			AssociationEndMember element = (AssociationEndMember)referentialConstraint.FromRole;
			if (!RequiredEnds.Contains(element) && !OptionalEnds.Contains(element))
			{
				AddEnd(ref IncludedValueEnds, element);
			}
		}
		FixSet(ref IncludedValueEnds);
	}

	internal AssociationSetMetadata(IEnumerable<AssociationEndMember> requiredEnds)
	{
		if (requiredEnds.Any())
		{
			RequiredEnds = new Set<AssociationEndMember>(requiredEnds);
		}
		FixSet(ref RequiredEnds);
		FixSet(ref OptionalEnds);
		FixSet(ref IncludedValueEnds);
	}

	private static void AddEnd(ref Set<AssociationEndMember> set, AssociationEndMember element)
	{
		if (set == null)
		{
			set = new Set<AssociationEndMember>();
		}
		set.Add(element);
	}

	private static void FixSet(ref Set<AssociationEndMember> set)
	{
		if (set == null)
		{
			set = Set<AssociationEndMember>.Empty;
		}
		else
		{
			set.MakeReadOnly();
		}
	}
}
