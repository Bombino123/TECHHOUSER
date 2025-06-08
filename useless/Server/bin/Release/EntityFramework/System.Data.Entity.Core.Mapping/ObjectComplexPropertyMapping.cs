using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Mapping;

internal class ObjectComplexPropertyMapping : ObjectPropertyMapping
{
	internal override MemberMappingKind MemberMappingKind => MemberMappingKind.ComplexPropertyMapping;

	internal ObjectComplexPropertyMapping(EdmProperty edmProperty, EdmProperty clrProperty)
		: base(edmProperty, clrProperty)
	{
	}
}
