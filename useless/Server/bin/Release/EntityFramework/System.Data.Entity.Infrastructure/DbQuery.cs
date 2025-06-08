using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Internal.Linq;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace System.Data.Entity.Infrastructure;

[DebuggerDisplay("{DebuggerDisplay()}")]
public abstract class DbQuery : IOrderedQueryable, IQueryable, IEnumerable, IListSource, IInternalQueryAdapter, IDbAsyncEnumerable
{
	private IQueryProvider _provider;

	bool IListSource.ContainsListCollection => false;

	public virtual Type ElementType => GetInternalQueryWithCheck("ElementType").ElementType;

	Expression IQueryable.Expression => GetInternalQueryWithCheck("IQueryable.Expression").Expression;

	IQueryProvider IQueryable.Provider => _provider ?? (_provider = new NonGenericDbQueryProvider(GetInternalQueryWithCheck("IQueryable.Provider").InternalContext, GetInternalQueryWithCheck("IQueryable.Provider")));

	public string Sql => ToString();

	internal virtual IInternalQuery InternalQuery => null;

	IInternalQuery IInternalQueryAdapter.InternalQuery => InternalQuery;

	internal DbQuery()
	{
	}

	IList IListSource.GetList()
	{
		throw Error.DbQuery_BindingToDbQueryNotSupported();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetInternalQueryWithCheck("IEnumerable.GetEnumerator").GetEnumerator();
	}

	IDbAsyncEnumerator IDbAsyncEnumerable.GetAsyncEnumerator()
	{
		return GetInternalQueryWithCheck("IDbAsyncEnumerable.GetAsyncEnumerator").GetAsyncEnumerator();
	}

	public virtual DbQuery Include(string path)
	{
		return this;
	}

	public virtual DbQuery AsNoTracking()
	{
		return this;
	}

	[Obsolete("Queries are now streaming by default unless a retrying ExecutionStrategy is used. Calling this method will have no effect.")]
	public virtual DbQuery AsStreaming()
	{
		return this;
	}

	internal virtual DbQuery WithExecutionStrategy(IDbExecutionStrategy executionStrategy)
	{
		return this;
	}

	public DbQuery<TElement> Cast<TElement>()
	{
		if (InternalQuery == null)
		{
			throw new NotSupportedException(Strings.TestDoublesCannotBeConverted);
		}
		if (typeof(TElement) != InternalQuery.ElementType)
		{
			throw Error.DbEntity_BadTypeForCast(typeof(DbQuery).Name, typeof(TElement).Name, InternalQuery.ElementType.Name);
		}
		return new DbQuery<TElement>((IInternalQuery<TElement>)InternalQuery);
	}

	public override string ToString()
	{
		if (InternalQuery != null)
		{
			return InternalQuery.ToTraceString();
		}
		return base.ToString();
	}

	private string DebuggerDisplay()
	{
		return base.ToString();
	}

	internal virtual IInternalQuery GetInternalQueryWithCheck(string memberName)
	{
		throw new NotImplementedException(Strings.TestDoubleNotImplemented(memberName, GetType().Name, typeof(DbSet).Name));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override bool Equals(object obj)
	{
		return base.Equals(obj);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public new Type GetType()
	{
		return base.GetType();
	}
}
[DebuggerDisplay("{DebuggerDisplay()}")]
public class DbQuery<TResult> : IOrderedQueryable<TResult>, IQueryable<TResult>, IEnumerable<TResult>, IEnumerable, IQueryable, IOrderedQueryable, IListSource, IInternalQueryAdapter, IDbAsyncEnumerable<TResult>, IDbAsyncEnumerable
{
	private readonly IInternalQuery<TResult> _internalQuery;

	private IQueryProvider _provider;

	bool IListSource.ContainsListCollection => false;

	Type IQueryable.ElementType => GetInternalQueryWithCheck("IQueryable.ElementType").ElementType;

	Expression IQueryable.Expression => GetInternalQueryWithCheck("IQueryable.Expression").Expression;

	IQueryProvider IQueryable.Provider => _provider ?? (_provider = new DbQueryProvider(GetInternalQueryWithCheck("IQueryable.Provider").InternalContext, GetInternalQueryWithCheck("IQueryable.Provider")));

	IInternalQuery IInternalQueryAdapter.InternalQuery => _internalQuery;

	internal IInternalQuery<TResult> InternalQuery => _internalQuery;

	public string Sql => ToString();

	internal DbQuery(IInternalQuery<TResult> internalQuery)
	{
		_internalQuery = internalQuery;
	}

	public virtual DbQuery<TResult> Include(string path)
	{
		Check.NotEmpty(path, "path");
		if (_internalQuery != null)
		{
			return new DbQuery<TResult>(_internalQuery.Include(path));
		}
		return this;
	}

	public virtual DbQuery<TResult> AsNoTracking()
	{
		if (_internalQuery != null)
		{
			return new DbQuery<TResult>(_internalQuery.AsNoTracking());
		}
		return this;
	}

	[Obsolete("Queries are now streaming by default unless a retrying ExecutionStrategy is used. Calling this method will have no effect.")]
	public virtual DbQuery<TResult> AsStreaming()
	{
		if (_internalQuery != null)
		{
			return new DbQuery<TResult>(_internalQuery.AsStreaming());
		}
		return this;
	}

	internal virtual DbQuery<TResult> WithExecutionStrategy(IDbExecutionStrategy executionStrategy)
	{
		if (_internalQuery != null)
		{
			return new DbQuery<TResult>(_internalQuery.WithExecutionStrategy(executionStrategy));
		}
		return this;
	}

	IList IListSource.GetList()
	{
		throw Error.DbQuery_BindingToDbQueryNotSupported();
	}

	IEnumerator<TResult> IEnumerable<TResult>.GetEnumerator()
	{
		return GetInternalQueryWithCheck("IEnumerable<TResult>.GetEnumerator").GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetInternalQueryWithCheck("IEnumerable.GetEnumerator").GetEnumerator();
	}

	IDbAsyncEnumerator IDbAsyncEnumerable.GetAsyncEnumerator()
	{
		return GetInternalQueryWithCheck("IDbAsyncEnumerable.GetAsyncEnumerator").GetAsyncEnumerator();
	}

	IDbAsyncEnumerator<TResult> IDbAsyncEnumerable<TResult>.GetAsyncEnumerator()
	{
		return GetInternalQueryWithCheck("IDbAsyncEnumerable<TResult>.GetAsyncEnumerator").GetAsyncEnumerator();
	}

	private IInternalQuery<TResult> GetInternalQueryWithCheck(string memberName)
	{
		if (_internalQuery == null)
		{
			throw new NotImplementedException(Strings.TestDoubleNotImplemented(memberName, GetType().Name, typeof(DbSet<>).Name));
		}
		return _internalQuery;
	}

	public override string ToString()
	{
		if (_internalQuery != null)
		{
			return _internalQuery.ToTraceString();
		}
		return base.ToString();
	}

	private string DebuggerDisplay()
	{
		return base.ToString();
	}

	public static implicit operator DbQuery(DbQuery<TResult> entry)
	{
		if (entry._internalQuery == null)
		{
			throw new NotSupportedException(Strings.TestDoublesCannotBeConverted);
		}
		return new InternalDbQuery<TResult>(entry._internalQuery);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override bool Equals(object obj)
	{
		return base.Equals(obj);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public new Type GetType()
	{
		return base.GetType();
	}
}
