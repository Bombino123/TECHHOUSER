using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class ForeignKeyConstraint
{
	private readonly ExtentPair m_extentPair;

	private readonly List<string> m_parentKeys;

	private readonly List<string> m_childKeys;

	private readonly ReferentialConstraint m_constraint;

	private Dictionary<string, string> m_keyMap;

	internal List<string> ParentKeys => m_parentKeys;

	internal List<string> ChildKeys => m_childKeys;

	internal ExtentPair Pair => m_extentPair;

	internal RelationshipMultiplicity ChildMultiplicity => m_constraint.ToRole.RelationshipMultiplicity;

	internal bool GetParentProperty(string childPropertyName, out string parentPropertyName)
	{
		BuildKeyMap();
		return m_keyMap.TryGetValue(childPropertyName, out parentPropertyName);
	}

	internal ForeignKeyConstraint(RelationshipSet relationshipSet, ReferentialConstraint constraint)
	{
		AssociationSet associationSet = relationshipSet as AssociationSet;
		AssociationEndMember associationEndMember = constraint.FromRole as AssociationEndMember;
		AssociationEndMember associationEndMember2 = constraint.ToRole as AssociationEndMember;
		if (associationSet == null || associationEndMember == null || associationEndMember2 == null)
		{
			throw new NotSupportedException();
		}
		m_constraint = constraint;
		EntitySet entitySetAtEnd = MetadataHelper.GetEntitySetAtEnd(associationSet, associationEndMember);
		EntitySet entitySetAtEnd2 = MetadataHelper.GetEntitySetAtEnd(associationSet, associationEndMember2);
		m_extentPair = new ExtentPair(entitySetAtEnd, entitySetAtEnd2);
		m_childKeys = new List<string>();
		foreach (EdmProperty toProperty in constraint.ToProperties)
		{
			m_childKeys.Add(toProperty.Name);
		}
		m_parentKeys = new List<string>();
		foreach (EdmProperty fromProperty in constraint.FromProperties)
		{
			m_parentKeys.Add(fromProperty.Name);
		}
		PlanCompiler.Assert(associationEndMember.RelationshipMultiplicity == RelationshipMultiplicity.ZeroOrOne || RelationshipMultiplicity.One == associationEndMember.RelationshipMultiplicity, "from-end of relationship constraint cannot have multiplicity greater than 1");
	}

	private void BuildKeyMap()
	{
		if (m_keyMap != null)
		{
			return;
		}
		m_keyMap = new Dictionary<string, string>();
		IEnumerator<EdmProperty> enumerator = m_constraint.FromProperties.GetEnumerator();
		IEnumerator<EdmProperty> enumerator2 = m_constraint.ToProperties.GetEnumerator();
		while (true)
		{
			bool num = !enumerator.MoveNext();
			bool flag = !enumerator2.MoveNext();
			PlanCompiler.Assert(num == flag, "key count mismatch");
			if (!num)
			{
				m_keyMap[enumerator2.Current.Name] = enumerator.Current.Name;
				continue;
			}
			break;
		}
	}
}
