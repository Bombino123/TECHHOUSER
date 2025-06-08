using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Core.Objects.Internal;

internal class BufferedDataReader : DbDataReader
{
	private DbDataReader _underlyingReader;

	private List<BufferedDataRecord> _bufferedDataRecords = new List<BufferedDataRecord>();

	private BufferedDataRecord _currentResultSet;

	private int _currentResultSetNumber;

	private int _recordsAffected;

	private bool _disposed;

	private bool _isClosed;

	public override int RecordsAffected => _recordsAffected;

	public override object this[string name]
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public override object this[int ordinal]
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public override int Depth
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public override int FieldCount
	{
		get
		{
			AssertReaderIsOpen();
			return _currentResultSet.FieldCount;
		}
	}

	public override bool HasRows
	{
		get
		{
			AssertReaderIsOpen();
			return _currentResultSet.HasRows;
		}
	}

	public override bool IsClosed => _isClosed;

	public BufferedDataReader(DbDataReader reader)
	{
		_underlyingReader = reader;
	}

	private void AssertReaderIsOpen()
	{
		if (_isClosed)
		{
			throw Error.ADP_ClosedDataReaderError();
		}
	}

	private void AssertReaderIsOpenWithData()
	{
		if (_isClosed)
		{
			throw Error.ADP_ClosedDataReaderError();
		}
		if (!_currentResultSet.IsDataReady)
		{
			throw Error.ADP_NoData();
		}
	}

	[Conditional("DEBUG")]
	private void AssertFieldIsReady(int ordinal)
	{
		if (_isClosed)
		{
			throw Error.ADP_ClosedDataReaderError();
		}
		if (!_currentResultSet.IsDataReady)
		{
			throw Error.ADP_NoData();
		}
		if (0 > ordinal || ordinal > _currentResultSet.FieldCount)
		{
			throw new IndexOutOfRangeException();
		}
	}

	internal void Initialize(string providerManifestToken, DbProviderServices providerServices, Type[] columnTypes, bool[] nullableColumns)
	{
		DbDataReader underlyingReader = _underlyingReader;
		if (underlyingReader == null)
		{
			return;
		}
		_underlyingReader = null;
		try
		{
			if (columnTypes != null && underlyingReader.GetType().Name != "SqlDataReader")
			{
				_bufferedDataRecords.Add(ShapedBufferedDataRecord.Initialize(providerManifestToken, providerServices, underlyingReader, columnTypes, nullableColumns));
			}
			else
			{
				_bufferedDataRecords.Add(ShapelessBufferedDataRecord.Initialize(providerManifestToken, providerServices, underlyingReader));
			}
			while (underlyingReader.NextResult())
			{
				_bufferedDataRecords.Add(ShapelessBufferedDataRecord.Initialize(providerManifestToken, providerServices, underlyingReader));
			}
			_recordsAffected = underlyingReader.RecordsAffected;
			_currentResultSet = _bufferedDataRecords[_currentResultSetNumber];
		}
		finally
		{
			underlyingReader.Dispose();
		}
	}

	internal async Task InitializeAsync(string providerManifestToken, DbProviderServices providerServices, Type[] columnTypes, bool[] nullableColumns, CancellationToken cancellationToken)
	{
		if (_underlyingReader == null)
		{
			return;
		}
		cancellationToken.ThrowIfCancellationRequested();
		DbDataReader reader = _underlyingReader;
		_underlyingReader = null;
		try
		{
			if (columnTypes != null && reader.GetType().Name != "SqlDataReader")
			{
				List<BufferedDataRecord> bufferedDataRecords = _bufferedDataRecords;
				bufferedDataRecords.Add(await ShapedBufferedDataRecord.InitializeAsync(providerManifestToken, providerServices, reader, columnTypes, nullableColumns, cancellationToken).WithCurrentCulture());
			}
			else
			{
				List<BufferedDataRecord> bufferedDataRecords = _bufferedDataRecords;
				bufferedDataRecords.Add(await ShapelessBufferedDataRecord.InitializeAsync(providerManifestToken, providerServices, reader, cancellationToken).WithCurrentCulture());
			}
			while (await reader.NextResultAsync(cancellationToken).WithCurrentCulture())
			{
				List<BufferedDataRecord> bufferedDataRecords = _bufferedDataRecords;
				bufferedDataRecords.Add(await ShapelessBufferedDataRecord.InitializeAsync(providerManifestToken, providerServices, reader, cancellationToken).WithCurrentCulture());
			}
			_recordsAffected = reader.RecordsAffected;
			_currentResultSet = _bufferedDataRecords[_currentResultSetNumber];
		}
		finally
		{
			reader.Dispose();
		}
	}

	public override void Close()
	{
		_bufferedDataRecords = null;
		_isClosed = true;
		DbDataReader underlyingReader = _underlyingReader;
		if (underlyingReader != null)
		{
			_underlyingReader = null;
			underlyingReader.Dispose();
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (!_disposed && disposing && !IsClosed)
		{
			Close();
		}
		_disposed = true;
		base.Dispose(disposing);
	}

	public override bool GetBoolean(int ordinal)
	{
		return _currentResultSet.GetBoolean(ordinal);
	}

	public override byte GetByte(int ordinal)
	{
		return _currentResultSet.GetByte(ordinal);
	}

	public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
	{
		throw new NotSupportedException();
	}

	public override char GetChar(int ordinal)
	{
		return _currentResultSet.GetChar(ordinal);
	}

	public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
	{
		throw new NotSupportedException();
	}

	public override DateTime GetDateTime(int ordinal)
	{
		return _currentResultSet.GetDateTime(ordinal);
	}

	public override decimal GetDecimal(int ordinal)
	{
		return _currentResultSet.GetDecimal(ordinal);
	}

	public override double GetDouble(int ordinal)
	{
		return _currentResultSet.GetDouble(ordinal);
	}

	public override float GetFloat(int ordinal)
	{
		return _currentResultSet.GetFloat(ordinal);
	}

	public override Guid GetGuid(int ordinal)
	{
		return _currentResultSet.GetGuid(ordinal);
	}

	public override short GetInt16(int ordinal)
	{
		return _currentResultSet.GetInt16(ordinal);
	}

	public override int GetInt32(int ordinal)
	{
		return _currentResultSet.GetInt32(ordinal);
	}

	public override long GetInt64(int ordinal)
	{
		return _currentResultSet.GetInt64(ordinal);
	}

	public override string GetString(int ordinal)
	{
		return _currentResultSet.GetString(ordinal);
	}

	public override T GetFieldValue<T>(int ordinal)
	{
		return _currentResultSet.GetFieldValue<T>(ordinal);
	}

	public override Task<T> GetFieldValueAsync<T>(int ordinal, CancellationToken cancellationToken)
	{
		return _currentResultSet.GetFieldValueAsync<T>(ordinal, cancellationToken);
	}

	public override object GetValue(int ordinal)
	{
		return _currentResultSet.GetValue(ordinal);
	}

	public override int GetValues(object[] values)
	{
		Check.NotNull(values, "values");
		AssertReaderIsOpenWithData();
		return _currentResultSet.GetValues(values);
	}

	public override string GetDataTypeName(int ordinal)
	{
		AssertReaderIsOpen();
		return _currentResultSet.GetDataTypeName(ordinal);
	}

	public override Type GetFieldType(int ordinal)
	{
		AssertReaderIsOpen();
		return _currentResultSet.GetFieldType(ordinal);
	}

	public override string GetName(int ordinal)
	{
		AssertReaderIsOpen();
		return _currentResultSet.GetName(ordinal);
	}

	public override int GetOrdinal(string name)
	{
		Check.NotNull(name, "name");
		AssertReaderIsOpen();
		return _currentResultSet.GetOrdinal(name);
	}

	public override bool IsDBNull(int ordinal)
	{
		return _currentResultSet.IsDBNull(ordinal);
	}

	public override Task<bool> IsDBNullAsync(int ordinal, CancellationToken cancellationToken)
	{
		return _currentResultSet.IsDBNullAsync(ordinal, cancellationToken);
	}

	public override IEnumerator GetEnumerator()
	{
		return new DbEnumerator((IDataReader)this);
	}

	public override DataTable GetSchemaTable()
	{
		throw new NotSupportedException();
	}

	public override bool NextResult()
	{
		AssertReaderIsOpen();
		if (++_currentResultSetNumber < _bufferedDataRecords.Count)
		{
			_currentResultSet = _bufferedDataRecords[_currentResultSetNumber];
			return true;
		}
		_currentResultSet = null;
		return false;
	}

	public override Task<bool> NextResultAsync(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		return Task.FromResult(NextResult());
	}

	public override bool Read()
	{
		AssertReaderIsOpen();
		return _currentResultSet.Read();
	}

	public override Task<bool> ReadAsync(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		AssertReaderIsOpen();
		return _currentResultSet.ReadAsync(cancellationToken);
	}
}
