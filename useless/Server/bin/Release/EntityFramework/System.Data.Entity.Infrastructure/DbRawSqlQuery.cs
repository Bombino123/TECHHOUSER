using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Internal;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Infrastructure;

public class DbRawSqlQuery : IEnumerable, IListSource, IDbAsyncEnumerable
{
	private readonly InternalSqlQuery _internalQuery;

	internal InternalSqlQuery InternalQuery => _internalQuery;

	bool IListSource.ContainsListCollection => false;

	internal DbRawSqlQuery(InternalSqlQuery internalQuery)
	{
		_internalQuery = internalQuery;
	}

	[Obsolete("Queries are now streaming by default unless a retrying ExecutionStrategy is used. Calling this method will have no effect.")]
	public virtual DbRawSqlQuery AsStreaming()
	{
		if (_internalQuery != null)
		{
			return new DbRawSqlQuery(_internalQuery.AsStreaming());
		}
		return this;
	}

	public virtual IEnumerator GetEnumerator()
	{
		return GetInternalQueryWithCheck("GetEnumerator").GetEnumerator();
	}

	IDbAsyncEnumerator IDbAsyncEnumerable.GetAsyncEnumerator()
	{
		return GetInternalQueryWithCheck("IDbAsyncEnumerable.GetAsyncEnumerator").GetAsyncEnumerator();
	}

	public virtual Task ForEachAsync(Action<object> action)
	{
		Check.NotNull(action, "action");
		return IDbAsyncEnumerableExtensions.ForEachAsync(this, action, CancellationToken.None);
	}

	public virtual Task ForEachAsync(Action<object> action, CancellationToken cancellationToken)
	{
		Check.NotNull(action, "action");
		return IDbAsyncEnumerableExtensions.ForEachAsync(this, action, cancellationToken);
	}

	public virtual Task<List<object>> ToListAsync()
	{
		return this.ToListAsync<object>();
	}

	public virtual Task<List<object>> ToListAsync(CancellationToken cancellationToken)
	{
		return this.ToListAsync<object>(cancellationToken);
	}

	public override string ToString()
	{
		if (_internalQuery != null)
		{
			return _internalQuery.ToString();
		}
		return base.ToString();
	}

	private InternalSqlQuery GetInternalQueryWithCheck(string memberName)
	{
		if (_internalQuery == null)
		{
			throw new NotImplementedException(Strings.TestDoubleNotImplemented(memberName, GetType().Name, typeof(DbSqlQuery).Name));
		}
		return _internalQuery;
	}

	IList IListSource.GetList()
	{
		throw Error.DbQuery_BindingToDbQueryNotSupported();
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
public class DbRawSqlQuery<TElement> : IEnumerable<TElement>, IEnumerable, IListSource, IDbAsyncEnumerable<TElement>, IDbAsyncEnumerable
{
	private readonly InternalSqlQuery _internalQuery;

	internal InternalSqlQuery InternalQuery => _internalQuery;

	bool IListSource.ContainsListCollection => false;

	internal DbRawSqlQuery(InternalSqlQuery internalQuery)
	{
		_internalQuery = internalQuery;
	}

	[Obsolete("Queries are now streaming by default unless a retrying ExecutionStrategy is used. Calling this method will have no effect.")]
	public virtual DbRawSqlQuery<TElement> AsStreaming()
	{
		if (_internalQuery != null)
		{
			return new DbRawSqlQuery<TElement>(_internalQuery.AsStreaming());
		}
		return this;
	}

	public virtual IEnumerator<TElement> GetEnumerator()
	{
		return (IEnumerator<TElement>)GetInternalQueryWithCheck("GetEnumerator").GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	IDbAsyncEnumerator<TElement> IDbAsyncEnumerable<TElement>.GetAsyncEnumerator()
	{
		return (IDbAsyncEnumerator<TElement>)GetInternalQueryWithCheck("IDbAsyncEnumerable<TElement>.GetAsyncEnumerator").GetAsyncEnumerator();
	}

	IDbAsyncEnumerator IDbAsyncEnumerable.GetAsyncEnumerator()
	{
		return _internalQuery.GetAsyncEnumerator();
	}

	public Task ForEachAsync(Action<TElement> action)
	{
		Check.NotNull(action, "action");
		return IDbAsyncEnumerableExtensions.ForEachAsync(this, action, CancellationToken.None);
	}

	public Task ForEachAsync(Action<TElement> action, CancellationToken cancellationToken)
	{
		Check.NotNull(action, "action");
		return IDbAsyncEnumerableExtensions.ForEachAsync(this, action, cancellationToken);
	}

	public Task<List<TElement>> ToListAsync()
	{
		return IDbAsyncEnumerableExtensions.ToListAsync(this);
	}

	public Task<List<TElement>> ToListAsync(CancellationToken cancellationToken)
	{
		return IDbAsyncEnumerableExtensions.ToListAsync(this, cancellationToken);
	}

	public Task<TElement[]> ToArrayAsync()
	{
		return IDbAsyncEnumerableExtensions.ToArrayAsync(this);
	}

	public Task<TElement[]> ToArrayAsync(CancellationToken cancellationToken)
	{
		return IDbAsyncEnumerableExtensions.ToArrayAsync(this, cancellationToken);
	}

	public Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TKey>(Func<TElement, TKey> keySelector)
	{
		Check.NotNull(keySelector, "keySelector");
		return IDbAsyncEnumerableExtensions.ToDictionaryAsync(this, keySelector);
	}

	public Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TKey>(Func<TElement, TKey> keySelector, CancellationToken cancellationToken)
	{
		Check.NotNull(keySelector, "keySelector");
		return IDbAsyncEnumerableExtensions.ToDictionaryAsync(this, keySelector, cancellationToken);
	}

	public Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TKey>(Func<TElement, TKey> keySelector, IEqualityComparer<TKey> comparer)
	{
		Check.NotNull(keySelector, "keySelector");
		return IDbAsyncEnumerableExtensions.ToDictionaryAsync(this, keySelector, comparer);
	}

	public Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TKey>(Func<TElement, TKey> keySelector, IEqualityComparer<TKey> comparer, CancellationToken cancellationToken)
	{
		Check.NotNull(keySelector, "keySelector");
		return IDbAsyncEnumerableExtensions.ToDictionaryAsync(this, keySelector, comparer, cancellationToken);
	}

	public Task<Dictionary<TKey, TResult>> ToDictionaryAsync<TKey, TResult>(Func<TElement, TKey> keySelector, Func<TElement, TResult> elementSelector)
	{
		Check.NotNull(keySelector, "keySelector");
		Check.NotNull(elementSelector, "elementSelector");
		return IDbAsyncEnumerableExtensions.ToDictionaryAsync(this, keySelector, elementSelector);
	}

	public Task<Dictionary<TKey, TResult>> ToDictionaryAsync<TKey, TResult>(Func<TElement, TKey> keySelector, Func<TElement, TResult> elementSelector, CancellationToken cancellationToken)
	{
		Check.NotNull(keySelector, "keySelector");
		Check.NotNull(elementSelector, "elementSelector");
		return IDbAsyncEnumerableExtensions.ToDictionaryAsync(this, keySelector, elementSelector, cancellationToken);
	}

	public Task<Dictionary<TKey, TResult>> ToDictionaryAsync<TKey, TResult>(Func<TElement, TKey> keySelector, Func<TElement, TResult> elementSelector, IEqualityComparer<TKey> comparer)
	{
		Check.NotNull(keySelector, "keySelector");
		Check.NotNull(elementSelector, "elementSelector");
		return IDbAsyncEnumerableExtensions.ToDictionaryAsync(this, keySelector, elementSelector, comparer);
	}

	public Task<Dictionary<TKey, TResult>> ToDictionaryAsync<TKey, TResult>(Func<TElement, TKey> keySelector, Func<TElement, TResult> elementSelector, IEqualityComparer<TKey> comparer, CancellationToken cancellationToken)
	{
		Check.NotNull(keySelector, "keySelector");
		Check.NotNull(elementSelector, "elementSelector");
		return IDbAsyncEnumerableExtensions.ToDictionaryAsync(this, keySelector, elementSelector, comparer, cancellationToken);
	}

	public Task<TElement> FirstAsync()
	{
		return IDbAsyncEnumerableExtensions.FirstAsync(this);
	}

	public Task<TElement> FirstAsync(CancellationToken cancellationToken)
	{
		return IDbAsyncEnumerableExtensions.FirstAsync(this, cancellationToken);
	}

	public Task<TElement> FirstAsync(Func<TElement, bool> predicate)
	{
		Check.NotNull(predicate, "predicate");
		return IDbAsyncEnumerableExtensions.FirstAsync(this, predicate);
	}

	public Task<TElement> FirstAsync(Func<TElement, bool> predicate, CancellationToken cancellationToken)
	{
		Check.NotNull(predicate, "predicate");
		return IDbAsyncEnumerableExtensions.FirstAsync(this, predicate, cancellationToken);
	}

	public Task<TElement> FirstOrDefaultAsync()
	{
		return IDbAsyncEnumerableExtensions.FirstOrDefaultAsync(this);
	}

	public Task<TElement> FirstOrDefaultAsync(CancellationToken cancellationToken)
	{
		return IDbAsyncEnumerableExtensions.FirstOrDefaultAsync(this, cancellationToken);
	}

	public Task<TElement> FirstOrDefaultAsync(Func<TElement, bool> predicate)
	{
		Check.NotNull(predicate, "predicate");
		return IDbAsyncEnumerableExtensions.FirstOrDefaultAsync(this, predicate);
	}

	public Task<TElement> FirstOrDefaultAsync(Func<TElement, bool> predicate, CancellationToken cancellationToken)
	{
		Check.NotNull(predicate, "predicate");
		return IDbAsyncEnumerableExtensions.FirstOrDefaultAsync(this, predicate, cancellationToken);
	}

	public Task<TElement> SingleAsync()
	{
		return IDbAsyncEnumerableExtensions.SingleAsync(this);
	}

	public Task<TElement> SingleAsync(CancellationToken cancellationToken)
	{
		return IDbAsyncEnumerableExtensions.SingleAsync(this, cancellationToken);
	}

	public Task<TElement> SingleAsync(Func<TElement, bool> predicate)
	{
		Check.NotNull(predicate, "predicate");
		return IDbAsyncEnumerableExtensions.SingleAsync(this, predicate);
	}

	public Task<TElement> SingleAsync(Func<TElement, bool> predicate, CancellationToken cancellationToken)
	{
		Check.NotNull(predicate, "predicate");
		return IDbAsyncEnumerableExtensions.SingleAsync(this, predicate, cancellationToken);
	}

	public Task<TElement> SingleOrDefaultAsync()
	{
		return IDbAsyncEnumerableExtensions.SingleOrDefaultAsync(this);
	}

	public Task<TElement> SingleOrDefaultAsync(CancellationToken cancellationToken)
	{
		return IDbAsyncEnumerableExtensions.SingleOrDefaultAsync(this, cancellationToken);
	}

	public Task<TElement> SingleOrDefaultAsync(Func<TElement, bool> predicate)
	{
		Check.NotNull(predicate, "predicate");
		return IDbAsyncEnumerableExtensions.SingleOrDefaultAsync(this, predicate);
	}

	public Task<TElement> SingleOrDefaultAsync(Func<TElement, bool> predicate, CancellationToken cancellationToken)
	{
		Check.NotNull(predicate, "predicate");
		return IDbAsyncEnumerableExtensions.SingleOrDefaultAsync(this, predicate, cancellationToken);
	}

	public Task<bool> ContainsAsync(TElement value)
	{
		return IDbAsyncEnumerableExtensions.ContainsAsync(this, value);
	}

	public Task<bool> ContainsAsync(TElement value, CancellationToken cancellationToken)
	{
		return IDbAsyncEnumerableExtensions.ContainsAsync(this, value, cancellationToken);
	}

	public Task<bool> AnyAsync()
	{
		return IDbAsyncEnumerableExtensions.AnyAsync(this);
	}

	public Task<bool> AnyAsync(CancellationToken cancellationToken)
	{
		return IDbAsyncEnumerableExtensions.AnyAsync(this, cancellationToken);
	}

	public Task<bool> AnyAsync(Func<TElement, bool> predicate)
	{
		Check.NotNull(predicate, "predicate");
		return IDbAsyncEnumerableExtensions.AnyAsync(this, predicate);
	}

	public Task<bool> AnyAsync(Func<TElement, bool> predicate, CancellationToken cancellationToken)
	{
		Check.NotNull(predicate, "predicate");
		return IDbAsyncEnumerableExtensions.AnyAsync(this, predicate, cancellationToken);
	}

	public Task<bool> AllAsync(Func<TElement, bool> predicate)
	{
		Check.NotNull(predicate, "predicate");
		return IDbAsyncEnumerableExtensions.AllAsync(this, predicate);
	}

	public Task<bool> AllAsync(Func<TElement, bool> predicate, CancellationToken cancellationToken)
	{
		Check.NotNull(predicate, "predicate");
		return IDbAsyncEnumerableExtensions.AllAsync(this, predicate, cancellationToken);
	}

	public Task<int> CountAsync()
	{
		return IDbAsyncEnumerableExtensions.CountAsync(this);
	}

	public Task<int> CountAsync(CancellationToken cancellationToken)
	{
		return IDbAsyncEnumerableExtensions.CountAsync(this, cancellationToken);
	}

	public Task<int> CountAsync(Func<TElement, bool> predicate)
	{
		Check.NotNull(predicate, "predicate");
		return IDbAsyncEnumerableExtensions.CountAsync(this, predicate);
	}

	public Task<int> CountAsync(Func<TElement, bool> predicate, CancellationToken cancellationToken)
	{
		Check.NotNull(predicate, "predicate");
		return IDbAsyncEnumerableExtensions.CountAsync(this, predicate, cancellationToken);
	}

	public Task<long> LongCountAsync()
	{
		return IDbAsyncEnumerableExtensions.LongCountAsync(this);
	}

	public Task<long> LongCountAsync(CancellationToken cancellationToken)
	{
		return IDbAsyncEnumerableExtensions.LongCountAsync(this, cancellationToken);
	}

	public Task<long> LongCountAsync(Func<TElement, bool> predicate)
	{
		Check.NotNull(predicate, "predicate");
		return IDbAsyncEnumerableExtensions.LongCountAsync(this, predicate);
	}

	public Task<long> LongCountAsync(Func<TElement, bool> predicate, CancellationToken cancellationToken)
	{
		Check.NotNull(predicate, "predicate");
		return IDbAsyncEnumerableExtensions.LongCountAsync(this, predicate, cancellationToken);
	}

	public Task<TElement> MinAsync()
	{
		return IDbAsyncEnumerableExtensions.MinAsync(this);
	}

	public Task<TElement> MinAsync(CancellationToken cancellationToken)
	{
		return IDbAsyncEnumerableExtensions.MinAsync(this, cancellationToken);
	}

	public Task<TElement> MaxAsync()
	{
		return IDbAsyncEnumerableExtensions.MaxAsync(this);
	}

	public Task<TElement> MaxAsync(CancellationToken cancellationToken)
	{
		return IDbAsyncEnumerableExtensions.MaxAsync(this, cancellationToken);
	}

	public override string ToString()
	{
		if (_internalQuery != null)
		{
			return _internalQuery.ToString();
		}
		return base.ToString();
	}

	private InternalSqlQuery GetInternalQueryWithCheck(string memberName)
	{
		if (_internalQuery == null)
		{
			throw new NotImplementedException(Strings.TestDoubleNotImplemented(memberName, GetType().Name, typeof(DbSqlQuery<>).Name));
		}
		return _internalQuery;
	}

	IList IListSource.GetList()
	{
		throw Error.DbQuery_BindingToDbQueryNotSupported();
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
