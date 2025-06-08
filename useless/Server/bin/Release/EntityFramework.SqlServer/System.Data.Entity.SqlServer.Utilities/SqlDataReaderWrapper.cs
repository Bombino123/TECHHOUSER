using System.Collections;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace System.Data.Entity.SqlServer.Utilities;

internal class SqlDataReaderWrapper : MarshalByRefObject
{
	private readonly SqlDataReader _sqlDataReader;

	public virtual int Depth => ((DbDataReader)(object)_sqlDataReader).Depth;

	public virtual int FieldCount => ((DbDataReader)(object)_sqlDataReader).FieldCount;

	public virtual bool HasRows => ((DbDataReader)(object)_sqlDataReader).HasRows;

	public virtual bool IsClosed => ((DbDataReader)(object)_sqlDataReader).IsClosed;

	public virtual int RecordsAffected => ((DbDataReader)(object)_sqlDataReader).RecordsAffected;

	public virtual int VisibleFieldCount => ((DbDataReader)(object)_sqlDataReader).VisibleFieldCount;

	public virtual object this[int i] => ((DbDataReader)(object)_sqlDataReader)[i];

	public virtual object this[string name] => ((DbDataReader)(object)_sqlDataReader)[name];

	protected SqlDataReaderWrapper()
	{
	}

	public SqlDataReaderWrapper(SqlDataReader sqlDataReader)
	{
		_sqlDataReader = sqlDataReader;
	}

	public virtual IDataReader GetData(int i)
	{
		return ((IDataRecord)_sqlDataReader).GetData(i);
	}

	public virtual void Dispose()
	{
		((DbDataReader)(object)_sqlDataReader).Dispose();
	}

	public virtual Task<T> GetFieldValueAsync<T>(int ordinal)
	{
		return ((DbDataReader)(object)_sqlDataReader).GetFieldValueAsync<T>(ordinal);
	}

	public virtual Task<bool> IsDBNullAsync(int ordinal)
	{
		return ((DbDataReader)(object)_sqlDataReader).IsDBNullAsync(ordinal);
	}

	public virtual Task<bool> ReadAsync()
	{
		return ((DbDataReader)(object)_sqlDataReader).ReadAsync();
	}

	public virtual Task<bool> NextResultAsync()
	{
		return ((DbDataReader)(object)_sqlDataReader).NextResultAsync();
	}

	public virtual void Close()
	{
		((DbDataReader)(object)_sqlDataReader).Close();
	}

	public virtual string GetDataTypeName(int i)
	{
		return ((DbDataReader)(object)_sqlDataReader).GetDataTypeName(i);
	}

	public virtual IEnumerator GetEnumerator()
	{
		return ((DbDataReader)(object)_sqlDataReader).GetEnumerator();
	}

	public virtual Type GetFieldType(int i)
	{
		return ((DbDataReader)(object)_sqlDataReader).GetFieldType(i);
	}

	public virtual string GetName(int i)
	{
		return ((DbDataReader)(object)_sqlDataReader).GetName(i);
	}

	public virtual Type GetProviderSpecificFieldType(int i)
	{
		return ((DbDataReader)(object)_sqlDataReader).GetProviderSpecificFieldType(i);
	}

	public virtual int GetOrdinal(string name)
	{
		return ((DbDataReader)(object)_sqlDataReader).GetOrdinal(name);
	}

	public virtual object GetProviderSpecificValue(int i)
	{
		return ((DbDataReader)(object)_sqlDataReader).GetProviderSpecificValue(i);
	}

	public virtual int GetProviderSpecificValues(object[] values)
	{
		return ((DbDataReader)(object)_sqlDataReader).GetProviderSpecificValues(values);
	}

	public virtual DataTable GetSchemaTable()
	{
		return ((DbDataReader)(object)_sqlDataReader).GetSchemaTable();
	}

	public virtual bool GetBoolean(int i)
	{
		return ((DbDataReader)(object)_sqlDataReader).GetBoolean(i);
	}

	public virtual XmlReader GetXmlReader(int i)
	{
		return _sqlDataReader.GetXmlReader(i);
	}

	public virtual Stream GetStream(int i)
	{
		return ((DbDataReader)(object)_sqlDataReader).GetStream(i);
	}

	public virtual byte GetByte(int i)
	{
		return ((DbDataReader)(object)_sqlDataReader).GetByte(i);
	}

	public virtual long GetBytes(int i, long dataIndex, byte[] buffer, int bufferIndex, int length)
	{
		return ((DbDataReader)(object)_sqlDataReader).GetBytes(i, dataIndex, buffer, bufferIndex, length);
	}

	public virtual TextReader GetTextReader(int i)
	{
		return ((DbDataReader)(object)_sqlDataReader).GetTextReader(i);
	}

	public virtual char GetChar(int i)
	{
		return ((DbDataReader)(object)_sqlDataReader).GetChar(i);
	}

	public virtual long GetChars(int i, long dataIndex, char[] buffer, int bufferIndex, int length)
	{
		return ((DbDataReader)(object)_sqlDataReader).GetChars(i, dataIndex, buffer, bufferIndex, length);
	}

	public virtual DateTime GetDateTime(int i)
	{
		return ((DbDataReader)(object)_sqlDataReader).GetDateTime(i);
	}

	public virtual decimal GetDecimal(int i)
	{
		return ((DbDataReader)(object)_sqlDataReader).GetDecimal(i);
	}

	public virtual double GetDouble(int i)
	{
		return ((DbDataReader)(object)_sqlDataReader).GetDouble(i);
	}

	public virtual float GetFloat(int i)
	{
		return ((DbDataReader)(object)_sqlDataReader).GetFloat(i);
	}

	public virtual Guid GetGuid(int i)
	{
		return ((DbDataReader)(object)_sqlDataReader).GetGuid(i);
	}

	public virtual short GetInt16(int i)
	{
		return ((DbDataReader)(object)_sqlDataReader).GetInt16(i);
	}

	public virtual int GetInt32(int i)
	{
		return ((DbDataReader)(object)_sqlDataReader).GetInt32(i);
	}

	public virtual long GetInt64(int i)
	{
		return ((DbDataReader)(object)_sqlDataReader).GetInt64(i);
	}

	public virtual SqlBoolean GetSqlBoolean(int i)
	{
		return _sqlDataReader.GetSqlBoolean(i);
	}

	public virtual SqlBinary GetSqlBinary(int i)
	{
		return _sqlDataReader.GetSqlBinary(i);
	}

	public virtual SqlByte GetSqlByte(int i)
	{
		return _sqlDataReader.GetSqlByte(i);
	}

	public virtual SqlBytes GetSqlBytes(int i)
	{
		return _sqlDataReader.GetSqlBytes(i);
	}

	public virtual SqlChars GetSqlChars(int i)
	{
		return _sqlDataReader.GetSqlChars(i);
	}

	public virtual SqlDateTime GetSqlDateTime(int i)
	{
		return _sqlDataReader.GetSqlDateTime(i);
	}

	public virtual SqlDecimal GetSqlDecimal(int i)
	{
		return _sqlDataReader.GetSqlDecimal(i);
	}

	public virtual SqlGuid GetSqlGuid(int i)
	{
		return _sqlDataReader.GetSqlGuid(i);
	}

	public virtual SqlDouble GetSqlDouble(int i)
	{
		return _sqlDataReader.GetSqlDouble(i);
	}

	public virtual SqlInt16 GetSqlInt16(int i)
	{
		return _sqlDataReader.GetSqlInt16(i);
	}

	public virtual SqlInt32 GetSqlInt32(int i)
	{
		return _sqlDataReader.GetSqlInt32(i);
	}

	public virtual SqlInt64 GetSqlInt64(int i)
	{
		return _sqlDataReader.GetSqlInt64(i);
	}

	public virtual SqlMoney GetSqlMoney(int i)
	{
		return _sqlDataReader.GetSqlMoney(i);
	}

	public virtual SqlSingle GetSqlSingle(int i)
	{
		return _sqlDataReader.GetSqlSingle(i);
	}

	public virtual SqlString GetSqlString(int i)
	{
		return _sqlDataReader.GetSqlString(i);
	}

	public virtual SqlXml GetSqlXml(int i)
	{
		return _sqlDataReader.GetSqlXml(i);
	}

	public virtual object GetSqlValue(int i)
	{
		return _sqlDataReader.GetSqlValue(i);
	}

	public virtual int GetSqlValues(object[] values)
	{
		return _sqlDataReader.GetSqlValues(values);
	}

	public virtual string GetString(int i)
	{
		return ((DbDataReader)(object)_sqlDataReader).GetString(i);
	}

	public virtual T GetFieldValue<T>(int i)
	{
		return ((DbDataReader)(object)_sqlDataReader).GetFieldValue<T>(i);
	}

	public virtual object GetValue(int i)
	{
		return ((DbDataReader)(object)_sqlDataReader).GetValue(i);
	}

	public virtual TimeSpan GetTimeSpan(int i)
	{
		return _sqlDataReader.GetTimeSpan(i);
	}

	public virtual DateTimeOffset GetDateTimeOffset(int i)
	{
		return _sqlDataReader.GetDateTimeOffset(i);
	}

	public virtual int GetValues(object[] values)
	{
		return ((DbDataReader)(object)_sqlDataReader).GetValues(values);
	}

	public virtual bool IsDBNull(int i)
	{
		return ((DbDataReader)(object)_sqlDataReader).IsDBNull(i);
	}

	public virtual bool NextResult()
	{
		return ((DbDataReader)(object)_sqlDataReader).NextResult();
	}

	public virtual bool Read()
	{
		return ((DbDataReader)(object)_sqlDataReader).Read();
	}

	public virtual Task<bool> NextResultAsync(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		return ((DbDataReader)(object)_sqlDataReader).NextResultAsync(cancellationToken);
	}

	public virtual Task<bool> ReadAsync(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		return ((DbDataReader)(object)_sqlDataReader).ReadAsync(cancellationToken);
	}

	public virtual Task<bool> IsDBNullAsync(int i, CancellationToken cancellationToken)
	{
		return ((DbDataReader)(object)_sqlDataReader).IsDBNullAsync(i, cancellationToken);
	}

	public virtual Task<T> GetFieldValueAsync<T>(int i, CancellationToken cancellationToken)
	{
		return ((DbDataReader)(object)_sqlDataReader).GetFieldValueAsync<T>(i, cancellationToken);
	}
}
