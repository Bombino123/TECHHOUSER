using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects.ELinq;
using System.Data.Entity.Core.Objects.Internal;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Internal;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Core.Objects;

public abstract class ObjectQuery : IEnumerable, IOrderedQueryable, IQueryable, IListSource, IDbAsyncEnumerable
{
	private readonly ObjectQueryState _state;

	private TypeUsage _resultType;

	private ObjectQueryProvider _provider;

	internal ObjectQueryState QueryState => _state;

	internal virtual ObjectQueryProvider ObjectQueryProvider
	{
		get
		{
			if (_provider == null)
			{
				_provider = new ObjectQueryProvider(this);
			}
			return _provider;
		}
	}

	internal IDbExecutionStrategy ExecutionStrategy
	{
		get
		{
			return QueryState.ExecutionStrategy;
		}
		set
		{
			QueryState.ExecutionStrategy = value;
		}
	}

	bool IListSource.ContainsListCollection => false;

	public string CommandText
	{
		get
		{
			if (!_state.TryGetCommandText(out var commandText))
			{
				return string.Empty;
			}
			return commandText;
		}
	}

	public ObjectContext Context => _state.ObjectContext;

	public MergeOption MergeOption
	{
		get
		{
			return _state.EffectiveMergeOption;
		}
		set
		{
			EntityUtil.CheckArgumentMergeOption(value);
			_state.UserSpecifiedMergeOption = value;
		}
	}

	public bool Streaming
	{
		get
		{
			return _state.EffectiveStreamingBehavior;
		}
		set
		{
			_state.UserSpecifiedStreamingBehavior = value;
		}
	}

	public ObjectParameterCollection Parameters => _state.EnsureParameters();

	public bool EnablePlanCaching
	{
		get
		{
			return _state.PlanCachingEnabled;
		}
		set
		{
			_state.PlanCachingEnabled = value;
		}
	}

	Type IQueryable.ElementType => _state.ElementType;

	Expression IQueryable.Expression => GetExpression();

	IQueryProvider IQueryable.Provider => ObjectQueryProvider;

	internal ObjectQuery(ObjectQueryState queryState)
	{
		_state = queryState;
	}

	internal ObjectQuery()
	{
	}

	[Browsable(false)]
	public string ToTraceString()
	{
		return _state.GetExecutionPlan(null).ToTraceString();
	}

	public TypeUsage GetResultType()
	{
		if (_resultType == null)
		{
			TypeUsage resultType = _state.ResultType;
			if (!TypeHelpers.TryGetCollectionElementType(resultType, out var elementType))
			{
				elementType = resultType;
			}
			elementType = _state.ObjectContext.Perspective.MetadataWorkspace.GetOSpaceTypeUsage(elementType);
			if (elementType == null)
			{
				throw new InvalidOperationException(Strings.ObjectQuery_UnableToMapResultType);
			}
			_resultType = elementType;
		}
		return _resultType;
	}

	public ObjectResult Execute(MergeOption mergeOption)
	{
		EntityUtil.CheckArgumentMergeOption(mergeOption);
		return ExecuteInternal(mergeOption);
	}

	public Task<ObjectResult> ExecuteAsync(MergeOption mergeOption)
	{
		return ExecuteAsync(mergeOption, CancellationToken.None);
	}

	public Task<ObjectResult> ExecuteAsync(MergeOption mergeOption, CancellationToken cancellationToken)
	{
		EntityUtil.CheckArgumentMergeOption(mergeOption);
		cancellationToken.ThrowIfCancellationRequested();
		return ExecuteInternalAsync(mergeOption, cancellationToken);
	}

	IList IListSource.GetList()
	{
		return GetIListSourceListInternal();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumeratorInternal();
	}

	IDbAsyncEnumerator IDbAsyncEnumerable.GetAsyncEnumerator()
	{
		return GetAsyncEnumeratorInternal();
	}

	internal abstract Expression GetExpression();

	internal abstract IEnumerator GetEnumeratorInternal();

	internal abstract IDbAsyncEnumerator GetAsyncEnumeratorInternal();

	internal abstract Task<ObjectResult> ExecuteInternalAsync(MergeOption mergeOption, CancellationToken cancellationToken);

	internal abstract IList GetIListSourceListInternal();

	internal abstract ObjectResult ExecuteInternal(MergeOption mergeOption);
}
public class ObjectQuery<T> : ObjectQuery, IOrderedQueryable<T>, IQueryable<T>, IEnumerable<T>, IEnumerable, IQueryable, IOrderedQueryable, IDbAsyncEnumerable<T>, IDbAsyncEnumerable
{
	internal static readonly MethodInfo MergeAsMethod = typeof(ObjectQuery<T>).GetOnlyDeclaredMethod("MergeAs");

	internal static readonly MethodInfo IncludeSpanMethod = typeof(ObjectQuery<T>).GetOnlyDeclaredMethod("IncludeSpan");

	private const string DefaultName = "it";

	private string _name = "it";

	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			Check.NotNull(value, "value");
			if (!ObjectParameter.ValidateParameterName(value))
			{
				throw new ArgumentException(Strings.ObjectQuery_InvalidQueryName(value), "value");
			}
			_name = value;
		}
	}

	private static bool IsLinqQuery(ObjectQuery query)
	{
		return query.QueryState is ELinqQueryState;
	}

	public ObjectQuery(string commandText, ObjectContext context)
		: this((ObjectQueryState)new EntitySqlQueryState(typeof(T), commandText, allowsLimit: false, context, null, null))
	{
		context.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(T), Assembly.GetCallingAssembly());
	}

	public ObjectQuery(string commandText, ObjectContext context, MergeOption mergeOption)
		: this((ObjectQueryState)new EntitySqlQueryState(typeof(T), commandText, allowsLimit: false, context, null, null))
	{
		EntityUtil.CheckArgumentMergeOption(mergeOption);
		base.QueryState.UserSpecifiedMergeOption = mergeOption;
		context.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(T), Assembly.GetCallingAssembly());
	}

	internal ObjectQuery(EntitySetBase entitySet, ObjectContext context, MergeOption mergeOption)
		: this((ObjectQueryState)new EntitySqlQueryState(typeof(T), BuildScanEntitySetEsql(entitySet), entitySet.Scan(), allowsLimit: false, context, null, null))
	{
		EntityUtil.CheckArgumentMergeOption(mergeOption);
		base.QueryState.UserSpecifiedMergeOption = mergeOption;
		context.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(T), Assembly.GetCallingAssembly());
	}

	private static string BuildScanEntitySetEsql(EntitySetBase entitySet)
	{
		return string.Format(CultureInfo.InvariantCulture, "{0}.{1}", new object[2]
		{
			EntityUtil.QuoteIdentifier(entitySet.EntityContainer.Name),
			EntityUtil.QuoteIdentifier(entitySet.Name)
		});
	}

	internal ObjectQuery(ObjectQueryState queryState)
		: base(queryState)
	{
	}

	internal ObjectQuery()
	{
	}

	public new ObjectResult<T> Execute(MergeOption mergeOption)
	{
		EntityUtil.CheckArgumentMergeOption(mergeOption);
		return GetResults(mergeOption);
	}

	public new Task<ObjectResult<T>> ExecuteAsync(MergeOption mergeOption)
	{
		return ExecuteAsync(mergeOption, CancellationToken.None);
	}

	public new Task<ObjectResult<T>> ExecuteAsync(MergeOption mergeOption, CancellationToken cancellationToken)
	{
		EntityUtil.CheckArgumentMergeOption(mergeOption);
		return GetResultsAsync(mergeOption, cancellationToken);
	}

	public ObjectQuery<T> Include(string path)
	{
		Check.NotEmpty(path, "path");
		return new ObjectQuery<T>(base.QueryState.Include(this, path));
	}

	public ObjectQuery<T> Distinct()
	{
		if (IsLinqQuery(this))
		{
			return (ObjectQuery<T>)Queryable.Distinct(this);
		}
		return new ObjectQuery<T>(EntitySqlQueryBuilder.Distinct(base.QueryState));
	}

	public ObjectQuery<T> Except(ObjectQuery<T> query)
	{
		Check.NotNull(query, "query");
		if (IsLinqQuery(this) || IsLinqQuery(query))
		{
			return (ObjectQuery<T>)Queryable.Except(this, query);
		}
		return new ObjectQuery<T>(EntitySqlQueryBuilder.Except(base.QueryState, query.QueryState));
	}

	public ObjectQuery<DbDataRecord> GroupBy(string keys, string projection, params ObjectParameter[] parameters)
	{
		Check.NotEmpty(keys, "keys");
		Check.NotEmpty(projection, "projection");
		Check.NotNull(parameters, "parameters");
		return new ObjectQuery<DbDataRecord>(EntitySqlQueryBuilder.GroupBy(base.QueryState, Name, keys, projection, parameters));
	}

	public ObjectQuery<T> Intersect(ObjectQuery<T> query)
	{
		Check.NotNull(query, "query");
		if (IsLinqQuery(this) || IsLinqQuery(query))
		{
			return (ObjectQuery<T>)Queryable.Intersect(this, query);
		}
		return new ObjectQuery<T>(EntitySqlQueryBuilder.Intersect(base.QueryState, query.QueryState));
	}

	public ObjectQuery<TResultType> OfType<TResultType>()
	{
		if (IsLinqQuery(this))
		{
			return (ObjectQuery<TResultType>)Queryable.OfType<TResultType>(this);
		}
		base.QueryState.ObjectContext.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TResultType), Assembly.GetCallingAssembly());
		Type typeFromHandle = typeof(TResultType);
		if (!base.QueryState.ObjectContext.MetadataWorkspace.GetItemCollection(DataSpace.OSpace).TryGetType(typeFromHandle.Name, typeFromHandle.NestingNamespace() ?? string.Empty, out var type) || (!Helper.IsEntityType(type) && !Helper.IsComplexType(type)))
		{
			throw new EntitySqlException(Strings.ObjectQuery_QueryBuilder_InvalidResultType(typeof(TResultType).FullName));
		}
		return new ObjectQuery<TResultType>(EntitySqlQueryBuilder.OfType(base.QueryState, type, typeFromHandle));
	}

	public ObjectQuery<T> OrderBy(string keys, params ObjectParameter[] parameters)
	{
		Check.NotEmpty(keys, "keys");
		Check.NotNull(parameters, "parameters");
		return new ObjectQuery<T>(EntitySqlQueryBuilder.OrderBy(base.QueryState, Name, keys, parameters));
	}

	public ObjectQuery<DbDataRecord> Select(string projection, params ObjectParameter[] parameters)
	{
		Check.NotEmpty(projection, "projection");
		Check.NotNull(parameters, "parameters");
		return new ObjectQuery<DbDataRecord>(EntitySqlQueryBuilder.Select(base.QueryState, Name, projection, parameters));
	}

	public ObjectQuery<TResultType> SelectValue<TResultType>(string projection, params ObjectParameter[] parameters)
	{
		Check.NotEmpty(projection, "projection");
		Check.NotNull(parameters, "parameters");
		base.QueryState.ObjectContext.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TResultType), Assembly.GetCallingAssembly());
		return new ObjectQuery<TResultType>(EntitySqlQueryBuilder.SelectValue(base.QueryState, Name, projection, parameters, typeof(TResultType)));
	}

	public ObjectQuery<T> Skip(string keys, string count, params ObjectParameter[] parameters)
	{
		Check.NotEmpty(keys, "keys");
		Check.NotEmpty(count, "count");
		Check.NotNull(parameters, "parameters");
		return new ObjectQuery<T>(EntitySqlQueryBuilder.Skip(base.QueryState, Name, keys, count, parameters));
	}

	public ObjectQuery<T> Top(string count, params ObjectParameter[] parameters)
	{
		Check.NotEmpty(count, "count");
		return new ObjectQuery<T>(EntitySqlQueryBuilder.Top(base.QueryState, Name, count, parameters));
	}

	public ObjectQuery<T> Union(ObjectQuery<T> query)
	{
		Check.NotNull(query, "query");
		if (IsLinqQuery(this) || IsLinqQuery(query))
		{
			return (ObjectQuery<T>)Queryable.Union(this, query);
		}
		return new ObjectQuery<T>(EntitySqlQueryBuilder.Union(base.QueryState, query.QueryState));
	}

	public ObjectQuery<T> UnionAll(ObjectQuery<T> query)
	{
		Check.NotNull(query, "query");
		return new ObjectQuery<T>(EntitySqlQueryBuilder.UnionAll(base.QueryState, query.QueryState));
	}

	public ObjectQuery<T> Where(string predicate, params ObjectParameter[] parameters)
	{
		Check.NotEmpty(predicate, "predicate");
		Check.NotNull(parameters, "parameters");
		return new ObjectQuery<T>(EntitySqlQueryBuilder.Where(base.QueryState, Name, predicate, parameters));
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		base.QueryState.ObjectContext.AsyncMonitor.EnsureNotEntered();
		return new LazyEnumerator<T>(() => GetResults(null));
	}

	IDbAsyncEnumerator<T> IDbAsyncEnumerable<T>.GetAsyncEnumerator()
	{
		base.QueryState.ObjectContext.AsyncMonitor.EnsureNotEntered();
		return new LazyAsyncEnumerator<T>((CancellationToken cancellationToken) => GetResultsAsync(null, cancellationToken));
	}

	internal override IEnumerator GetEnumeratorInternal()
	{
		return ((IEnumerable<T>)this).GetEnumerator();
	}

	internal override IDbAsyncEnumerator GetAsyncEnumeratorInternal()
	{
		return ((IDbAsyncEnumerable<T>)this).GetAsyncEnumerator();
	}

	internal override IList GetIListSourceListInternal()
	{
		return ((IListSource)GetResults(null)).GetList();
	}

	internal override ObjectResult ExecuteInternal(MergeOption mergeOption)
	{
		return GetResults(mergeOption);
	}

	internal override async Task<ObjectResult> ExecuteInternalAsync(MergeOption mergeOption, CancellationToken cancellationToken)
	{
		return await GetResultsAsync(mergeOption, cancellationToken).WithCurrentCulture();
	}

	internal override Expression GetExpression()
	{
		if (!base.QueryState.TryGetExpression(out var expression))
		{
			expression = Expression.Constant(this);
		}
		if (base.QueryState.UserSpecifiedMergeOption.HasValue)
		{
			expression = TypeSystem.EnsureType(expression, typeof(ObjectQuery<T>));
			expression = Expression.Call(expression, MergeAsMethod, Expression.Constant(base.QueryState.UserSpecifiedMergeOption.Value));
		}
		if (base.QueryState.Span != null)
		{
			expression = TypeSystem.EnsureType(expression, typeof(ObjectQuery<T>));
			expression = Expression.Call(expression, IncludeSpanMethod, Expression.Constant(base.QueryState.Span));
		}
		return expression;
	}

	internal ObjectQuery<T> MergeAs(MergeOption mergeOption)
	{
		throw new InvalidOperationException(Strings.ELinq_MethodNotDirectlyCallable);
	}

	internal ObjectQuery<T> IncludeSpan(Span span)
	{
		throw new InvalidOperationException(Strings.ELinq_MethodNotDirectlyCallable);
	}

	private ObjectResult<T> GetResults(MergeOption? forMergeOption)
	{
		base.QueryState.ObjectContext.AsyncMonitor.EnsureNotEntered();
		IDbExecutionStrategy executionStrategy = base.ExecutionStrategy ?? DbProviderServices.GetExecutionStrategy(base.QueryState.ObjectContext.Connection, base.QueryState.ObjectContext.MetadataWorkspace);
		if (executionStrategy.RetriesOnFailure && base.QueryState.EffectiveStreamingBehavior)
		{
			throw new InvalidOperationException(Strings.ExecutionStrategy_StreamingNotSupported(executionStrategy.GetType().Name));
		}
		return executionStrategy.Execute(() => base.QueryState.ObjectContext.ExecuteInTransaction(() => base.QueryState.GetExecutionPlan(forMergeOption).Execute<T>(base.QueryState.ObjectContext, base.QueryState.Parameters), executionStrategy, startLocalTransaction: false, !base.QueryState.EffectiveStreamingBehavior));
	}

	private Task<ObjectResult<T>> GetResultsAsync(MergeOption? forMergeOption, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		base.QueryState.ObjectContext.AsyncMonitor.EnsureNotEntered();
		IDbExecutionStrategy dbExecutionStrategy = base.ExecutionStrategy ?? DbProviderServices.GetExecutionStrategy(base.QueryState.ObjectContext.Connection, base.QueryState.ObjectContext.MetadataWorkspace);
		if (dbExecutionStrategy.RetriesOnFailure && base.QueryState.EffectiveStreamingBehavior)
		{
			throw new InvalidOperationException(Strings.ExecutionStrategy_StreamingNotSupported(dbExecutionStrategy.GetType().Name));
		}
		return GetResultsAsync(forMergeOption, dbExecutionStrategy, cancellationToken);
	}

	private async Task<ObjectResult<T>> GetResultsAsync(MergeOption? forMergeOption, IDbExecutionStrategy executionStrategy, CancellationToken cancellationToken)
	{
		MergeOption mergeOption = (forMergeOption.HasValue ? forMergeOption.Value : base.QueryState.EffectiveMergeOption);
		if (mergeOption != MergeOption.NoTracking)
		{
			base.QueryState.ObjectContext.AsyncMonitor.Enter();
		}
		try
		{
			return await executionStrategy.ExecuteAsync(() => base.QueryState.ObjectContext.ExecuteInTransactionAsync(() => base.QueryState.GetExecutionPlan(forMergeOption).ExecuteAsync<T>(base.QueryState.ObjectContext, base.QueryState.Parameters, cancellationToken), executionStrategy, startLocalTransaction: false, !base.QueryState.EffectiveStreamingBehavior, cancellationToken), cancellationToken).WithCurrentCulture();
		}
		finally
		{
			if (mergeOption != MergeOption.NoTracking)
			{
				base.QueryState.ObjectContext.AsyncMonitor.Exit();
			}
		}
	}
}
