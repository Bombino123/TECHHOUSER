using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Objects;

internal struct EntitySetQualifiedType : IEqualityComparer<EntitySetQualifiedType>
{
	internal static readonly IEqualityComparer<EntitySetQualifiedType> EqualityComparer = default(EntitySetQualifiedType);

	internal readonly Type ClrType;

	internal readonly EntitySet EntitySet;

	internal EntitySetQualifiedType(Type type, EntitySet set)
	{
		ClrType = EntityUtil.GetEntityIdentityType(type);
		EntitySet = set;
	}

	public bool Equals(EntitySetQualifiedType x, EntitySetQualifiedType y)
	{
		if ((object)x.ClrType == y.ClrType)
		{
			return x.EntitySet == y.EntitySet;
		}
		return false;
	}

	public int GetHashCode(EntitySetQualifiedType obj)
	{
		return obj.ClrType.GetHashCode() + obj.EntitySet.Name.GetHashCode() + obj.EntitySet.EntityContainer.Name.GetHashCode();
	}
}
