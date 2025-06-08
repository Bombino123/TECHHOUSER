using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.Objects.ELinq;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;

namespace System.Data.Entity.Internal.Linq;

internal class InternalQuery<TElement> : IInternalQuery<TElement>, IInternalQuery
{
	private readonly InternalContext _internalContext;

	private ObjectQuery<TElement> _objectQuery;

	public virtual InternalContext InternalContext => _internalContext;

	public virtual ObjectQuery<TElement> ObjectQuery => _objectQuery;

	ObjectQuery IInternalQuery.ObjectQuery => ObjectQuery;

	public virtual Expression Expression => ((IQueryable)_objectQuery).Expression;

	public virtual ObjectQueryProvider ObjectQueryProvider => _objectQuery.ObjectQueryProvider;

	public Type ElementType => typeof(TElement);

	public InternalQuery(InternalContext internalContext)
	{
		_internalContext = internalContext;
	}

	public InternalQuery(InternalContext internalContext, ObjectQuery objectQuery)
	{
		_internalContext = internalContext;
		_objectQuery = (ObjectQuery<TElement>)objectQuery;
	}

	public virtual void ResetQuery()
	{
		_objectQuery = null;
	}

	public virtual IInternalQuery<TElement> Include(string path)
	{
		return new InternalQuery<TElement>(_internalContext, _objectQuery.Include(path));
	}

	public virtual IInternalQuery<TElement> AsNoTracking()
	{
		return new InternalQuery<TElement>(_internalContext, (ObjectQuery)DbHelpers.CreateNoTrackingQuery(_objectQuery));
	}

	public virtual IInternalQuery<TElement> AsStreaming()
	{
		return new InternalQuery<TElement>(_internalContext, (ObjectQuery)DbHelpers.CreateStreamingQuery(_objectQuery));
	}

	public virtual IInternalQuery<TElement> WithExecutionStrategy(IDbExecutionStrategy executionStrategy)
	{
		return new InternalQuery<TElement>(_internalContext, (ObjectQuery)DbHelpers.CreateQueryWithExecutionStrategy(_objectQuery, executionStrategy));
	}

	protected void InitializeQuery(ObjectQuery<TElement> objectQuery)
	{
		_objectQuery = objectQuery;
	}

	public virtual string ToTraceString()
	{
		return _objectQuery.ToTraceString();
	}

	public virtual IEnumerator<TElement> GetEnumerator()
	{
		InternalContext.Initialize();
		return ((IEnumerable<TElement>)_objectQuery).GetEnumerator();
	}

	IEnumerator IInternalQuery.GetEnumerator()
	{
		return GetEnumerator();
	}

	public virtual IDbAsyncEnumerator<TElement> GetAsyncEnumerator()
	{
		InternalContext.Initialize();
		return ((IDbAsyncEnumerable<TElement>)_objectQuery).GetAsyncEnumerator();
	}

	IDbAsyncEnumerator IInternalQuery.GetAsyncEnumerator()
	{
		return GetAsyncEnumerator();
	}
}
