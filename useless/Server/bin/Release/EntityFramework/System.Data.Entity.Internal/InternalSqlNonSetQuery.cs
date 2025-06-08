using System.Collections;
using System.Data.Entity.Infrastructure;

namespace System.Data.Entity.Internal;

internal class InternalSqlNonSetQuery : InternalSqlQuery
{
	private readonly InternalContext _internalContext;

	private readonly Type _elementType;

	internal InternalSqlNonSetQuery(InternalContext internalContext, Type elementType, string sql, object[] parameters)
		: this(internalContext, elementType, sql, null, parameters)
	{
	}

	private InternalSqlNonSetQuery(InternalContext internalContext, Type elementType, string sql, bool? streaming, object[] parameters)
		: base(sql, streaming, parameters)
	{
		_internalContext = internalContext;
		_elementType = elementType;
	}

	public override InternalSqlQuery AsNoTracking()
	{
		return this;
	}

	public override InternalSqlQuery AsStreaming()
	{
		if (!base.Streaming.HasValue || !base.Streaming.Value)
		{
			return new InternalSqlNonSetQuery(_internalContext, _elementType, base.Sql, true, base.Parameters);
		}
		return this;
	}

	public override IEnumerator GetEnumerator()
	{
		return _internalContext.ExecuteSqlQuery(_elementType, base.Sql, base.Streaming, base.Parameters);
	}

	public override IDbAsyncEnumerator GetAsyncEnumerator()
	{
		return _internalContext.ExecuteSqlQueryAsync(_elementType, base.Sql, base.Streaming, base.Parameters);
	}
}
