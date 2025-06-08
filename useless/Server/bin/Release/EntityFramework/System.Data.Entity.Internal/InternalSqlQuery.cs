using System.Collections;
using System.Data.Entity.Infrastructure;

namespace System.Data.Entity.Internal;

internal abstract class InternalSqlQuery : IEnumerable, IDbAsyncEnumerable
{
	private readonly string _sql;

	private readonly object[] _parameters;

	private readonly bool? _streaming;

	public string Sql => _sql;

	internal bool? Streaming => _streaming;

	public object[] Parameters => _parameters;

	internal InternalSqlQuery(string sql, bool? streaming, object[] parameters)
	{
		_sql = sql;
		_parameters = parameters;
		_streaming = streaming;
	}

	public abstract InternalSqlQuery AsNoTracking();

	public abstract InternalSqlQuery AsStreaming();

	public abstract IEnumerator GetEnumerator();

	public abstract IDbAsyncEnumerator GetAsyncEnumerator();

	public override string ToString()
	{
		return Sql;
	}
}
