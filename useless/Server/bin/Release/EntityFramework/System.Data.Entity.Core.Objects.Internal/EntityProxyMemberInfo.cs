using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Objects.Internal;

internal sealed class EntityProxyMemberInfo
{
	private readonly EdmMember _member;

	private readonly int _propertyIndex;

	internal EdmMember EdmMember => _member;

	internal int PropertyIndex => _propertyIndex;

	internal EntityProxyMemberInfo(EdmMember member, int propertyIndex)
	{
		_member = member;
		_propertyIndex = propertyIndex;
	}
}
