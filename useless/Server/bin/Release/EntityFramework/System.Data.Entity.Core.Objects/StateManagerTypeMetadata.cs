using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Objects;

internal class StateManagerTypeMetadata
{
	private readonly TypeUsage _typeUsage;

	private readonly StateManagerMemberMetadata[] _members;

	private readonly Dictionary<string, int> _objectNameToOrdinal;

	private readonly Dictionary<string, int> _cLayerNameToOrdinal;

	private readonly DataRecordInfo _recordInfo;

	internal TypeUsage CdmMetadata => _typeUsage;

	internal DataRecordInfo DataRecordInfo => _recordInfo;

	internal virtual int FieldCount => _members.Length;

	internal IEnumerable<StateManagerMemberMetadata> Members => _members;

	internal StateManagerTypeMetadata()
	{
	}

	internal StateManagerTypeMetadata(EdmType edmType, ObjectTypeMapping mapping)
	{
		_typeUsage = TypeUsage.Create(edmType);
		_recordInfo = new DataRecordInfo(_typeUsage);
		ReadOnlyMetadataCollection<EdmProperty> properties = TypeHelpers.GetProperties(edmType);
		_members = new StateManagerMemberMetadata[properties.Count];
		_objectNameToOrdinal = new Dictionary<string, int>(properties.Count);
		_cLayerNameToOrdinal = new Dictionary<string, int>(properties.Count);
		ReadOnlyMetadataCollection<EdmMember> readOnlyMetadataCollection = null;
		if (Helper.IsEntityType(edmType))
		{
			readOnlyMetadataCollection = ((EntityType)edmType).KeyMembers;
		}
		for (int i = 0; i < _members.Length; i++)
		{
			EdmProperty edmProperty = properties[i];
			ObjectPropertyMapping objectPropertyMapping = null;
			if (mapping != null)
			{
				objectPropertyMapping = mapping.GetPropertyMap(edmProperty.Name);
				if (objectPropertyMapping != null)
				{
					_objectNameToOrdinal.Add(objectPropertyMapping.ClrProperty.Name, i);
				}
			}
			_cLayerNameToOrdinal.Add(edmProperty.Name, i);
			_members[i] = new StateManagerMemberMetadata(objectPropertyMapping, edmProperty, readOnlyMetadataCollection?.Contains(edmProperty) ?? false);
		}
	}

	internal Type GetFieldType(int ordinal)
	{
		return Member(ordinal).ClrType;
	}

	internal virtual StateManagerMemberMetadata Member(int ordinal)
	{
		if ((uint)ordinal < (uint)_members.Length)
		{
			return _members[ordinal];
		}
		throw new ArgumentOutOfRangeException("ordinal");
	}

	internal string CLayerMemberName(int ordinal)
	{
		return Member(ordinal).CLayerName;
	}

	internal int GetOrdinalforOLayerMemberName(string name)
	{
		if (string.IsNullOrEmpty(name) || !_objectNameToOrdinal.TryGetValue(name, out var value))
		{
			return -1;
		}
		return value;
	}

	internal int GetOrdinalforCLayerMemberName(string name)
	{
		if (string.IsNullOrEmpty(name) || !_cLayerNameToOrdinal.TryGetValue(name, out var value))
		{
			return -1;
		}
		return value;
	}
}
