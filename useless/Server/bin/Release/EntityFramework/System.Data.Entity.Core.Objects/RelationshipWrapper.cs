using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Objects;

internal sealed class RelationshipWrapper : IEquatable<RelationshipWrapper>
{
	internal readonly AssociationSet AssociationSet;

	internal readonly EntityKey Key0;

	internal readonly EntityKey Key1;

	internal ReadOnlyMetadataCollection<AssociationEndMember> AssociationEndMembers => AssociationSet.ElementType.AssociationEndMembers;

	internal RelationshipWrapper(AssociationSet extent, EntityKey key)
	{
		AssociationSet = extent;
		Key0 = key;
		Key1 = key;
	}

	internal RelationshipWrapper(RelationshipWrapper wrapper, int ordinal, EntityKey key)
	{
		AssociationSet = wrapper.AssociationSet;
		Key0 = ((ordinal == 0) ? key : wrapper.Key0);
		Key1 = ((ordinal == 0) ? wrapper.Key1 : key);
	}

	internal RelationshipWrapper(AssociationSet extent, KeyValuePair<string, EntityKey> roleAndKey1, KeyValuePair<string, EntityKey> roleAndKey2)
		: this(extent, roleAndKey1.Key, roleAndKey1.Value, roleAndKey2.Key, roleAndKey2.Value)
	{
	}

	internal RelationshipWrapper(AssociationSet extent, string role0, EntityKey key0, string role1, EntityKey key1)
	{
		AssociationSet = extent;
		if (extent.ElementType.AssociationEndMembers[0].Name == role0)
		{
			Key0 = key0;
			Key1 = key1;
		}
		else
		{
			Key0 = key1;
			Key1 = key0;
		}
	}

	internal AssociationEndMember GetAssociationEndMember(EntityKey key)
	{
		return AssociationEndMembers[(Key0 != key) ? 1 : 0];
	}

	internal EntityKey GetOtherEntityKey(EntityKey key)
	{
		if (!(Key0 == key))
		{
			if (!(Key1 == key))
			{
				return null;
			}
			return Key0;
		}
		return Key1;
	}

	internal EntityKey GetEntityKey(int ordinal)
	{
		return ordinal switch
		{
			0 => Key0, 
			1 => Key1, 
			_ => throw new ArgumentOutOfRangeException("ordinal"), 
		};
	}

	public override int GetHashCode()
	{
		return AssociationSet.Name.GetHashCode() ^ (Key0.GetHashCode() + Key1.GetHashCode());
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as RelationshipWrapper);
	}

	public bool Equals(RelationshipWrapper wrapper)
	{
		if (this != wrapper)
		{
			if (wrapper != null && AssociationSet == wrapper.AssociationSet && Key0.Equals(wrapper.Key0))
			{
				return Key1.Equals(wrapper.Key1);
			}
			return false;
		}
		return true;
	}
}
