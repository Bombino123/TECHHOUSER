using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.Internal.Materialization;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Query.PlanCompiler;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Core.Query.ResultAssembly;

internal class BridgeDataReader : DbDataReader, IExtendedDataRecord, IDataRecord
{
	private Shaper<RecordState> _shaper;

	private IEnumerator<KeyValuePair<Shaper<RecordState>, CoordinatorFactory<RecordState>>> _nextResultShaperInfoEnumerator;

	private CoordinatorFactory<RecordState> _coordinatorFactory;

	private RecordState _defaultRecordState;

	private BridgeDataRecord _dataRecord;

	private bool _hasRows;

	private bool _isClosed;

	private int _initialized;

	private readonly Action _initialize;

	private readonly Func<CancellationToken, Task> _initializeAsync;

	public override int Depth
	{
		get
		{
			EnsureInitialized();
			AssertReaderIsOpen("Depth");
			return _dataRecord.Depth;
		}
	}

	public override bool HasRows
	{
		get
		{
			EnsureInitialized();
			AssertReaderIsOpen("HasRows");
			return _hasRows;
		}
	}

	public override bool IsClosed
	{
		get
		{
			EnsureInitialized();
			if (!_isClosed)
			{
				return _dataRecord.IsClosed;
			}
			return true;
		}
	}

	public override int RecordsAffected
	{
		get
		{
			EnsureInitialized();
			int result = -1;
			if (_dataRecord.Depth == 0)
			{
				result = _shaper.Reader.RecordsAffected;
			}
			return result;
		}
	}

	public override int FieldCount
	{
		get
		{
			EnsureInitialized();
			AssertReaderIsOpen("FieldCount");
			return _defaultRecordState.ColumnCount;
		}
	}

	public override object this[int ordinal]
	{
		get
		{
			EnsureInitialized();
			return _dataRecord[ordinal];
		}
	}

	public override object this[string name]
	{
		get
		{
			EnsureInitialized();
			int ordinal = GetOrdinal(name);
			return _dataRecord[ordinal];
		}
	}

	public DataRecordInfo DataRecordInfo
	{
		get
		{
			EnsureInitialized();
			AssertReaderIsOpen("DataRecordInfo");
			if (_dataRecord.HasData)
			{
				return _dataRecord.DataRecordInfo;
			}
			return _defaultRecordState.DataRecordInfo;
		}
	}

	internal BridgeDataReader(Shaper<RecordState> shaper, CoordinatorFactory<RecordState> coordinatorFactory, int depth, IEnumerator<KeyValuePair<Shaper<RecordState>, CoordinatorFactory<RecordState>>> nextResultShaperInfos)
	{
		BridgeDataReader bridgeDataReader = this;
		_nextResultShaperInfoEnumerator = nextResultShaperInfos;
		_initialize = delegate
		{
			bridgeDataReader.SetShaper(shaper, coordinatorFactory, depth);
		};
		_initializeAsync = (CancellationToken ct) => bridgeDataReader.SetShaperAsync(shaper, coordinatorFactory, depth, ct);
	}

	protected virtual void EnsureInitialized()
	{
		if (Interlocked.CompareExchange(ref _initialized, 1, 0) == 0)
		{
			_initialize();
		}
	}

	protected virtual Task EnsureInitializedAsync(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (Interlocked.CompareExchange(ref _initialized, 1, 0) != 0)
		{
			return Task.FromResult<object>(null);
		}
		return _initializeAsync(cancellationToken);
	}

	private void SetShaper(Shaper<RecordState> shaper, CoordinatorFactory<RecordState> coordinatorFactory, int depth)
	{
		_shaper = shaper;
		_coordinatorFactory = coordinatorFactory;
		_dataRecord = new BridgeDataRecord(shaper, depth);
		if (!_shaper.DataWaiting)
		{
			_shaper.DataWaiting = _shaper.RootEnumerator.MoveNext();
		}
		InitializeHasRows();
	}

	private async Task SetShaperAsync(Shaper<RecordState> shaper, CoordinatorFactory<RecordState> coordinatorFactory, int depth, CancellationToken cancellationToken)
	{
		_shaper = shaper;
		_coordinatorFactory = coordinatorFactory;
		_dataRecord = new BridgeDataRecord(shaper, depth);
		if (!_shaper.DataWaiting)
		{
			Shaper<RecordState> shaper2 = _shaper;
			shaper2.DataWaiting = await _shaper.RootEnumerator.MoveNextAsync(cancellationToken).WithCurrentCulture();
		}
		InitializeHasRows();
	}

	private void InitializeHasRows()
	{
		_hasRows = false;
		if (_shaper.DataWaiting)
		{
			RecordState current = _shaper.RootEnumerator.Current;
			if (current != null)
			{
				_hasRows = current.CoordinatorFactory == _coordinatorFactory;
			}
		}
		_defaultRecordState = _coordinatorFactory.GetDefaultRecordState(_shaper);
	}

	private void AssertReaderIsOpen(string methodName)
	{
		if (IsClosed)
		{
			if (_dataRecord.IsImplicitlyClosed)
			{
				throw Error.ADP_ImplicitlyClosedDataReaderError();
			}
			if (_dataRecord.IsExplicitlyClosed)
			{
				throw Error.ADP_DataReaderClosed(methodName);
			}
		}
	}

	internal void CloseImplicitly()
	{
		EnsureInitialized();
		Consume();
		_dataRecord.CloseImplicitly();
	}

	internal async Task CloseImplicitlyAsync(CancellationToken cancellationToken)
	{
		await EnsureInitializedAsync(cancellationToken).WithCurrentCulture();
		await ConsumeAsync(cancellationToken).WithCurrentCulture();
		await _dataRecord.CloseImplicitlyAsync(cancellationToken).WithCurrentCulture();
	}

	private void Consume()
	{
		while (ReadInternal())
		{
		}
	}

	private async Task ConsumeAsync(CancellationToken cancellationToken)
	{
		while (await ReadInternalAsync(cancellationToken).WithCurrentCulture())
		{
		}
	}

	internal static Type GetClrTypeFromTypeMetadata(TypeUsage typeUsage)
	{
		if (TypeHelpers.TryGetEdmType<PrimitiveType>(typeUsage, out var type))
		{
			return type.ClrEquivalentType;
		}
		if (TypeSemantics.IsReferenceType(typeUsage))
		{
			return typeof(EntityKey);
		}
		if (TypeUtils.IsStructuredType(typeUsage))
		{
			return typeof(DbDataRecord);
		}
		if (TypeUtils.IsCollectionType(typeUsage))
		{
			return typeof(DbDataReader);
		}
		if (TypeUtils.IsEnumerationType(typeUsage))
		{
			return ((EnumType)typeUsage.EdmType).UnderlyingType.ClrEquivalentType;
		}
		return typeof(object);
	}

	public override void Close()
	{
		EnsureInitialized();
		_dataRecord.CloseExplicitly();
		if (!_isClosed)
		{
			_isClosed = true;
			if (_dataRecord.Depth == 0)
			{
				_shaper.Reader.Close();
			}
			else
			{
				Consume();
			}
		}
		if (_nextResultShaperInfoEnumerator != null)
		{
			_nextResultShaperInfoEnumerator.Dispose();
			_nextResultShaperInfoEnumerator = null;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override IEnumerator GetEnumerator()
	{
		return new DbEnumerator((IDataReader)this, closeReader: true);
	}

	public override DataTable GetSchemaTable()
	{
		throw new NotSupportedException(Strings.ADP_GetSchemaTableIsNotSupported);
	}

	public override bool NextResult()
	{
		EnsureInitialized();
		AssertReaderIsOpen("NextResult");
		if (_nextResultShaperInfoEnumerator != null && _shaper.Reader.NextResult() && _nextResultShaperInfoEnumerator.MoveNext())
		{
			KeyValuePair<Shaper<RecordState>, CoordinatorFactory<RecordState>> current = _nextResultShaperInfoEnumerator.Current;
			_dataRecord.CloseImplicitly();
			SetShaper(current.Key, current.Value, 0);
			return true;
		}
		if (_dataRecord.Depth == 0)
		{
			CommandHelper.ConsumeReader(_shaper.Reader);
		}
		else
		{
			Consume();
		}
		CloseImplicitly();
		_dataRecord.SetRecordSource(null, hasData: false);
		return false;
	}

	public override async Task<bool> NextResultAsync(CancellationToken cancellationToken)
	{
		await EnsureInitializedAsync(cancellationToken).WithCurrentCulture();
		AssertReaderIsOpen("NextResult");
		bool flag = _nextResultShaperInfoEnumerator != null;
		if (flag)
		{
			flag = await _shaper.Reader.NextResultAsync(cancellationToken).WithCurrentCulture();
		}
		if (flag && _nextResultShaperInfoEnumerator.MoveNext())
		{
			KeyValuePair<Shaper<RecordState>, CoordinatorFactory<RecordState>> nextResultShaperInfo = _nextResultShaperInfoEnumerator.Current;
			await _dataRecord.CloseImplicitlyAsync(cancellationToken).WithCurrentCulture();
			SetShaper(nextResultShaperInfo.Key, nextResultShaperInfo.Value, 0);
			return true;
		}
		if (_dataRecord.Depth != 0)
		{
			await ConsumeAsync(cancellationToken).WithCurrentCulture();
		}
		else
		{
			await CommandHelper.ConsumeReaderAsync(_shaper.Reader, cancellationToken).WithCurrentCulture();
		}
		await CloseImplicitlyAsync(cancellationToken).WithCurrentCulture();
		_dataRecord.SetRecordSource(null, hasData: false);
		return false;
	}

	public override bool Read()
	{
		EnsureInitialized();
		AssertReaderIsOpen("Read");
		_dataRecord.CloseImplicitly();
		bool flag = ReadInternal();
		_dataRecord.SetRecordSource(_shaper.RootEnumerator.Current, flag);
		return flag;
	}

	public override async Task<bool> ReadAsync(CancellationToken cancellationToken)
	{
		await EnsureInitializedAsync(cancellationToken).WithCurrentCulture();
		AssertReaderIsOpen("Read");
		await _dataRecord.CloseImplicitlyAsync(cancellationToken).WithCurrentCulture();
		bool flag = await ReadInternalAsync(cancellationToken).WithCurrentCulture();
		_dataRecord.SetRecordSource(_shaper.RootEnumerator.Current, flag);
		return flag;
	}

	private bool ReadInternal()
	{
		bool result = false;
		if (!_shaper.DataWaiting)
		{
			_shaper.DataWaiting = _shaper.RootEnumerator.MoveNext();
		}
		while (_shaper.DataWaiting && _shaper.RootEnumerator.Current.CoordinatorFactory != _coordinatorFactory && _shaper.RootEnumerator.Current.CoordinatorFactory.Depth > _coordinatorFactory.Depth)
		{
			_shaper.DataWaiting = _shaper.RootEnumerator.MoveNext();
		}
		if (_shaper.DataWaiting && _shaper.RootEnumerator.Current.CoordinatorFactory == _coordinatorFactory)
		{
			_shaper.DataWaiting = false;
			_shaper.RootEnumerator.Current.AcceptPendingValues();
			result = true;
		}
		return result;
	}

	private async Task<bool> ReadInternalAsync(CancellationToken cancellationToken)
	{
		bool result = false;
		if (!_shaper.DataWaiting)
		{
			Shaper<RecordState> shaper = _shaper;
			shaper.DataWaiting = await _shaper.RootEnumerator.MoveNextAsync(cancellationToken).WithCurrentCulture();
		}
		while (_shaper.DataWaiting && _shaper.RootEnumerator.Current.CoordinatorFactory != _coordinatorFactory && _shaper.RootEnumerator.Current.CoordinatorFactory.Depth > _coordinatorFactory.Depth)
		{
			Shaper<RecordState> shaper = _shaper;
			shaper.DataWaiting = await _shaper.RootEnumerator.MoveNextAsync(cancellationToken).WithCurrentCulture();
		}
		if (_shaper.DataWaiting && _shaper.RootEnumerator.Current.CoordinatorFactory == _coordinatorFactory)
		{
			_shaper.DataWaiting = false;
			_shaper.RootEnumerator.Current.AcceptPendingValues();
			result = true;
		}
		return result;
	}

	public override string GetDataTypeName(int ordinal)
	{
		EnsureInitialized();
		AssertReaderIsOpen("GetDataTypeName");
		if (_dataRecord.HasData)
		{
			return _dataRecord.GetDataTypeName(ordinal);
		}
		return _defaultRecordState.GetTypeUsage(ordinal).ToString();
	}

	public override Type GetFieldType(int ordinal)
	{
		EnsureInitialized();
		AssertReaderIsOpen("GetFieldType");
		if (_dataRecord.HasData)
		{
			return _dataRecord.GetFieldType(ordinal);
		}
		return GetClrTypeFromTypeMetadata(_defaultRecordState.GetTypeUsage(ordinal));
	}

	public override string GetName(int ordinal)
	{
		EnsureInitialized();
		AssertReaderIsOpen("GetName");
		if (_dataRecord.HasData)
		{
			return _dataRecord.GetName(ordinal);
		}
		return _defaultRecordState.GetName(ordinal);
	}

	public override int GetOrdinal(string name)
	{
		EnsureInitialized();
		AssertReaderIsOpen("GetOrdinal");
		if (_dataRecord.HasData)
		{
			return _dataRecord.GetOrdinal(name);
		}
		return _defaultRecordState.GetOrdinal(name);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override Type GetProviderSpecificFieldType(int ordinal)
	{
		throw new NotSupportedException();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override object GetProviderSpecificValue(int ordinal)
	{
		throw new NotSupportedException();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override int GetProviderSpecificValues(object[] values)
	{
		throw new NotSupportedException();
	}

	public override object GetValue(int ordinal)
	{
		EnsureInitialized();
		return _dataRecord.GetValue(ordinal);
	}

	public override async Task<T> GetFieldValueAsync<T>(int ordinal, CancellationToken cancellationToken)
	{
		await EnsureInitializedAsync(cancellationToken).WithCurrentCulture();
		return await base.GetFieldValueAsync<T>(ordinal, cancellationToken).WithCurrentCulture();
	}

	public override int GetValues(object[] values)
	{
		EnsureInitialized();
		return _dataRecord.GetValues(values);
	}

	public override bool GetBoolean(int ordinal)
	{
		EnsureInitialized();
		return _dataRecord.GetBoolean(ordinal);
	}

	public override byte GetByte(int ordinal)
	{
		EnsureInitialized();
		return _dataRecord.GetByte(ordinal);
	}

	public override char GetChar(int ordinal)
	{
		EnsureInitialized();
		return _dataRecord.GetChar(ordinal);
	}

	public override DateTime GetDateTime(int ordinal)
	{
		EnsureInitialized();
		return _dataRecord.GetDateTime(ordinal);
	}

	public override decimal GetDecimal(int ordinal)
	{
		EnsureInitialized();
		return _dataRecord.GetDecimal(ordinal);
	}

	public override double GetDouble(int ordinal)
	{
		EnsureInitialized();
		return _dataRecord.GetDouble(ordinal);
	}

	public override float GetFloat(int ordinal)
	{
		EnsureInitialized();
		return _dataRecord.GetFloat(ordinal);
	}

	public override Guid GetGuid(int ordinal)
	{
		EnsureInitialized();
		return _dataRecord.GetGuid(ordinal);
	}

	public override short GetInt16(int ordinal)
	{
		EnsureInitialized();
		return _dataRecord.GetInt16(ordinal);
	}

	public override int GetInt32(int ordinal)
	{
		EnsureInitialized();
		return _dataRecord.GetInt32(ordinal);
	}

	public override long GetInt64(int ordinal)
	{
		EnsureInitialized();
		return _dataRecord.GetInt64(ordinal);
	}

	public override string GetString(int ordinal)
	{
		EnsureInitialized();
		return _dataRecord.GetString(ordinal);
	}

	public override bool IsDBNull(int ordinal)
	{
		EnsureInitialized();
		return _dataRecord.IsDBNull(ordinal);
	}

	public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
	{
		EnsureInitialized();
		return _dataRecord.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);
	}

	public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
	{
		EnsureInitialized();
		return _dataRecord.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);
	}

	protected override DbDataReader GetDbDataReader(int ordinal)
	{
		EnsureInitialized();
		return (DbDataReader)_dataRecord.GetData(ordinal);
	}

	public DbDataRecord GetDataRecord(int ordinal)
	{
		EnsureInitialized();
		return _dataRecord.GetDataRecord(ordinal);
	}

	public DbDataReader GetDataReader(int ordinal)
	{
		EnsureInitialized();
		return GetDbDataReader(ordinal);
	}
}
