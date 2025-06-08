using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.Objects.ELinq;
using System.Data.Entity.Infrastructure;
using System.Linq.Expressions;

namespace System.Data.Entity.Internal.Linq;

internal interface IInternalQuery
{
	InternalContext InternalContext { get; }

	ObjectQuery ObjectQuery { get; }

	Type ElementType { get; }

	Expression Expression { get; }

	ObjectQueryProvider ObjectQueryProvider { get; }

	void ResetQuery();

	string ToTraceString();

	IDbAsyncEnumerator GetAsyncEnumerator();

	IEnumerator GetEnumerator();
}
internal interface IInternalQuery<out TElement> : IInternalQuery
{
	IInternalQuery<TElement> Include(string path);

	IInternalQuery<TElement> AsNoTracking();

	IInternalQuery<TElement> AsStreaming();

	IInternalQuery<TElement> WithExecutionStrategy(IDbExecutionStrategy executionStrategy);

	new IDbAsyncEnumerator<TElement> GetAsyncEnumerator();

	new IEnumerator<TElement> GetEnumerator();
}
