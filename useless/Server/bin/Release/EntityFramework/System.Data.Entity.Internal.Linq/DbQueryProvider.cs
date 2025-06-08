using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Internal.Linq;

internal class DbQueryProvider : IQueryProvider, IDbAsyncQueryProvider
{
	private readonly InternalContext _internalContext;

	private readonly IInternalQuery _internalQuery;

	public InternalContext InternalContext => _internalContext;

	public DbQueryProvider(InternalContext internalContext, IInternalQuery internalQuery)
	{
		_internalContext = internalContext;
		_internalQuery = internalQuery;
	}

	public virtual IQueryable<TElement> CreateQuery<TElement>(Expression expression)
	{
		Check.NotNull(expression, "expression");
		ObjectQuery objectQuery = CreateObjectQuery(expression);
		if (typeof(TElement) != ((IQueryable)objectQuery).ElementType)
		{
			return (IQueryable<TElement>)CreateQuery(objectQuery);
		}
		return new DbQuery<TElement>(new InternalQuery<TElement>(_internalContext, objectQuery));
	}

	public virtual IQueryable CreateQuery(Expression expression)
	{
		Check.NotNull(expression, "expression");
		return CreateQuery(CreateObjectQuery(expression));
	}

	public virtual TResult Execute<TResult>(Expression expression)
	{
		Check.NotNull(expression, "expression");
		_internalContext.Initialize();
		return ((IQueryProvider)_internalQuery.ObjectQueryProvider).Execute<TResult>(expression);
	}

	public virtual object Execute(Expression expression)
	{
		Check.NotNull(expression, "expression");
		_internalContext.Initialize();
		return ((IQueryProvider)_internalQuery.ObjectQueryProvider).Execute(expression);
	}

	Task<TResult> IDbAsyncQueryProvider.ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
	{
		Check.NotNull(expression, "expression");
		cancellationToken.ThrowIfCancellationRequested();
		_internalContext.Initialize();
		return ((IDbAsyncQueryProvider)_internalQuery.ObjectQueryProvider).ExecuteAsync<TResult>(expression, cancellationToken);
	}

	Task<object> IDbAsyncQueryProvider.ExecuteAsync(Expression expression, CancellationToken cancellationToken)
	{
		Check.NotNull(expression, "expression");
		cancellationToken.ThrowIfCancellationRequested();
		_internalContext.Initialize();
		return ((IDbAsyncQueryProvider)_internalQuery.ObjectQueryProvider).ExecuteAsync(expression, cancellationToken);
	}

	private IQueryable CreateQuery(ObjectQuery objectQuery)
	{
		IInternalQuery internalQuery = CreateInternalQuery(objectQuery);
		return (IQueryable)typeof(DbQuery<>).MakeGenericType(internalQuery.ElementType).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).Single()
			.Invoke(new object[1] { internalQuery });
	}

	protected ObjectQuery CreateObjectQuery(Expression expression)
	{
		expression = new DbQueryVisitor().Visit(expression);
		return (ObjectQuery)((IQueryProvider)_internalQuery.ObjectQueryProvider).CreateQuery(expression);
	}

	protected IInternalQuery CreateInternalQuery(ObjectQuery objectQuery)
	{
		return (IInternalQuery)typeof(InternalQuery<>).MakeGenericType(((IQueryable)objectQuery).ElementType).GetDeclaredConstructor(typeof(InternalContext), typeof(ObjectQuery)).Invoke(new object[2] { _internalContext, objectQuery });
	}
}
