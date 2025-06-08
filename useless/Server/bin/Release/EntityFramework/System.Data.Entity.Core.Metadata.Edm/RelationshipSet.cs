namespace System.Data.Entity.Core.Metadata.Edm;

public abstract class RelationshipSet : EntitySetBase
{
	public new RelationshipType ElementType => (RelationshipType)base.ElementType;

	public override BuiltInTypeKind BuiltInTypeKind => BuiltInTypeKind.RelationshipSet;

	internal RelationshipSet(string name, string schema, string table, string definingQuery, RelationshipType relationshipType)
		: base(name, schema, table, definingQuery, relationshipType)
	{
	}
}
