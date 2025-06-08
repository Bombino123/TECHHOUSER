using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Mapping;

internal class ObjectAssociationEndMapping : ObjectMemberMapping
{
	internal override MemberMappingKind MemberMappingKind => MemberMappingKind.AssociationEndMapping;

	internal ObjectAssociationEndMapping(AssociationEndMember edmAssociationEnd, AssociationEndMember clrAssociationEnd)
		: base(edmAssociationEnd, clrAssociationEnd)
	{
	}
}
