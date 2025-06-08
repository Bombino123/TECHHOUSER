using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures;

internal class ExtentKey : InternalBase
{
	private readonly List<MemberPath> m_keyFields;

	internal IEnumerable<MemberPath> KeyFields => m_keyFields;

	internal ExtentKey(IEnumerable<MemberPath> keyFields)
	{
		m_keyFields = new List<MemberPath>(keyFields);
	}

	internal static List<ExtentKey> GetKeysForEntityType(MemberPath prefix, EntityType entityType)
	{
		ExtentKey primaryKeyForEntityType = GetPrimaryKeyForEntityType(prefix, entityType);
		return new List<ExtentKey> { primaryKeyForEntityType };
	}

	internal static ExtentKey GetPrimaryKeyForEntityType(MemberPath prefix, EntityType entityType)
	{
		List<MemberPath> list = new List<MemberPath>();
		foreach (EdmMember keyMember in entityType.KeyMembers)
		{
			list.Add(new MemberPath(prefix, keyMember));
		}
		return new ExtentKey(list);
	}

	internal static ExtentKey GetKeyForRelationType(MemberPath prefix, AssociationType relationType)
	{
		List<MemberPath> list = new List<MemberPath>();
		foreach (AssociationEndMember associationEndMember in relationType.AssociationEndMembers)
		{
			MemberPath prefix2 = new MemberPath(prefix, associationEndMember);
			EntityType entityTypeForEnd = MetadataHelper.GetEntityTypeForEnd(associationEndMember);
			ExtentKey primaryKeyForEntityType = GetPrimaryKeyForEntityType(prefix2, entityTypeForEnd);
			list.AddRange(primaryKeyForEntityType.KeyFields);
		}
		return new ExtentKey(list);
	}

	internal string ToUserString()
	{
		return StringUtil.ToCommaSeparatedStringSorted(m_keyFields);
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		StringUtil.ToCommaSeparatedStringSorted(builder, m_keyFields);
	}
}
