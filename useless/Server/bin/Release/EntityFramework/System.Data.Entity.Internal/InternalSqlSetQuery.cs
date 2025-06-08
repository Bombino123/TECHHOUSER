using System.Collections;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Internal.Linq;

namespace System.Data.Entity.Internal;

internal class InternalSqlSetQuery : InternalSqlQuery
{
	private readonly IInternalSet _set;

	private readonly bool _isNoTracking;

	public bool IsNoTracking => _isNoTracking;

	internal InternalSqlSetQuery(IInternalSet set, string sql, bool isNoTracking, object[] parameters)
		: this(set, sql, isNoTracking, null, parameters)
	{
	}

	private InternalSqlSetQuery(IInternalSet set, string sql, bool isNoTracking, bool? streaming, object[] parameters)
		: base(sql, streaming, parameters)
	{
		_set = set;
		_isNoTracking = isNoTracking;
	}

	public override InternalSqlQuery AsNoTracking()
	{
		if (!_isNoTracking)
		{
			return new InternalSqlSetQuery(_set, base.Sql, isNoTracking: true, base.Streaming, base.Parameters);
		}
		return this;
	}

	public override InternalSqlQuery AsStreaming()
	{
		if (!base.Streaming.HasValue || !base.Streaming.Value)
		{
			return new InternalSqlSetQuery(_set, base.Sql, _isNoTracking, true, base.Parameters);
		}
		return this;
	}

	public override IEnumerator GetEnumerator()
	{
		return _set.ExecuteSqlQuery(base.Sql, _isNoTracking, base.Streaming, base.Parameters);
	}

	public override IDbAsyncEnumerator GetAsyncEnumerator()
	{
		return _set.ExecuteSqlQueryAsync(base.Sql, _isNoTracking, base.Streaming, base.Parameters);
	}
}
