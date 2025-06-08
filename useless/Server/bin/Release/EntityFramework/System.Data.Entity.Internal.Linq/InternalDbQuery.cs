using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Internal.Linq;

internal class InternalDbQuery<TElement> : DbQuery, IOrderedQueryable<TElement>, IQueryable<TElement>, IEnumerable<TElement>, IEnumerable, IQueryable, IOrderedQueryable, IDbAsyncEnumerable<TElement>, IDbAsyncEnumerable
{
	private readonly IInternalQuery<TElement> _internalQuery;

	internal override IInternalQuery InternalQuery => _internalQuery;

	public InternalDbQuery(IInternalQuery<TElement> internalQuery)
	{
		_internalQuery = internalQuery;
	}

	public override DbQuery Include(string path)
	{
		Check.NotEmpty(path, "path");
		return new InternalDbQuery<TElement>(_internalQuery.Include(path));
	}

	public override DbQuery AsNoTracking()
	{
		return new InternalDbQuery<TElement>(_internalQuery.AsNoTracking());
	}

	[Obsolete("Queries are now streaming by default unless a retrying ExecutionStrategy is used. Calling this method will have no effect.")]
	public override DbQuery AsStreaming()
	{
		return new InternalDbQuery<TElement>(_internalQuery.AsStreaming());
	}

	internal override DbQuery WithExecutionStrategy(IDbExecutionStrategy executionStrategy)
	{
		return new InternalDbQuery<TElement>(_internalQuery.WithExecutionStrategy(executionStrategy));
	}

	internal override IInternalQuery GetInternalQueryWithCheck(string memberName)
	{
		return _internalQuery;
	}

	public IEnumerator<TElement> GetEnumerator()
	{
		return _internalQuery.GetEnumerator();
	}

	public IDbAsyncEnumerator<TElement> GetAsyncEnumerator()
	{
		return _internalQuery.GetAsyncEnumerator();
	}
}
