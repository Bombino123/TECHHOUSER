using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Objects.DataClasses;

[Serializable]
internal class RelationshipFixer<TSourceEntity, TTargetEntity> : IRelationshipFixer where TSourceEntity : class where TTargetEntity : class
{
	private readonly RelationshipMultiplicity _sourceRoleMultiplicity;

	private readonly RelationshipMultiplicity _targetRoleMultiplicity;

	internal RelationshipFixer(RelationshipMultiplicity sourceRoleMultiplicity, RelationshipMultiplicity targetRoleMultiplicity)
	{
		_sourceRoleMultiplicity = sourceRoleMultiplicity;
		_targetRoleMultiplicity = targetRoleMultiplicity;
	}

	RelatedEnd IRelationshipFixer.CreateSourceEnd(RelationshipNavigation navigation, RelationshipManager relationshipManager)
	{
		return relationshipManager.CreateRelatedEnd<TTargetEntity, TSourceEntity>(navigation, _targetRoleMultiplicity, _sourceRoleMultiplicity, null);
	}
}
