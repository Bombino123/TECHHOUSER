using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Metadata.Edm;

public sealed class AssociationSetEnd : MetadataItem
{
	private readonly EntitySet _entitySet;

	private readonly AssociationSet _parentSet;

	private readonly AssociationEndMember _endMember;

	public override BuiltInTypeKind BuiltInTypeKind => BuiltInTypeKind.AssociationSetEnd;

	[MetadataProperty(BuiltInTypeKind.AssociationSet, false)]
	public AssociationSet ParentAssociationSet => _parentSet;

	[MetadataProperty(BuiltInTypeKind.AssociationEndMember, false)]
	public AssociationEndMember CorrespondingAssociationEndMember => _endMember;

	[MetadataProperty(PrimitiveTypeKind.String, false)]
	public string Name => CorrespondingAssociationEndMember.Name;

	[MetadataProperty(PrimitiveTypeKind.String, false)]
	[Obsolete("This property is going away, please use the Name property instead")]
	public string Role => Name;

	[MetadataProperty(BuiltInTypeKind.EntitySet, false)]
	public EntitySet EntitySet => _entitySet;

	internal override string Identity => Name;

	internal AssociationSetEnd(EntitySet entitySet, AssociationSet parentSet, AssociationEndMember endMember)
	{
		_entitySet = Check.NotNull(entitySet, "entitySet");
		_parentSet = Check.NotNull(parentSet, "parentSet");
		_endMember = Check.NotNull(endMember, "endMember");
	}

	public override string ToString()
	{
		return Name;
	}

	internal override void SetReadOnly()
	{
		if (!base.IsReadOnly)
		{
			base.SetReadOnly();
			ParentAssociationSet?.SetReadOnly();
			CorrespondingAssociationEndMember?.SetReadOnly();
			EntitySet?.SetReadOnly();
		}
	}
}
