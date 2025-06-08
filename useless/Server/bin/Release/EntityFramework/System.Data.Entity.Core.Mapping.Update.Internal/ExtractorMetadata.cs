using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Resources;
using System.Linq;

namespace System.Data.Entity.Core.Mapping.Update.Internal;

internal class ExtractorMetadata
{
	private class MemberInformation
	{
		internal readonly int Ordinal;

		internal readonly int? EntityKeyOrdinal;

		internal readonly PropagatorFlags Flags;

		internal readonly bool IsServerGenerated;

		internal readonly bool CheckIsNotNull;

		internal readonly EdmMember Member;

		internal bool IsKeyMember => PropagatorFlags.Key == (Flags & PropagatorFlags.Key);

		internal bool IsForeignKeyMember => PropagatorFlags.ForeignKey == (Flags & PropagatorFlags.ForeignKey);

		internal MemberInformation(int ordinal, int? entityKeyOrdinal, PropagatorFlags flags, EdmMember member, bool isServerGenerated, bool isNullConditionMember)
		{
			Ordinal = ordinal;
			EntityKeyOrdinal = entityKeyOrdinal;
			Flags = flags;
			Member = member;
			IsServerGenerated = isServerGenerated;
			CheckIsNotNull = !TypeSemantics.IsNullable(member) && (isNullConditionMember || member.TypeUsage.EdmType.BuiltInTypeKind == BuiltInTypeKind.ComplexType);
		}
	}

	private readonly MemberInformation[] m_memberMap;

	private readonly StructuralType m_type;

	private readonly UpdateTranslator m_translator;

	internal ExtractorMetadata(EntitySetBase entitySetBase, StructuralType type, UpdateTranslator translator)
	{
		m_type = type;
		m_translator = translator;
		EntityType entityType = null;
		Set<EdmMember> set;
		Set<EdmMember> set2;
		switch (type.BuiltInTypeKind)
		{
		case BuiltInTypeKind.RowType:
			set = new Set<EdmMember>(((RowType)type).Properties).MakeReadOnly();
			set2 = Set<EdmMember>.Empty;
			break;
		case BuiltInTypeKind.EntityType:
			entityType = (EntityType)type;
			set = new Set<EdmMember>(entityType.KeyMembers).MakeReadOnly();
			set2 = new Set<EdmMember>(((EntitySet)entitySetBase).ForeignKeyDependents.SelectMany((Tuple<AssociationSet, ReferentialConstraint> fk) => fk.Item2.ToProperties)).MakeReadOnly();
			break;
		default:
			set = Set<EdmMember>.Empty;
			set2 = Set<EdmMember>.Empty;
			break;
		}
		IBaseList<EdmMember> allStructuralMembers = TypeHelpers.GetAllStructuralMembers(type);
		m_memberMap = new MemberInformation[allStructuralMembers.Count];
		for (int i = 0; i < allStructuralMembers.Count; i++)
		{
			EdmMember edmMember = allStructuralMembers[i];
			PropagatorFlags propagatorFlags = PropagatorFlags.NoFlags;
			int? entityKeyOrdinal = null;
			if (set.Contains(edmMember))
			{
				propagatorFlags |= PropagatorFlags.Key;
				if (entityType != null)
				{
					entityKeyOrdinal = entityType.KeyMembers.IndexOf(edmMember);
				}
			}
			if (set2.Contains(edmMember))
			{
				propagatorFlags |= PropagatorFlags.ForeignKey;
			}
			if (MetadataHelper.GetConcurrencyMode(edmMember) == ConcurrencyMode.Fixed)
			{
				propagatorFlags |= PropagatorFlags.ConcurrencyValue;
			}
			bool isServerGenerated = m_translator.ViewLoader.IsServerGen(entitySetBase, m_translator.MetadataWorkspace, edmMember);
			bool isNullConditionMember = m_translator.ViewLoader.IsNullConditionMember(entitySetBase, m_translator.MetadataWorkspace, edmMember);
			m_memberMap[i] = new MemberInformation(i, entityKeyOrdinal, propagatorFlags, edmMember, isServerGenerated, isNullConditionMember);
		}
	}

	internal PropagatorResult RetrieveMember(IEntityStateEntry stateEntry, IExtendedDataRecord record, bool useCurrentValues, EntityKey key, int ordinal, ModifiedPropertiesBehavior modifiedPropertiesBehavior)
	{
		MemberInformation memberInformation = m_memberMap[ordinal];
		int identifier;
		if (!memberInformation.IsKeyMember)
		{
			identifier = ((!memberInformation.IsForeignKeyMember) ? (-1) : m_translator.KeyManager.GetKeyIdentifierForMember(key, record.GetName(ordinal), useCurrentValues));
		}
		else
		{
			int value = memberInformation.EntityKeyOrdinal.Value;
			identifier = m_translator.KeyManager.GetKeyIdentifierForMemberOffset(key, value, ((EntityType)m_type).KeyMembers.Count);
		}
		bool flag = modifiedPropertiesBehavior == ModifiedPropertiesBehavior.AllModified || (modifiedPropertiesBehavior == ModifiedPropertiesBehavior.SomeModified && stateEntry.ModifiedProperties != null && stateEntry.ModifiedProperties[memberInformation.Ordinal]);
		if (memberInformation.CheckIsNotNull && record.IsDBNull(ordinal))
		{
			throw EntityUtil.Update(Strings.Update_NullValue(record.GetName(ordinal)), null, stateEntry);
		}
		object value2 = record.GetValue(ordinal);
		if (value2 is EntityKey entityKey)
		{
			return CreateEntityKeyResult(stateEntry, entityKey);
		}
		if (value2 is IExtendedDataRecord record2)
		{
			ModifiedPropertiesBehavior modifiedPropertiesBehavior2 = ((!flag) ? ModifiedPropertiesBehavior.NoneModified : ModifiedPropertiesBehavior.AllModified);
			UpdateTranslator translator = m_translator;
			return ExtractResultFromRecord(stateEntry, flag, record2, useCurrentValues, translator, modifiedPropertiesBehavior2);
		}
		return CreateSimpleResult(stateEntry, record, memberInformation, identifier, flag, ordinal, value2);
	}

	private PropagatorResult CreateEntityKeyResult(IEntityStateEntry stateEntry, EntityKey entityKey)
	{
		RowType keyRowType = entityKey.GetEntitySet(m_translator.MetadataWorkspace).ElementType.GetKeyRowType();
		ExtractorMetadata extractorMetadata = m_translator.GetExtractorMetadata(stateEntry.EntitySet, keyRowType);
		PropagatorResult[] array = new PropagatorResult[keyRowType.Properties.Count];
		for (int i = 0; i < keyRowType.Properties.Count; i++)
		{
			EdmMember edmMember = keyRowType.Properties[i];
			MemberInformation memberInformation = extractorMetadata.m_memberMap[i];
			int keyIdentifierForMemberOffset = m_translator.KeyManager.GetKeyIdentifierForMemberOffset(entityKey, i, keyRowType.Properties.Count);
			object obj = null;
			array[i] = PropagatorResult.CreateKeyValue(value: (!entityKey.IsTemporary) ? entityKey.FindValueByName(edmMember.Name) : stateEntry.StateManager.GetEntityStateEntry(entityKey).CurrentValues[edmMember.Name], flags: memberInformation.Flags, stateEntry: stateEntry, identifier: keyIdentifierForMemberOffset);
		}
		return PropagatorResult.CreateStructuralValue(array, extractorMetadata.m_type, isModified: false);
	}

	private PropagatorResult CreateSimpleResult(IEntityStateEntry stateEntry, IExtendedDataRecord record, MemberInformation memberInformation, int identifier, bool isModified, int recordOrdinal, object value)
	{
		CurrentValueRecord currentValueRecord = record as CurrentValueRecord;
		PropagatorFlags propagatorFlags = memberInformation.Flags;
		if (!isModified)
		{
			propagatorFlags |= PropagatorFlags.Preserve;
		}
		if (-1 != identifier)
		{
			PropagatorResult propagatorResult = (((!memberInformation.IsServerGenerated && !memberInformation.IsForeignKeyMember) || currentValueRecord == null) ? PropagatorResult.CreateKeyValue(propagatorFlags, value, stateEntry, identifier) : PropagatorResult.CreateServerGenKeyValue(propagatorFlags, value, stateEntry, identifier, recordOrdinal));
			m_translator.KeyManager.RegisterIdentifierOwner(propagatorResult);
			return propagatorResult;
		}
		if ((memberInformation.IsServerGenerated || memberInformation.IsForeignKeyMember) && currentValueRecord != null)
		{
			return PropagatorResult.CreateServerGenSimpleValue(propagatorFlags, value, currentValueRecord, recordOrdinal);
		}
		return PropagatorResult.CreateSimpleValue(propagatorFlags, value);
	}

	internal static PropagatorResult ExtractResultFromRecord(IEntityStateEntry stateEntry, bool isModified, IExtendedDataRecord record, bool useCurrentValues, UpdateTranslator translator, ModifiedPropertiesBehavior modifiedPropertiesBehavior)
	{
		StructuralType structuralType = (StructuralType)record.DataRecordInfo.RecordType.EdmType;
		ExtractorMetadata extractorMetadata = translator.GetExtractorMetadata(stateEntry.EntitySet, structuralType);
		EntityKey entityKey = stateEntry.EntityKey;
		PropagatorResult[] array = new PropagatorResult[record.FieldCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = extractorMetadata.RetrieveMember(stateEntry, record, useCurrentValues, entityKey, i, modifiedPropertiesBehavior);
		}
		return PropagatorResult.CreateStructuralValue(array, structuralType, isModified);
	}
}
