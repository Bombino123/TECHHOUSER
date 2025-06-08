using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.Internal.Materialization;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Core.Query.ResultAssembly;

internal sealed class BridgeDataRecord : DbDataRecord, IExtendedDataRecord, IDataRecord
{
	private enum Status
	{
		Open,
		ClosedImplicitly,
		ClosedExplicitly
	}

	internal readonly int Depth;

	private readonly Shaper<RecordState> _shaper;

	private RecordState _source;

	private Status _status;

	private int _lastColumnRead;

	private long _lastDataOffsetRead;

	private int _lastOrdinalCheckedForNull;

	private object _lastValueCheckedForNull;

	private BridgeDataReader _currentNestedReader;

	private BridgeDataRecord _currentNestedRecord;

	internal bool HasData => _source != null;

	internal bool IsClosed => _status != Status.Open;

	internal bool IsExplicitlyClosed => _status == Status.ClosedExplicitly;

	internal bool IsImplicitlyClosed => _status == Status.ClosedImplicitly;

	public DataRecordInfo DataRecordInfo
	{
		get
		{
			AssertReaderIsOpen();
			return _source.DataRecordInfo;
		}
	}

	public override int FieldCount
	{
		get
		{
			AssertReaderIsOpen();
			return _source.ColumnCount;
		}
	}

	public override object this[int ordinal] => GetValue(ordinal);

	public override object this[string name] => GetValue(GetOrdinal(name));

	internal BridgeDataRecord(Shaper<RecordState> shaper, int depth)
	{
		_shaper = shaper;
		Depth = depth;
	}

	internal void CloseExplicitly()
	{
		Close(Status.ClosedExplicitly, CloseNestedObjectImplicitly);
	}

	internal Task CloseExplicitlyAsync(CancellationToken cancellationToken)
	{
		return Close(Status.ClosedExplicitly, () => CloseNestedObjectImplicitlyAsync(cancellationToken));
	}

	internal void CloseImplicitly()
	{
		Close(Status.ClosedImplicitly, CloseNestedObjectImplicitly);
	}

	internal Task CloseImplicitlyAsync(CancellationToken cancellationToken)
	{
		return Close(Status.ClosedImplicitly, () => CloseNestedObjectImplicitlyAsync(cancellationToken));
	}

	private T Close<T>(Status status, Func<T> close)
	{
		_status = status;
		_source = null;
		return close();
	}

	private object CloseNestedObjectImplicitly()
	{
		BridgeDataRecord currentNestedRecord = _currentNestedRecord;
		if (currentNestedRecord != null)
		{
			_currentNestedRecord = null;
			currentNestedRecord.CloseImplicitly();
		}
		BridgeDataReader currentNestedReader = _currentNestedReader;
		if (currentNestedReader != null)
		{
			_currentNestedReader = null;
			currentNestedReader.CloseImplicitly();
		}
		return null;
	}

	private async Task CloseNestedObjectImplicitlyAsync(CancellationToken cancellationToken)
	{
		BridgeDataRecord currentNestedRecord = _currentNestedRecord;
		if (currentNestedRecord != null)
		{
			_currentNestedRecord = null;
			await currentNestedRecord.CloseImplicitlyAsync(cancellationToken).WithCurrentCulture();
		}
		BridgeDataReader currentNestedReader = _currentNestedReader;
		if (currentNestedReader != null)
		{
			_currentNestedReader = null;
			await currentNestedReader.CloseImplicitlyAsync(cancellationToken).WithCurrentCulture();
		}
	}

	internal void SetRecordSource(RecordState newSource, bool hasData)
	{
		if (hasData)
		{
			_source = newSource;
		}
		else
		{
			_source = null;
		}
		_status = Status.Open;
		_lastColumnRead = -1;
		_lastDataOffsetRead = -1L;
		_lastOrdinalCheckedForNull = -1;
		_lastValueCheckedForNull = null;
	}

	private void AssertReaderIsOpen()
	{
		if (IsExplicitlyClosed)
		{
			throw Error.ADP_ClosedDataReaderError();
		}
		if (IsImplicitlyClosed)
		{
			throw Error.ADP_ImplicitlyClosedDataReaderError();
		}
	}

	private void AssertReaderIsOpenWithData()
	{
		AssertReaderIsOpen();
		if (!HasData)
		{
			throw Error.ADP_NoData();
		}
	}

	private void AssertSequentialAccess(int ordinal)
	{
		if (ordinal < 0 || ordinal >= _source.ColumnCount)
		{
			throw new ArgumentOutOfRangeException("ordinal");
		}
		if (_lastColumnRead >= ordinal)
		{
			throw new InvalidOperationException(Strings.ADP_NonSequentialColumnAccess(ordinal.ToString(CultureInfo.InvariantCulture), (_lastColumnRead + 1).ToString(CultureInfo.InvariantCulture)));
		}
		_lastColumnRead = ordinal;
		_lastDataOffsetRead = long.MaxValue;
	}

	private void AssertSequentialAccess(int ordinal, long dataOffset, string methodName)
	{
		if (ordinal < 0 || ordinal >= _source.ColumnCount)
		{
			throw new ArgumentOutOfRangeException("ordinal");
		}
		if (_lastColumnRead > ordinal || (_lastColumnRead == ordinal && _lastDataOffsetRead == long.MaxValue))
		{
			throw new InvalidOperationException(Strings.ADP_NonSequentialColumnAccess(ordinal.ToString(CultureInfo.InvariantCulture), (_lastColumnRead + 1).ToString(CultureInfo.InvariantCulture)));
		}
		if (_lastColumnRead == ordinal)
		{
			if (_lastDataOffsetRead >= dataOffset)
			{
				throw new InvalidOperationException(Strings.ADP_NonSequentialChunkAccess(dataOffset.ToString(CultureInfo.InvariantCulture), (_lastDataOffsetRead + 1).ToString(CultureInfo.InvariantCulture), methodName));
			}
		}
		else
		{
			_lastColumnRead = ordinal;
			_lastDataOffsetRead = -1L;
		}
	}

	private TypeUsage GetTypeUsage(int ordinal)
	{
		if (ordinal < 0 || ordinal >= _source.ColumnCount)
		{
			throw new ArgumentOutOfRangeException("ordinal");
		}
		if (_source.CurrentColumnValues[ordinal] is RecordState recordState)
		{
			return recordState.DataRecordInfo.RecordType;
		}
		return _source.GetTypeUsage(ordinal);
	}

	public override string GetDataTypeName(int ordinal)
	{
		AssertReaderIsOpenWithData();
		return GetTypeUsage(ordinal).ToString();
	}

	public override Type GetFieldType(int ordinal)
	{
		AssertReaderIsOpenWithData();
		return BridgeDataReader.GetClrTypeFromTypeMetadata(GetTypeUsage(ordinal));
	}

	public override string GetName(int ordinal)
	{
		AssertReaderIsOpen();
		return _source.GetName(ordinal);
	}

	public override int GetOrdinal(string name)
	{
		AssertReaderIsOpen();
		return _source.GetOrdinal(name);
	}

	public override object GetValue(int ordinal)
	{
		AssertReaderIsOpenWithData();
		AssertSequentialAccess(ordinal);
		object obj = null;
		if (ordinal == _lastOrdinalCheckedForNull)
		{
			obj = _lastValueCheckedForNull;
		}
		else
		{
			_lastOrdinalCheckedForNull = -1;
			_lastValueCheckedForNull = null;
			CloseNestedObjectImplicitly();
			obj = _source.CurrentColumnValues[ordinal];
			if (_source.IsNestedObject(ordinal))
			{
				obj = GetNestedObjectValue(obj);
			}
		}
		return obj;
	}

	private object GetNestedObjectValue(object result)
	{
		if (result != DBNull.Value)
		{
			if (result is RecordState recordState)
			{
				if (recordState.IsNull)
				{
					result = DBNull.Value;
				}
				else
				{
					BridgeDataRecord bridgeDataRecord = new BridgeDataRecord(_shaper, Depth + 1);
					bridgeDataRecord.SetRecordSource(recordState, hasData: true);
					result = bridgeDataRecord;
					_currentNestedRecord = bridgeDataRecord;
					_currentNestedReader = null;
				}
			}
			else if (result is Coordinator<RecordState> coordinator)
			{
				BridgeDataReader bridgeDataReader = new BridgeDataReader(_shaper, coordinator.TypedCoordinatorFactory, Depth + 1, null);
				result = bridgeDataReader;
				_currentNestedRecord = null;
				_currentNestedReader = bridgeDataReader;
			}
		}
		return result;
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

	public override bool GetBoolean(int ordinal)
	{
		return (bool)GetValue(ordinal);
	}

	public override byte GetByte(int ordinal)
	{
		return (byte)GetValue(ordinal);
	}

	public override char GetChar(int ordinal)
	{
		return (char)GetValue(ordinal);
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

	public override string GetString(int ordinal)
	{
		return (string)GetValue(ordinal);
	}

	public override bool IsDBNull(int ordinal)
	{
		object value = GetValue(ordinal);
		_lastColumnRead--;
		_lastDataOffsetRead = -1L;
		_lastValueCheckedForNull = value;
		_lastOrdinalCheckedForNull = ordinal;
		return DBNull.Value == value;
	}

	public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
	{
		AssertReaderIsOpenWithData();
		AssertSequentialAccess(ordinal, dataOffset, "GetBytes");
		long bytes = _source.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);
		if (buffer != null)
		{
			_lastDataOffsetRead = dataOffset + bytes - 1;
		}
		return bytes;
	}

	public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
	{
		AssertReaderIsOpenWithData();
		AssertSequentialAccess(ordinal, dataOffset, "GetChars");
		long chars = _source.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);
		if (buffer != null)
		{
			_lastDataOffsetRead = dataOffset + chars - 1;
		}
		return chars;
	}

	protected override DbDataReader GetDbDataReader(int ordinal)
	{
		return (DbDataReader)GetValue(ordinal);
	}

	public DbDataRecord GetDataRecord(int ordinal)
	{
		return (DbDataRecord)GetValue(ordinal);
	}

	public DbDataReader GetDataReader(int ordinal)
	{
		return GetDbDataReader(ordinal);
	}
}
