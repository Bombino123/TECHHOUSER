using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class RelPropertyHelper
{
	private readonly Dictionary<EntityTypeBase, List<RelProperty>> _relPropertyMap;

	private readonly HashSet<RelProperty> _interestingRelProperties;

	private void AddRelProperty(AssociationType associationType, AssociationEndMember fromEnd, AssociationEndMember toEnd)
	{
		if (toEnd.RelationshipMultiplicity == RelationshipMultiplicity.Many)
		{
			return;
		}
		RelProperty item = new RelProperty(associationType, fromEnd, toEnd);
		if (_interestingRelProperties != null && _interestingRelProperties.Contains(item))
		{
			EntityTypeBase elementType = ((RefType)fromEnd.TypeUsage.EdmType).ElementType;
			if (!_relPropertyMap.TryGetValue(elementType, out var value))
			{
				value = new List<RelProperty>();
				_relPropertyMap[elementType] = value;
			}
			value.Add(item);
		}
	}

	private void ProcessRelationship(RelationshipType relationshipType)
	{
		if (relationshipType is AssociationType associationType && associationType.AssociationEndMembers.Count == 2)
		{
			AssociationEndMember associationEndMember = associationType.AssociationEndMembers[0];
			AssociationEndMember associationEndMember2 = associationType.AssociationEndMembers[1];
			AddRelProperty(associationType, associationEndMember, associationEndMember2);
			AddRelProperty(associationType, associationEndMember2, associationEndMember);
		}
	}

	internal RelPropertyHelper(MetadataWorkspace ws, HashSet<RelProperty> interestingRelProperties)
	{
		_relPropertyMap = new Dictionary<EntityTypeBase, List<RelProperty>>();
		_interestingRelProperties = interestingRelProperties;
		foreach (RelationshipType item in ws.GetItems<RelationshipType>(DataSpace.CSpace))
		{
			ProcessRelationship(item);
		}
	}

	internal IEnumerable<RelProperty> GetDeclaredOnlyRelProperties(EntityTypeBase entityType)
	{
		if (!_relPropertyMap.TryGetValue(entityType, out var value))
		{
			yield break;
		}
		foreach (RelProperty item in value)
		{
			yield return item;
		}
	}

	internal IEnumerable<RelProperty> GetRelProperties(EntityTypeBase entityType)
	{
		if (entityType.BaseType != null)
		{
			foreach (RelProperty relProperty in GetRelProperties(entityType.BaseType as EntityTypeBase))
			{
				yield return relProperty;
			}
		}
		foreach (RelProperty declaredOnlyRelProperty in GetDeclaredOnlyRelProperties(entityType))
		{
			yield return declaredOnlyRelProperty;
		}
	}
}
