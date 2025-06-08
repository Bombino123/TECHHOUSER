using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;

namespace System.Data.Entity.Core.Objects.Internal;

internal class ForeignKeyFactory
{
	private const string s_NullPart = "EntityHasNullForeignKey";

	private const string s_NullForeignKey = "EntityHasNullForeignKey.EntityHasNullForeignKey";

	public static bool IsConceptualNullKey(EntityKey key)
	{
		if (key == null)
		{
			return false;
		}
		if (string.Equals(key.EntityContainerName, "EntityHasNullForeignKey"))
		{
			return string.Equals(key.EntitySetName, "EntityHasNullForeignKey");
		}
		return false;
	}

	public static bool IsConceptualNullKeyChanged(EntityKey conceptualNullKey, EntityKey realKey)
	{
		if (realKey == null)
		{
			return true;
		}
		return !EntityKey.InternalEquals(conceptualNullKey, realKey, compareEntitySets: false);
	}

	public static EntityKey CreateConceptualNullKey(EntityKey originalKey)
	{
		return new EntityKey("EntityHasNullForeignKey.EntityHasNullForeignKey", originalKey.EntityKeyValues);
	}

	public static EntityKey CreateKeyFromForeignKeyValues(EntityEntry dependentEntry, RelatedEnd relatedEnd)
	{
		ReferentialConstraint constraint = ((AssociationType)relatedEnd.RelationMetadata).ReferentialConstraints.First();
		return CreateKeyFromForeignKeyValues(dependentEntry, constraint, relatedEnd.GetTargetEntitySetFromRelationshipSet(), useOriginalValues: false);
	}

	public static EntityKey CreateKeyFromForeignKeyValues(EntityEntry dependentEntry, ReferentialConstraint constraint, EntitySet principalEntitySet, bool useOriginalValues)
	{
		ReadOnlyMetadataCollection<EdmProperty> toProperties = constraint.ToProperties;
		int count = toProperties.Count;
		if (count == 1)
		{
			object obj = (useOriginalValues ? dependentEntry.GetOriginalEntityValue(toProperties.First().Name) : dependentEntry.GetCurrentEntityValue(toProperties.First().Name));
			if (obj != DBNull.Value)
			{
				return new EntityKey(principalEntitySet, obj);
			}
			return null;
		}
		string[] keyMemberNames = principalEntitySet.ElementType.KeyMemberNames;
		object[] array = new object[count];
		ReadOnlyMetadataCollection<EdmProperty> fromProperties = constraint.FromProperties;
		for (int i = 0; i < count; i++)
		{
			object obj2 = (useOriginalValues ? dependentEntry.GetOriginalEntityValue(toProperties[i].Name) : dependentEntry.GetCurrentEntityValue(toProperties[i].Name));
			if (obj2 == DBNull.Value)
			{
				return null;
			}
			int num = Array.IndexOf(keyMemberNames, fromProperties[i].Name);
			array[num] = obj2;
		}
		return new EntityKey(principalEntitySet, array);
	}
}
