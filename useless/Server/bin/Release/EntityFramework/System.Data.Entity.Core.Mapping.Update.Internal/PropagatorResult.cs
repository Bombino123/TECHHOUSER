using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Linq;

namespace System.Data.Entity.Core.Mapping.Update.Internal;

internal abstract class PropagatorResult
{
	private class SimpleValue : PropagatorResult
	{
		private readonly PropagatorFlags m_flags;

		protected readonly object m_value;

		internal override PropagatorFlags PropagatorFlags => m_flags;

		internal override bool IsSimple => true;

		internal override bool IsNull
		{
			get
			{
				if (-1 == Identifier)
				{
					return DBNull.Value == m_value;
				}
				return false;
			}
		}

		internal SimpleValue(PropagatorFlags flags, object value)
		{
			m_flags = flags;
			m_value = value ?? DBNull.Value;
		}

		internal override object GetSimpleValue()
		{
			return m_value;
		}

		internal override PropagatorResult ReplicateResultWithNewFlags(PropagatorFlags flags)
		{
			return new SimpleValue(flags, m_value);
		}

		internal override PropagatorResult ReplicateResultWithNewValue(object value)
		{
			return new SimpleValue(PropagatorFlags, value);
		}

		internal override PropagatorResult Replace(Func<PropagatorResult, PropagatorResult> map)
		{
			return map(this);
		}
	}

	private class ServerGenSimpleValue : SimpleValue
	{
		private readonly CurrentValueRecord m_record;

		private readonly int m_recordOrdinal;

		internal override CurrentValueRecord Record => m_record;

		internal override int RecordOrdinal => m_recordOrdinal;

		internal ServerGenSimpleValue(PropagatorFlags flags, object value, CurrentValueRecord record, int recordOrdinal)
			: base(flags, value)
		{
			m_record = record;
			m_recordOrdinal = recordOrdinal;
		}

		internal override PropagatorResult ReplicateResultWithNewFlags(PropagatorFlags flags)
		{
			return new ServerGenSimpleValue(flags, m_value, Record, RecordOrdinal);
		}

		internal override PropagatorResult ReplicateResultWithNewValue(object value)
		{
			return new ServerGenSimpleValue(PropagatorFlags, value, Record, RecordOrdinal);
		}
	}

	private class KeyValue : SimpleValue
	{
		private readonly IEntityStateEntry m_stateEntry;

		private readonly int m_identifier;

		protected readonly KeyValue m_next;

		internal override IEntityStateEntry StateEntry => m_stateEntry;

		internal override int Identifier => m_identifier;

		internal override CurrentValueRecord Record => m_stateEntry.CurrentValues;

		internal override PropagatorResult Next => m_next;

		internal KeyValue(PropagatorFlags flags, object value, IEntityStateEntry stateEntry, int identifier, KeyValue next)
			: base(flags, value)
		{
			m_stateEntry = stateEntry;
			m_identifier = identifier;
			m_next = next;
		}

		internal override PropagatorResult ReplicateResultWithNewFlags(PropagatorFlags flags)
		{
			return new KeyValue(flags, m_value, StateEntry, Identifier, m_next);
		}

		internal override PropagatorResult ReplicateResultWithNewValue(object value)
		{
			return new KeyValue(PropagatorFlags, value, StateEntry, Identifier, m_next);
		}

		internal virtual KeyValue ReplicateResultWithNewNext(KeyValue next)
		{
			if (m_next != null)
			{
				next = m_next.ReplicateResultWithNewNext(next);
			}
			return new KeyValue(PropagatorFlags, m_value, m_stateEntry, m_identifier, next);
		}

		internal override PropagatorResult Merge(KeyManager keyManager, PropagatorResult other)
		{
			KeyValue keyValue = other as KeyValue;
			if (keyValue == null)
			{
				EntityUtil.InternalError(EntityUtil.InternalErrorCode.UpdatePipelineResultRequestInvalid, 0, "KeyValue.Merge");
			}
			if (Identifier != keyValue.Identifier)
			{
				if (keyManager.GetPrincipals(keyValue.Identifier).Contains(Identifier))
				{
					return ReplicateResultWithNewNext(keyValue);
				}
				return keyValue.ReplicateResultWithNewNext(this);
			}
			if (m_stateEntry == null || m_stateEntry.IsRelationship)
			{
				return keyValue.ReplicateResultWithNewNext(this);
			}
			return ReplicateResultWithNewNext(keyValue);
		}
	}

	private class ServerGenKeyValue : KeyValue
	{
		private readonly int m_recordOrdinal;

		internal override int RecordOrdinal => m_recordOrdinal;

		internal ServerGenKeyValue(PropagatorFlags flags, object value, IEntityStateEntry stateEntry, int identifier, int recordOrdinal, KeyValue next)
			: base(flags, value, stateEntry, identifier, next)
		{
			m_recordOrdinal = recordOrdinal;
		}

		internal override PropagatorResult ReplicateResultWithNewFlags(PropagatorFlags flags)
		{
			return new ServerGenKeyValue(flags, m_value, StateEntry, Identifier, RecordOrdinal, m_next);
		}

		internal override PropagatorResult ReplicateResultWithNewValue(object value)
		{
			return new ServerGenKeyValue(PropagatorFlags, value, StateEntry, Identifier, RecordOrdinal, m_next);
		}

		internal override KeyValue ReplicateResultWithNewNext(KeyValue next)
		{
			if (m_next != null)
			{
				next = m_next.ReplicateResultWithNewNext(next);
			}
			return new ServerGenKeyValue(PropagatorFlags, m_value, StateEntry, Identifier, RecordOrdinal, next);
		}
	}

	private class StructuralValue : PropagatorResult
	{
		private readonly PropagatorResult[] m_values;

		protected readonly StructuralType m_structuralType;

		internal override bool IsSimple => false;

		internal override bool IsNull => false;

		internal override StructuralType StructuralType => m_structuralType;

		internal StructuralValue(PropagatorResult[] values, StructuralType structuralType)
		{
			m_values = values;
			m_structuralType = structuralType;
		}

		internal override PropagatorResult GetMemberValue(int ordinal)
		{
			return m_values[ordinal];
		}

		internal override PropagatorResult[] GetMemberValues()
		{
			return m_values;
		}

		internal override PropagatorResult ReplicateResultWithNewFlags(PropagatorFlags flags)
		{
			throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.UpdatePipelineResultRequestInvalid, 0, "StructuralValue.ReplicateResultWithNewFlags");
		}

		internal override PropagatorResult Replace(Func<PropagatorResult, PropagatorResult> map)
		{
			PropagatorResult[] array = ReplaceValues(map);
			if (array != null)
			{
				return new StructuralValue(array, m_structuralType);
			}
			return this;
		}

		protected PropagatorResult[] ReplaceValues(Func<PropagatorResult, PropagatorResult> map)
		{
			PropagatorResult[] array = new PropagatorResult[m_values.Length];
			bool flag = false;
			for (int i = 0; i < array.Length; i++)
			{
				PropagatorResult propagatorResult = m_values[i].Replace(map);
				if (propagatorResult != m_values[i])
				{
					flag = true;
				}
				array[i] = propagatorResult;
			}
			if (!flag)
			{
				return null;
			}
			return array;
		}
	}

	private class UnmodifiedStructuralValue : StructuralValue
	{
		internal override PropagatorFlags PropagatorFlags => PropagatorFlags.Preserve;

		internal UnmodifiedStructuralValue(PropagatorResult[] values, StructuralType structuralType)
			: base(values, structuralType)
		{
		}

		internal override PropagatorResult Replace(Func<PropagatorResult, PropagatorResult> map)
		{
			PropagatorResult[] array = ReplaceValues(map);
			if (array != null)
			{
				return new UnmodifiedStructuralValue(array, m_structuralType);
			}
			return this;
		}
	}

	internal const int NullIdentifier = -1;

	internal const int NullOrdinal = -1;

	internal abstract bool IsNull { get; }

	internal abstract bool IsSimple { get; }

	internal virtual PropagatorFlags PropagatorFlags => PropagatorFlags.NoFlags;

	internal virtual IEntityStateEntry StateEntry => null;

	internal virtual CurrentValueRecord Record => null;

	internal virtual StructuralType StructuralType => null;

	internal virtual int RecordOrdinal => -1;

	internal virtual int Identifier => -1;

	internal virtual PropagatorResult Next => null;

	internal virtual object GetSimpleValue()
	{
		throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.UpdatePipelineResultRequestInvalid, 0, "PropagatorResult.GetSimpleValue");
	}

	internal virtual PropagatorResult GetMemberValue(int ordinal)
	{
		throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.UpdatePipelineResultRequestInvalid, 0, "PropagatorResult.GetMemberValue");
	}

	internal PropagatorResult GetMemberValue(EdmMember member)
	{
		int ordinal = TypeHelpers.GetAllStructuralMembers(StructuralType).IndexOf(member);
		return GetMemberValue(ordinal);
	}

	internal virtual PropagatorResult[] GetMemberValues()
	{
		throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.UpdatePipelineResultRequestInvalid, 0, "PropagatorResult.GetMembersValues");
	}

	internal abstract PropagatorResult ReplicateResultWithNewFlags(PropagatorFlags flags);

	internal virtual PropagatorResult ReplicateResultWithNewValue(object value)
	{
		throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.UpdatePipelineResultRequestInvalid, 0, "PropagatorResult.ReplicateResultWithNewValue");
	}

	internal abstract PropagatorResult Replace(Func<PropagatorResult, PropagatorResult> map);

	internal virtual PropagatorResult Merge(KeyManager keyManager, PropagatorResult other)
	{
		throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.UpdatePipelineResultRequestInvalid, 0, "PropagatorResult.Merge");
	}

	internal virtual void SetServerGenValue(object value)
	{
		if (RecordOrdinal != -1)
		{
			CurrentValueRecord record = Record;
			EdmMember fieldType = ((IExtendedDataRecord)record).DataRecordInfo.FieldMetadata[RecordOrdinal].FieldType;
			value = value ?? DBNull.Value;
			value = AlignReturnValue(value, fieldType);
			record.SetValue(RecordOrdinal, value);
		}
	}

	internal object AlignReturnValue(object value, EdmMember member)
	{
		if (DBNull.Value.Equals(value))
		{
			if (BuiltInTypeKind.EdmProperty == member.BuiltInTypeKind && !((EdmProperty)member).Nullable)
			{
				throw EntityUtil.Update(Strings.Update_NullReturnValueForNonNullableMember(member.Name, member.DeclaringType.FullName), null);
			}
		}
		else if (!Helper.IsSpatialType(member.TypeUsage))
		{
			Type type = null;
			Type clrEquivalentType;
			if (Helper.IsEnumType(member.TypeUsage.EdmType))
			{
				PrimitiveType primitiveType = Helper.AsPrimitive(member.TypeUsage.EdmType);
				type = Record.GetFieldType(RecordOrdinal);
				clrEquivalentType = primitiveType.ClrEquivalentType;
			}
			else
			{
				clrEquivalentType = ((PrimitiveType)member.TypeUsage.EdmType).ClrEquivalentType;
			}
			try
			{
				value = Convert.ChangeType(value, clrEquivalentType, CultureInfo.InvariantCulture);
				if (type != null)
				{
					value = Enum.ToObject(type, value);
				}
			}
			catch (Exception ex)
			{
				if (ex.RequiresContext())
				{
					Type type2 = type ?? clrEquivalentType;
					throw EntityUtil.Update(Strings.Update_ReturnValueHasUnexpectedType(value.GetType().FullName, type2.FullName, member.Name, member.DeclaringType.FullName), ex);
				}
				throw;
			}
		}
		return value;
	}

	internal static PropagatorResult CreateSimpleValue(PropagatorFlags flags, object value)
	{
		return new SimpleValue(flags, value);
	}

	internal static PropagatorResult CreateServerGenSimpleValue(PropagatorFlags flags, object value, CurrentValueRecord record, int recordOrdinal)
	{
		return new ServerGenSimpleValue(flags, value, record, recordOrdinal);
	}

	internal static PropagatorResult CreateKeyValue(PropagatorFlags flags, object value, IEntityStateEntry stateEntry, int identifier)
	{
		return new KeyValue(flags, value, stateEntry, identifier, null);
	}

	internal static PropagatorResult CreateServerGenKeyValue(PropagatorFlags flags, object value, IEntityStateEntry stateEntry, int identifier, int recordOrdinal)
	{
		return new ServerGenKeyValue(flags, value, stateEntry, identifier, recordOrdinal, null);
	}

	internal static PropagatorResult CreateStructuralValue(PropagatorResult[] values, StructuralType structuralType, bool isModified)
	{
		if (isModified)
		{
			return new StructuralValue(values, structuralType);
		}
		return new UnmodifiedStructuralValue(values, structuralType);
	}
}
