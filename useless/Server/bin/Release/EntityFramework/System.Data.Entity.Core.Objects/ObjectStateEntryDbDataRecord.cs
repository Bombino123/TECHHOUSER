using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Globalization;

namespace System.Data.Entity.Core.Objects;

internal sealed class ObjectStateEntryDbDataRecord : DbDataRecord, IExtendedDataRecord, IDataRecord
{
	private readonly StateManagerTypeMetadata _metadata;

	private readonly ObjectStateEntry _cacheEntry;

	private readonly object _userObject;

	private DataRecordInfo _recordInfo;

	public override int FieldCount => _cacheEntry.GetFieldCount(_metadata);

	public override object this[int ordinal] => GetValue(ordinal);

	public override object this[string name] => GetValue(GetOrdinal(name));

	public DataRecordInfo DataRecordInfo
	{
		get
		{
			if (_recordInfo == null)
			{
				_recordInfo = _cacheEntry.GetDataRecordInfo(_metadata, _userObject);
			}
			return _recordInfo;
		}
	}

	internal ObjectStateEntryDbDataRecord(EntityEntry cacheEntry, StateManagerTypeMetadata metadata, object userObject)
	{
		EntityState state = cacheEntry.State;
		if (state == EntityState.Unchanged || state == EntityState.Deleted || state == EntityState.Modified)
		{
			_cacheEntry = cacheEntry;
			_userObject = userObject;
			_metadata = metadata;
		}
	}

	internal ObjectStateEntryDbDataRecord(RelationshipEntry cacheEntry)
	{
		EntityState state = cacheEntry.State;
		if (state == EntityState.Unchanged || state == EntityState.Deleted || state == EntityState.Modified)
		{
			_cacheEntry = cacheEntry;
		}
	}

	public override bool GetBoolean(int ordinal)
	{
		return (bool)GetValue(ordinal);
	}

	public override byte GetByte(int ordinal)
	{
		return (byte)GetValue(ordinal);
	}

	public override long GetBytes(int ordinal, long dataIndex, byte[] buffer, int bufferIndex, int length)
	{
		byte[] array = (byte[])GetValue(ordinal);
		if (buffer == null)
		{
			return array.Length;
		}
		int num = (int)dataIndex;
		int num2 = Math.Min(array.Length - num, length);
		if (num < 0)
		{
			throw new ArgumentOutOfRangeException("dataIndex", Strings.ADP_InvalidSourceBufferIndex(array.Length.ToString(CultureInfo.InvariantCulture), ((long)num).ToString(CultureInfo.InvariantCulture)));
		}
		if (bufferIndex < 0 || (bufferIndex > 0 && bufferIndex >= buffer.Length))
		{
			throw new ArgumentOutOfRangeException("bufferIndex", Strings.ADP_InvalidDestinationBufferIndex(buffer.Length.ToString(CultureInfo.InvariantCulture), bufferIndex.ToString(CultureInfo.InvariantCulture)));
		}
		if (0 < num2)
		{
			Array.Copy(array, dataIndex, buffer, bufferIndex, num2);
		}
		else
		{
			if (length < 0)
			{
				throw new IndexOutOfRangeException(Strings.ADP_InvalidDataLength(((long)length).ToString(CultureInfo.InvariantCulture)));
			}
			num2 = 0;
		}
		return num2;
	}

	public override char GetChar(int ordinal)
	{
		return (char)GetValue(ordinal);
	}

	public override long GetChars(int ordinal, long dataIndex, char[] buffer, int bufferIndex, int length)
	{
		char[] array = (char[])GetValue(ordinal);
		if (buffer == null)
		{
			return array.Length;
		}
		int num = (int)dataIndex;
		int num2 = Math.Min(array.Length - num, length);
		if (num < 0)
		{
			throw new ArgumentOutOfRangeException("bufferIndex", Strings.ADP_InvalidSourceBufferIndex(buffer.Length.ToString(CultureInfo.InvariantCulture), ((long)bufferIndex).ToString(CultureInfo.InvariantCulture)));
		}
		if (bufferIndex < 0 || (bufferIndex > 0 && bufferIndex >= buffer.Length))
		{
			throw new ArgumentOutOfRangeException("bufferIndex", Strings.ADP_InvalidDestinationBufferIndex(buffer.Length.ToString(CultureInfo.InvariantCulture), bufferIndex.ToString(CultureInfo.InvariantCulture)));
		}
		if (0 < num2)
		{
			Array.Copy(array, dataIndex, buffer, bufferIndex, num2);
		}
		else
		{
			if (length < 0)
			{
				throw new IndexOutOfRangeException(Strings.ADP_InvalidDataLength(((long)length).ToString(CultureInfo.InvariantCulture)));
			}
			num2 = 0;
		}
		return num2;
	}

	protected override DbDataReader GetDbDataReader(int ordinal)
	{
		throw new NotSupportedException();
	}

	public override string GetDataTypeName(int ordinal)
	{
		return GetFieldType(ordinal).Name;
	}

	public override DateTime GetDateTime(int ordinal)
	{
		return (DateTime)GetValue(ordinal);
	}

	public override decimal GetDecimal(int ordinal)
	{
		return (decimal)GetValue(ordinal);
	}

	public override double GetDouble(int ordinal)
	{
		return (double)GetValue(ordinal);
	}

	public override Type GetFieldType(int ordinal)
	{
		return _cacheEntry.GetFieldType(ordinal, _metadata);
	}

	public override float GetFloat(int ordinal)
	{
		return (float)GetValue(ordinal);
	}

	public override Guid GetGuid(int ordinal)
	{
		return (Guid)GetValue(ordinal);
	}

	public override short GetInt16(int ordinal)
	{
		return (short)GetValue(ordinal);
	}

	public override int GetInt32(int ordinal)
	{
		return (int)GetValue(ordinal);
	}

	public override long GetInt64(int ordinal)
	{
		return (long)GetValue(ordinal);
	}

	public override string GetName(int ordinal)
	{
		return _cacheEntry.GetCLayerName(ordinal, _metadata);
	}

	public override int GetOrdinal(string name)
	{
		int ordinalforCLayerName = _cacheEntry.GetOrdinalforCLayerName(name, _metadata);
		if (ordinalforCLayerName == -1)
		{
			throw new ArgumentOutOfRangeException("name");
		}
		return ordinalforCLayerName;
	}

	public override string GetString(int ordinal)
	{
		return (string)GetValue(ordinal);
	}

	public override object GetValue(int ordinal)
	{
		if (_cacheEntry.IsRelationship)
		{
			return (_cacheEntry as RelationshipEntry).GetOriginalRelationValue(ordinal);
		}
		return (_cacheEntry as EntityEntry).GetOriginalEntityValue(_metadata, ordinal, _userObject, ObjectStateValueRecord.OriginalReadonly);
	}

	public override int GetValues(object[] values)
	{
		Check.NotNull(values, "values");
		int num = Math.Min(values.Length, FieldCount);
		for (int i = 0; i < num; i++)
		{
			values[i] = GetValue(i);
		}
		return num;
	}

	public override bool IsDBNull(int ordinal)
	{
		return GetValue(ordinal) == DBNull.Value;
	}

	public DbDataRecord GetDataRecord(int ordinal)
	{
		return (DbDataRecord)GetValue(ordinal);
	}

	public DbDataReader GetDataReader(int i)
	{
		return GetDbDataReader(i);
	}
}
