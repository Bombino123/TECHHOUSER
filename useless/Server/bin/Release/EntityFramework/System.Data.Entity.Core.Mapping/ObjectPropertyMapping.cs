using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Mapping;

internal class ObjectPropertyMapping : ObjectMemberMapping
{
	internal EdmProperty ClrProperty => (EdmProperty)base.ClrMember;

	internal override MemberMappingKind MemberMappingKind => MemberMappingKind.ScalarPropertyMapping;

	internal ObjectPropertyMapping(EdmProperty edmProperty, EdmProperty clrProperty)
		: base(edmProperty, clrProperty)
	{
	}
}
