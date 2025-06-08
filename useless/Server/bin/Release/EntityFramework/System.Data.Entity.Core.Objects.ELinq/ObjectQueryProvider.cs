using System.Collections.Generic;
using System.Data.Entity.Core.Objects.Internal;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Core.Objects.ELinq;

internal class ObjectQueryProvider : IQueryProvider, IDbAsyncQueryProvider
{
	private readonly ObjectContext _context;

	private readonly ObjectQuery _query;

	internal ObjectQueryProvider(ObjectContext context)
	{
		_context = context;
	}

	internal ObjectQueryProvider(ObjectQuery query)
		: this(query.Context)
	{
		_query = query;
	}

	internal virtual ObjectQuery<TElement> CreateQuery<TElement>(Expression expression)
	{
		return GetObjectQueryState(_query, expression, typeof(TElement)).CreateObjectQuery<TElement>();
	}

	internal virtual ObjectQuery CreateQuery(Expression expression, Type ofType)
	{
		return GetObjectQueryState(_query, expression, ofType).CreateQuery();
	}

	private ObjectQueryState GetObjectQueryState(ObjectQuery query, Expression expression, Type ofType)
	{
		if (query != null)
		{
			return new ELinqQueryState(ofType, _query, expression);
		}
		return new ELinqQueryState(ofType, _context, expression);
	}

	IQueryable<TElement> IQueryProvider.CreateQuery<TElement>(Expression expression)
	{
		Check.NotNull(expression, "expression");
		if (!typeof(IQueryable<TElement>).IsAssignableFrom(expression.Type))
		{
			throw new ArgumentException(Strings.ELinq_ExpressionMustBeIQueryable, "expression");
		}
		return CreateQuery<TElement>(expression);
	}

	TResult IQueryProvider.Execute<TResult>(Expression expression)
	{
		Check.NotNull(expression, "expression");
		return ExecuteSingle(CreateQuery<TResult>(expression), expression);
	}

	IQueryable IQueryProvider.CreateQuery(Expression expression)
	{
		Check.NotNull(expression, "expression");
		if (!typeof(IQueryable).IsAssignableFrom(expression.Type))
		{
			throw new ArgumentException(Strings.ELinq_ExpressionMustBeIQueryable, "expression");
		}
		Type elementType = TypeSystem.GetElementType(expression.Type);
		return CreateQuery(expression, elementType);
	}

	object IQueryProvider.Execute(Expression expression)
	{
		Check.NotNull(expression, "expression");
		return ExecuteSingle(Enumerable.Cast<object>(CreateQuery(expression, expression.Type)), expression);
	}

	Task<TResult> IDbAsyncQueryProvider.ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
	{
		Check.NotNull(expression, "expression");
		cancellationToken.ThrowIfCancellationRequested();
		return ExecuteSingleAsync(CreateQuery<TResult>(expression), expression, cancellationToken);
	}

	Task<object> IDbAsyncQueryProvider.ExecuteAsync(Expression expression, CancellationToken cancellationToken)
	{
		Check.NotNull(expression, "expression");
		cancellationToken.ThrowIfCancellationRequested();
		return ExecuteSingleAsync(IDbAsyncEnumerableExtensions.Cast<object>(CreateQuery(expression, expression.Type)), expression, cancellationToken);
	}

	internal static TResult ExecuteSingle<TResult>(IEnumerable<TResult> query, Expression queryRoot)
	{
		return GetElementFunction<TResult>(queryRoot)(query);
	}

	private static Func<IEnumerable<TResult>, TResult> GetElementFunction<TResult>(Expression queryRoot)
	{
		if (ReflectionUtil.TryIdentifySequenceMethod(queryRoot, unwrapLambda: true, out var sequenceMethod))
		{
			switch (sequenceMethod)
			{
			case SequenceMethod.First:
			case SequenceMethod.FirstPredicate:
				return (IEnumerable<TResult> sequence) => sequence.First();
			case SequenceMethod.FirstOrDefault:
			case SequenceMethod.FirstOrDefaultPredicate:
				return (IEnumerable<TResult> sequence) => sequence.FirstOrDefault();
			case SequenceMethod.SingleOrDefault:
			case SequenceMethod.SingleOrDefaultPredicate:
				return (IEnumerable<TResult> sequence) => sequence.SingleOrDefault();
			}
		}
		return (IEnumerable<TResult> sequence) => sequence.Single();
	}

	internal static Task<TResult> ExecuteSingleAsync<TResult>(IDbAsyncEnumerable<TResult> query, Expression queryRoot, CancellationToken cancellationToken)
	{
		return GetAsyncElementFunction<TResult>(queryRoot)(query, cancellationToken);
	}

	private static Func<IDbAsyncEnumerable<TResult>, CancellationToken, Task<TResult>> GetAsyncElementFunction<TResult>(Expression queryRoot)
	{
		if (ReflectionUtil.TryIdentifySequenceMethod(queryRoot, unwrapLambda: true, out var sequenceMethod))
		{
			switch (sequenceMethod)
			{
			case SequenceMethod.First:
			case SequenceMethod.FirstPredicate:
				return (IDbAsyncEnumerable<TResult> sequence, CancellationToken cancellationToken) => sequence.FirstAsync(cancellationToken);
			case SequenceMethod.FirstOrDefault:
			case SequenceMethod.FirstOrDefaultPredicate:
				return (IDbAsyncEnumerable<TResult> sequence, CancellationToken cancellationToken) => sequence.FirstOrDefaultAsync(cancellationToken);
			case SequenceMethod.SingleOrDefault:
			case SequenceMethod.SingleOrDefaultPredicate:
				return (IDbAsyncEnumerable<TResult> sequence, CancellationToken cancellationToken) => sequence.SingleOrDefaultAsync(cancellationToken);
			}
		}
		return (IDbAsyncEnumerable<TResult> sequence, CancellationToken cancellationToken) => sequence.SingleAsync(cancellationToken);
	}
}
