using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class RelProperty
{
	private readonly RelationshipType m_relationshipType;

	private readonly RelationshipEndMember m_fromEnd;

	private readonly RelationshipEndMember m_toEnd;

	public RelationshipType Relationship => m_relationshipType;

	public RelationshipEndMember FromEnd => m_fromEnd;

	public RelationshipEndMember ToEnd => m_toEnd;

	internal RelProperty(RelationshipType relationshipType, RelationshipEndMember fromEnd, RelationshipEndMember toEnd)
	{
		m_relationshipType = relationshipType;
		m_fromEnd = fromEnd;
		m_toEnd = toEnd;
	}

	public override bool Equals(object obj)
	{
		if (obj is RelProperty relProperty && Relationship.EdmEquals(relProperty.Relationship) && FromEnd.EdmEquals(relProperty.FromEnd))
		{
			return ToEnd.EdmEquals(relProperty.ToEnd);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ToEnd.Identity.GetHashCode();
	}

	[DebuggerNonUserCode]
	public override string ToString()
	{
		return m_relationshipType?.ToString() + ":" + m_fromEnd?.ToString() + ":" + m_toEnd;
	}
}
