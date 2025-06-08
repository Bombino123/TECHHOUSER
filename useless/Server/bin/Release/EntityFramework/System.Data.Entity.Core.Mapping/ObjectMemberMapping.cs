using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Mapping;

internal abstract class ObjectMemberMapping
{
	private readonly EdmMember m_edmMember;

	private readonly EdmMember m_clrMember;

	internal EdmMember EdmMember => m_edmMember;

	internal EdmMember ClrMember => m_clrMember;

	internal abstract MemberMappingKind MemberMappingKind { get; }

	protected ObjectMemberMapping(EdmMember edmMember, EdmMember clrMember)
	{
		m_edmMember = edmMember;
		m_clrMember = clrMember;
	}
}
