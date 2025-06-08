using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Globalization;

namespace System.Data.Entity.Core.Objects;

public abstract class DbUpdatableDataRecord : DbDataRecord, IExtendedDataRecord, IDataRecord
{
	internal readonly StateManagerTypeMetadata _metadata;

	internal readonly ObjectStateEntry _cacheEntry;

	internal readonly object _userObject;

	internal DataRecordInfo _recordInfo;

	public override int FieldCount => _cacheEntry.GetFieldCount(_metadata);

	public override object this[int i] => GetValue(i);

	public override object this[string name] => GetValue(GetOrdinal(name));

	public virtual DataRecordInfo DataRecordInfo
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

	internal DbUpdatableDataRecord(ObjectStateEntry cacheEntry, StateManagerTypeMetadata metadata, object userObject)
	{
		_cacheEntry = cacheEntry;
		_userObject = userObject;
		_metadata = metadata;
	}

	internal DbUpdatableDataRecord(ObjectStateEntry cacheEntry)
		: this(cacheEntry, null, null)
	{
	}

	public override bool GetBoolean(int i)
	{
		return (bool)GetValue(i);
	}

	public override byte GetByte(int i)
	{
		return (byte)GetValue(i);
	}

	public override long GetBytes(int i, long dataIndex, byte[] buffer, int bufferIndex, int length)
	{
		byte[] array = (byte[])GetValue(i);
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

	public override char GetChar(int i)
	{
		return (char)GetValue(i);
	}

	public override long GetChars(int i, long dataIndex, char[] buffer, int bufferIndex, int length)
	{
		char[] array = (char[])GetValue(i);
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

	IDataReader IDataRecord.GetData(int ordinal)
	{
		return GetDbDataReader(ordinal);
	}

	protected override DbDataReader GetDbDataReader(int i)
	{
		throw new NotSupportedException();
	}

	public override string GetDataTypeName(int i)
	{
		return GetFieldType(i).Name;
	}

	public override DateTime GetDateTime(int i)
	{
		return (DateTime)GetValue(i);
	}

	public override decimal GetDecimal(int i)
	{
		return (decimal)GetValue(i);
	}

	public override double GetDouble(int i)
	{
		return (double)GetValue(i);
	}

	public override Type GetFieldType(int i)
	{
		return _cacheEntry.GetFieldType(i, _metadata);
	}

	public override float GetFloat(int i)
	{
		return (float)GetValue(i);
	}

	public override Guid GetGuid(int i)
	{
		return (Guid)GetValue(i);
	}

	public override short GetInt16(int i)
	{
		return (short)GetValue(i);
	}

	public override int GetInt32(int i)
	{
		return (int)GetValue(i);
	}

	public override long GetInt64(int i)
	{
		return (long)GetValue(i);
	}

	public override string GetName(int i)
	{
		return _cacheEntry.GetCLayerName(i, _metadata);
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

	public override string GetString(int i)
	{
		return (string)GetValue(i);
	}

	public override object GetValue(int i)
	{
		return GetRecordValue(i);
	}

	protected abstract object GetRecordValue(int ordinal);

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

	public override bool IsDBNull(int i)
	{
		return GetValue(i) == DBNull.Value;
	}

	public void SetBoolean(int ordinal, bool value)
	{
		SetValue(ordinal, value);
	}

	public void SetByte(int ordinal, byte value)
	{
		SetValue(ordinal, value);
	}

	public void SetChar(int ordinal, char value)
	{
		SetValue(ordinal, value);
	}

	public void SetDataRecord(int ordinal, IDataRecord value)
	{
		SetValue(ordinal, value);
	}

	public void SetDateTime(int ordinal, DateTime value)
	{
		SetValue(ordinal, value);
	}

	public void SetDecimal(int ordinal, decimal value)
	{
		SetValue(ordinal, value);
	}

	public void SetDouble(int ordinal, double value)
	{
		SetValue(ordinal, value);
	}

	public void SetFloat(int ordinal, float value)
	{
		SetValue(ordinal, value);
	}

	public void SetGuid(int ordinal, Guid value)
	{
		SetValue(ordinal, value);
	}

	public void SetInt16(int ordinal, short value)
	{
		SetValue(ordinal, value);
	}

	public void SetInt32(int ordinal, int value)
	{
		SetValue(ordinal, value);
	}

	public void SetInt64(int ordinal, long value)
	{
		SetValue(ordinal, value);
	}

	public void SetString(int ordinal, string value)
	{
		SetValue(ordinal, value);
	}

	public void SetValue(int ordinal, object value)
	{
		SetRecordValue(ordinal, value);
	}

	public int SetValues(params object[] values)
	{
		int num = Math.Min(values.Length, FieldCount);
		for (int i = 0; i < num; i++)
		{
			SetRecordValue(i, values[i]);
		}
		return num;
	}

	public void SetDBNull(int ordinal)
	{
		SetRecordValue(ordinal, DBNull.Value);
	}

	public DbDataRecord GetDataRecord(int i)
	{
		return (DbDataRecord)GetValue(i);
	}

	public DbDataReader GetDataReader(int i)
	{
		return GetDbDataReader(i);
	}

	protected abstract void SetRecordValue(int ordinal, object value);
}
