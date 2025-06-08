namespace System.Data.Entity.Core.Objects.DataClasses;

internal interface IRelationshipFixer
{
	RelatedEnd CreateSourceEnd(RelationshipNavigation navigation, RelationshipManager relationshipManager);
}
