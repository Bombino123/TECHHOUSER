using System.Collections.Generic;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Utilities;
using System.Threading;

namespace System.Data.Entity.Core.Metadata.Edm;

public sealed class AssociationEndMember : RelationshipEndMember
{
	private Func<RelationshipManager, RelatedEnd, RelatedEnd> _getRelatedEndMethod;

	public override BuiltInTypeKind BuiltInTypeKind => BuiltInTypeKind.AssociationEndMember;

	internal Func<RelationshipManager, RelatedEnd, RelatedEnd> GetRelatedEnd
	{
		get
		{
			return _getRelatedEndMethod;
		}
		set
		{
			Interlocked.CompareExchange(ref _getRelatedEndMethod, value, null);
		}
	}

	internal AssociationEndMember(string name, RefType endRefType, RelationshipMultiplicity multiplicity)
		: base(name, endRefType, multiplicity)
	{
	}

	internal AssociationEndMember(string name, EntityType entityType)
		: base(name, new RefType(entityType), RelationshipMultiplicity.ZeroOrOne)
	{
	}

	public static AssociationEndMember Create(string name, RefType endRefType, RelationshipMultiplicity multiplicity, OperationAction deleteAction, IEnumerable<MetadataProperty> metadataProperties)
	{
		Check.NotEmpty(name, "name");
		Check.NotNull(endRefType, "endRefType");
		AssociationEndMember associationEndMember = new AssociationEndMember(name, endRefType, multiplicity);
		associationEndMember.DeleteBehavior = deleteAction;
		if (metadataProperties != null)
		{
			associationEndMember.AddMetadataProperties(metadataProperties);
		}
		associationEndMember.SetReadOnly();
		return associationEndMember;
	}
}
