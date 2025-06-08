using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class ConstraintManager
{
	private readonly Dictionary<EntityContainer, EntityContainer> m_entityContainerMap;

	private readonly Dictionary<ExtentPair, List<ForeignKeyConstraint>> m_parentChildRelationships;

	internal bool IsParentChildRelationship(EntitySetBase table1, EntitySetBase table2, out List<ForeignKeyConstraint> constraints)
	{
		LoadRelationships(table1.EntityContainer);
		LoadRelationships(table2.EntityContainer);
		ExtentPair key = new ExtentPair(table1, table2);
		return m_parentChildRelationships.TryGetValue(key, out constraints);
	}

	internal void LoadRelationships(EntityContainer entityContainer)
	{
		if (m_entityContainerMap.ContainsKey(entityContainer))
		{
			return;
		}
		foreach (EntitySetBase baseEntitySet in entityContainer.BaseEntitySets)
		{
			if (!(baseEntitySet is RelationshipSet { ElementType: var elementType } relationshipSet) || !(elementType is AssociationType associationType) || !IsBinary(elementType))
			{
				continue;
			}
			foreach (ReferentialConstraint referentialConstraint in associationType.ReferentialConstraints)
			{
				ForeignKeyConstraint foreignKeyConstraint = new ForeignKeyConstraint(relationshipSet, referentialConstraint);
				if (!m_parentChildRelationships.TryGetValue(foreignKeyConstraint.Pair, out var value))
				{
					value = new List<ForeignKeyConstraint>();
					m_parentChildRelationships[foreignKeyConstraint.Pair] = value;
				}
				value.Add(foreignKeyConstraint);
			}
		}
		m_entityContainerMap[entityContainer] = entityContainer;
	}

	internal ConstraintManager()
	{
		m_entityContainerMap = new Dictionary<EntityContainer, EntityContainer>();
		m_parentChildRelationships = new Dictionary<ExtentPair, List<ForeignKeyConstraint>>();
	}

	private static bool IsBinary(RelationshipType relationshipType)
	{
		int num = 0;
		foreach (EdmMember member in relationshipType.Members)
		{
			if (member is RelationshipEndMember)
			{
				num++;
				if (num > 2)
				{
					return false;
				}
			}
		}
		return num == 2;
	}
}
