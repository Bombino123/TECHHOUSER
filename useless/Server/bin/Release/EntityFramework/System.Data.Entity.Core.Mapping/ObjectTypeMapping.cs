using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Mapping;

internal class ObjectTypeMapping : MappingBase
{
	private readonly EdmType m_clrType;

	private readonly EdmType m_cdmType;

	private readonly string identity;

	private readonly Dictionary<string, ObjectMemberMapping> m_memberMapping;

	private static readonly Dictionary<string, ObjectMemberMapping> EmptyMemberMapping = new Dictionary<string, ObjectMemberMapping>(0);

	public override BuiltInTypeKind BuiltInTypeKind => BuiltInTypeKind.MetadataItem;

	internal EdmType ClrType => m_clrType;

	internal override MetadataItem EdmItem => EdmType;

	internal EdmType EdmType => m_cdmType;

	internal override string Identity => identity;

	internal ObjectTypeMapping(EdmType clrType, EdmType cdmType)
	{
		m_clrType = clrType;
		m_cdmType = cdmType;
		identity = clrType.Identity + ":" + cdmType.Identity;
		if (Helper.IsStructuralType(cdmType))
		{
			m_memberMapping = new Dictionary<string, ObjectMemberMapping>(((StructuralType)cdmType).Members.Count);
		}
		else
		{
			m_memberMapping = EmptyMemberMapping;
		}
	}

	internal ObjectPropertyMapping GetPropertyMap(string propertyName)
	{
		ObjectMemberMapping memberMap = GetMemberMap(propertyName, ignoreCase: false);
		if (memberMap != null && (memberMap.MemberMappingKind == MemberMappingKind.ScalarPropertyMapping || memberMap.MemberMappingKind == MemberMappingKind.ComplexPropertyMapping))
		{
			return (ObjectPropertyMapping)memberMap;
		}
		return null;
	}

	internal void AddMemberMap(ObjectMemberMapping memberMapping)
	{
		m_memberMapping.Add(memberMapping.EdmMember.Name, memberMapping);
	}

	internal ObjectMemberMapping GetMemberMapForClrMember(string clrMemberName, bool ignoreCase)
	{
		return GetMemberMap(clrMemberName, ignoreCase);
	}

	private ObjectMemberMapping GetMemberMap(string propertyName, bool ignoreCase)
	{
		Check.NotEmpty(propertyName, "propertyName");
		ObjectMemberMapping value = null;
		if (!ignoreCase)
		{
			m_memberMapping.TryGetValue(propertyName, out value);
		}
		else
		{
			foreach (KeyValuePair<string, ObjectMemberMapping> item in m_memberMapping)
			{
				if (item.Key.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
				{
					if (value != null)
					{
						throw new MappingException(Strings.Mapping_Duplicate_PropertyMap_CaseInsensitive(propertyName));
					}
					value = item.Value;
				}
			}
		}
		return value;
	}

	public override string ToString()
	{
		return Identity;
	}
}
