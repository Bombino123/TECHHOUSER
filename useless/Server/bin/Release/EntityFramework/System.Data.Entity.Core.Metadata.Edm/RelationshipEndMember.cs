namespace System.Data.Entity.Core.Metadata.Edm;

public abstract class RelationshipEndMember : EdmMember
{
	private OperationAction _deleteBehavior;

	private RelationshipMultiplicity _relationshipMultiplicity;

	[MetadataProperty(BuiltInTypeKind.OperationAction, true)]
	public OperationAction DeleteBehavior
	{
		get
		{
			return _deleteBehavior;
		}
		set
		{
			Util.ThrowIfReadOnly(this);
			_deleteBehavior = value;
		}
	}

	[MetadataProperty(BuiltInTypeKind.RelationshipMultiplicity, false)]
	public RelationshipMultiplicity RelationshipMultiplicity
	{
		get
		{
			return _relationshipMultiplicity;
		}
		set
		{
			Util.ThrowIfReadOnly(this);
			_relationshipMultiplicity = value;
		}
	}

	internal RelationshipEndMember(string name, RefType endRefType, RelationshipMultiplicity multiplicity)
		: base(name, TypeUsage.Create(endRefType, new FacetValues
		{
			Nullable = false
		}))
	{
		_relationshipMultiplicity = multiplicity;
		_deleteBehavior = OperationAction.None;
	}

	public EntityType GetEntityType()
	{
		if (TypeUsage == null)
		{
			return null;
		}
		return (EntityType)((RefType)TypeUsage.EdmType).ElementType;
	}
}
