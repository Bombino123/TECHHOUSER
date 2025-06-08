using System.Collections;
using System.ComponentModel;
using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Core.EntityClient;

public class EntityDataReader : DbDataReader, IExtendedDataRecord, IDataRecord
{
	private EntityCommand _command;

	private readonly CommandBehavior _behavior;

	private readonly DbDataReader _storeDataReader;

	private readonly IExtendedDataRecord _storeExtendedDataRecord;

	private bool _disposed;

	public override int Depth => _storeDataReader.Depth;

	public override int FieldCount => _storeDataReader.FieldCount;

	public override bool HasRows => _storeDataReader.HasRows;

	public override bool IsClosed => _storeDataReader.IsClosed;

	public override int RecordsAffected => _storeDataReader.RecordsAffected;

	public override object this[int ordinal] => _storeDataReader[ordinal];

	public override object this[string name]
	{
		get
		{
			Check.NotNull(name, "name");
			return _storeDataReader[name];
		}
	}

	public override int VisibleFieldCount => _storeDataReader.VisibleFieldCount;

	public DataRecordInfo DataRecordInfo
	{
		get
		{
			if (_storeExtendedDataRecord == null)
			{
				return null;
			}
			return _storeExtendedDataRecord.DataRecordInfo;
		}
	}

	internal EntityDataReader(EntityCommand command, DbDataReader storeDataReader, CommandBehavior behavior)
	{
		_command = command;
		_storeDataReader = storeDataReader;
		_storeExtendedDataRecord = storeDataReader as IExtendedDataRecord;
		_behavior = behavior;
	}

	internal EntityDataReader()
	{
	}

	public override void Close()
	{
		if (_command != null)
		{
			_storeDataReader.Close();
			_command.NotifyDataReaderClosing();
			if ((_behavior & CommandBehavior.CloseConnection) == CommandBehavior.CloseConnection)
			{
				_command.Connection.Close();
			}
			_command = null;
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (!_disposed && disposing)
		{
			_storeDataReader.Dispose();
		}
		_disposed = true;
		base.Dispose(disposing);
	}

	public override bool GetBoolean(int ordinal)
	{
		return _storeDataReader.GetBoolean(ordinal);
	}

	public override byte GetByte(int ordinal)
	{
		return _storeDataReader.GetByte(ordinal);
	}

	public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
	{
		return _storeDataReader.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);
	}

	public override char GetChar(int ordinal)
	{
		return _storeDataReader.GetChar(ordinal);
	}

	public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
	{
		return _storeDataReader.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);
	}

	public override string GetDataTypeName(int ordinal)
	{
		return _storeDataReader.GetDataTypeName(ordinal);
	}

	public override DateTime GetDateTime(int ordinal)
	{
		return _storeDataReader.GetDateTime(ordinal);
	}

	protected override DbDataReader GetDbDataReader(int ordinal)
	{
		return _storeDataReader.GetData(ordinal);
	}

	public override decimal GetDecimal(int ordinal)
	{
		return _storeDataReader.GetDecimal(ordinal);
	}

	public override double GetDouble(int ordinal)
	{
		return _storeDataReader.GetDouble(ordinal);
	}

	public override Type GetFieldType(int ordinal)
	{
		return _storeDataReader.GetFieldType(ordinal);
	}

	public override float GetFloat(int ordinal)
	{
		return _storeDataReader.GetFloat(ordinal);
	}

	public override Guid GetGuid(int ordinal)
	{
		return _storeDataReader.GetGuid(ordinal);
	}

	public override short GetInt16(int ordinal)
	{
		return _storeDataReader.GetInt16(ordinal);
	}

	public override int GetInt32(int ordinal)
	{
		return _storeDataReader.GetInt32(ordinal);
	}

	public override long GetInt64(int ordinal)
	{
		return _storeDataReader.GetInt64(ordinal);
	}

	public override string GetName(int ordinal)
	{
		return _storeDataReader.GetName(ordinal);
	}

	public override int GetOrdinal(string name)
	{
		Check.NotNull(name, "name");
		return _storeDataReader.GetOrdinal(name);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override Type GetProviderSpecificFieldType(int ordinal)
	{
		return _storeDataReader.GetProviderSpecificFieldType(ordinal);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override object GetProviderSpecificValue(int ordinal)
	{
		return _storeDataReader.GetProviderSpecificValue(ordinal);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override int GetProviderSpecificValues(object[] values)
	{
		return _storeDataReader.GetProviderSpecificValues(values);
	}

	public override DataTable GetSchemaTable()
	{
		return _storeDataReader.GetSchemaTable();
	}

	public override string GetString(int ordinal)
	{
		return _storeDataReader.GetString(ordinal);
	}

	public override object GetValue(int ordinal)
	{
		return _storeDataReader.GetValue(ordinal);
	}

	public override int GetValues(object[] values)
	{
		return _storeDataReader.GetValues(values);
	}

	public override bool IsDBNull(int ordinal)
	{
		return _storeDataReader.IsDBNull(ordinal);
	}

	public override bool NextResult()
	{
		try
		{
			return _storeDataReader.NextResult();
		}
		catch (Exception innerException)
		{
			throw new EntityCommandExecutionException(Strings.EntityClient_StoreReaderFailed, innerException);
		}
	}

	public override async Task<bool> NextResultAsync(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		try
		{
			return await _storeDataReader.NextResultAsync(cancellationToken).WithCurrentCulture();
		}
		catch (Exception innerException)
		{
			throw new EntityCommandExecutionException(Strings.EntityClient_StoreReaderFailed, innerException);
		}
	}

	public override bool Read()
	{
		return _storeDataReader.Read();
	}

	public override Task<bool> ReadAsync(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		return _storeDataReader.ReadAsync(cancellationToken);
	}

	public override IEnumerator GetEnumerator()
	{
		return _storeDataReader.GetEnumerator();
	}

	public DbDataRecord GetDataRecord(int i)
	{
		if (_storeExtendedDataRecord == null)
		{
			throw new ArgumentOutOfRangeException("i");
		}
		return _storeExtendedDataRecord.GetDataRecord(i);
	}

	public DbDataReader GetDataReader(int i)
	{
		return GetDbDataReader(i);
	}
}
