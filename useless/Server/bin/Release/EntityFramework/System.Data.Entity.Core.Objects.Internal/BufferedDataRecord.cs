using System.Collections.ObjectModel;
using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Core.Objects.Internal;

internal abstract class BufferedDataRecord
{
	protected int _currentRowNumber = -1;

	protected int _rowCount;

	private string[] _dataTypeNames;

	private Type[] _fieldTypes;

	private string[] _columnNames;

	private Lazy<FieldNameLookup> _fieldNameLookup;

	public bool IsDataReady { get; protected set; }

	public bool HasRows => _rowCount > 0;

	public int FieldCount => _dataTypeNames.Length;

	protected virtual void ReadMetadata(string providerManifestToken, DbProviderServices providerServices, DbDataReader reader)
	{
		int fieldCount = reader.FieldCount;
		string[] array = new string[fieldCount];
		Type[] array2 = new Type[fieldCount];
		string[] columnNames = new string[fieldCount];
		for (int i = 0; i < fieldCount; i++)
		{
			array[i] = reader.GetDataTypeName(i);
			array2[i] = reader.GetFieldType(i);
			columnNames[i] = reader.GetName(i);
		}
		_dataTypeNames = array;
		_fieldTypes = array2;
		_columnNames = columnNames;
		_fieldNameLookup = new Lazy<FieldNameLookup>(() => new FieldNameLookup(new ReadOnlyCollection<string>(columnNames)), isThreadSafe: false);
	}

	public abstract bool GetBoolean(int ordinal);

	public abstract byte GetByte(int ordinal);

	public abstract char GetChar(int ordinal);

	public abstract DateTime GetDateTime(int ordinal);

	public abstract decimal GetDecimal(int ordinal);

	public abstract double GetDouble(int ordinal);

	public abstract float GetFloat(int ordinal);

	public abstract Guid GetGuid(int ordinal);

	public abstract short GetInt16(int ordinal);

	public abstract int GetInt32(int ordinal);

	public abstract long GetInt64(int ordinal);

	public abstract string GetString(int ordinal);

	public abstract T GetFieldValue<T>(int ordinal);

	public abstract Task<T> GetFieldValueAsync<T>(int ordinal, CancellationToken cancellationToken);

	public abstract object GetValue(int ordinal);

	public abstract int GetValues(object[] values);

	public abstract bool IsDBNull(int ordinal);

	public abstract Task<bool> IsDBNullAsync(int ordinal, CancellationToken cancellationToken);

	public string GetDataTypeName(int ordinal)
	{
		return _dataTypeNames[ordinal];
	}

	public Type GetFieldType(int ordinal)
	{
		return _fieldTypes[ordinal];
	}

	public string GetName(int ordinal)
	{
		return _columnNames[ordinal];
	}

	public int GetOrdinal(string name)
	{
		return _fieldNameLookup.Value.GetOrdinal(name);
	}

	public abstract bool Read();

	public abstract Task<bool> ReadAsync(CancellationToken cancellationToken);
}
