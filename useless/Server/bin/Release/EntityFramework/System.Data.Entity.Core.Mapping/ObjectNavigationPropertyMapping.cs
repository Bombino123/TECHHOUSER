using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Mapping;

internal class ObjectNavigationPropertyMapping : ObjectMemberMapping
{
	internal override MemberMappingKind MemberMappingKind => MemberMappingKind.NavigationPropertyMapping;

	internal ObjectNavigationPropertyMapping(NavigationProperty edmNavigationProperty, NavigationProperty clrNavigationProperty)
		: base(edmNavigationProperty, clrNavigationProperty)
	{
	}
}
