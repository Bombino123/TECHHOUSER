using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Common.EntitySql;
using System.Data.Entity.Core.Common.QueryCache;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects.Internal;
using System.Data.Entity.Utilities;
using System.Linq.Expressions;

namespace System.Data.Entity.Core.Objects;

internal sealed class EntitySqlQueryState : ObjectQueryState
{
	private readonly string _queryText;

	private readonly DbExpression _queryExpression;

	private readonly bool _allowsLimit;

	private readonly ObjectQueryExecutionPlanFactory _objectQueryExecutionPlanFactory;

	internal bool AllowsLimitSubclause => _allowsLimit;

	internal EntitySqlQueryState(Type elementType, string commandText, bool allowsLimit, ObjectContext context, ObjectParameterCollection parameters, Span span)
		: this(elementType, commandText, null, allowsLimit, context, parameters, span)
	{
	}

	internal EntitySqlQueryState(Type elementType, string commandText, DbExpression expression, bool allowsLimit, ObjectContext context, ObjectParameterCollection parameters, Span span, ObjectQueryExecutionPlanFactory objectQueryExecutionPlanFactory = null)
		: base(elementType, context, parameters, span)
	{
		Check.NotEmpty(commandText, "commandText");
		_queryText = commandText;
		_queryExpression = expression;
		_allowsLimit = allowsLimit;
		_objectQueryExecutionPlanFactory = objectQueryExecutionPlanFactory ?? new ObjectQueryExecutionPlanFactory();
	}

	internal override bool TryGetCommandText(out string commandText)
	{
		commandText = _queryText;
		return true;
	}

	internal override bool TryGetExpression(out Expression expression)
	{
		expression = null;
		return false;
	}

	protected override TypeUsage GetResultType()
	{
		return Parse().ResultType;
	}

	internal override ObjectQueryState Include<TElementType>(ObjectQuery<TElementType> sourceQuery, string includePath)
	{
		ObjectQueryState objectQueryState = new EntitySqlQueryState(base.ElementType, _queryText, _queryExpression, _allowsLimit, base.ObjectContext, ObjectParameterCollection.DeepCopy(base.Parameters), Span.IncludeIn(base.Span, includePath));
		ApplySettingsTo(objectQueryState);
		return objectQueryState;
	}

	internal override ObjectQueryExecutionPlan GetExecutionPlan(MergeOption? forMergeOption)
	{
		MergeOption mergeOption = ObjectQueryState.EnsureMergeOption(forMergeOption, base.UserSpecifiedMergeOption);
		ObjectQueryExecutionPlan objectQueryExecutionPlan = _cachedPlan;
		if (objectQueryExecutionPlan != null)
		{
			if (objectQueryExecutionPlan.MergeOption == mergeOption && objectQueryExecutionPlan.Streaming == base.EffectiveStreamingBehavior)
			{
				return objectQueryExecutionPlan;
			}
			objectQueryExecutionPlan = null;
		}
		QueryCacheManager queryCacheManager = null;
		EntitySqlQueryCacheKey entitySqlQueryCacheKey = null;
		if (base.PlanCachingEnabled)
		{
			entitySqlQueryCacheKey = new EntitySqlQueryCacheKey(base.ObjectContext.DefaultContainerName, _queryText, (base.Parameters != null) ? base.Parameters.Count : 0, (base.Parameters == null) ? null : base.Parameters.GetCacheKey(), (base.Span == null) ? null : base.Span.GetCacheKey(), mergeOption, base.EffectiveStreamingBehavior, base.ElementType);
			queryCacheManager = base.ObjectContext.MetadataWorkspace.GetQueryCacheManager();
			ObjectQueryExecutionPlan value = null;
			if (queryCacheManager.TryCacheLookup<EntitySqlQueryCacheKey, ObjectQueryExecutionPlan>(entitySqlQueryCacheKey, out value))
			{
				objectQueryExecutionPlan = value;
			}
		}
		if (objectQueryExecutionPlan == null)
		{
			DbExpression query = Parse();
			DbQueryCommandTree tree = DbQueryCommandTree.FromValidExpression(base.ObjectContext.MetadataWorkspace, DataSpace.CSpace, query, useDatabaseNullSemantics: true, disableFilterOverProjectionSimplificationForCustomFunctions: false);
			objectQueryExecutionPlan = _objectQueryExecutionPlanFactory.Prepare(base.ObjectContext, tree, base.ElementType, mergeOption, base.EffectiveStreamingBehavior, base.Span, null, DbExpressionBuilder.AliasGenerator);
			if (entitySqlQueryCacheKey != null)
			{
				QueryCacheEntry inQueryCacheEntry = new QueryCacheEntry(entitySqlQueryCacheKey, objectQueryExecutionPlan);
				QueryCacheEntry outQueryCacheEntry = null;
				if (queryCacheManager.TryLookupAndAdd(inQueryCacheEntry, out outQueryCacheEntry))
				{
					objectQueryExecutionPlan = (ObjectQueryExecutionPlan)outQueryCacheEntry.GetTarget();
				}
			}
		}
		if (base.Parameters != null)
		{
			base.Parameters.SetReadOnly(isReadOnly: true);
		}
		_cachedPlan = objectQueryExecutionPlan;
		return objectQueryExecutionPlan;
	}

	internal DbExpression Parse()
	{
		if (_queryExpression != null)
		{
			return _queryExpression;
		}
		List<DbParameterReferenceExpression> list = null;
		if (base.Parameters != null)
		{
			list = new List<DbParameterReferenceExpression>(base.Parameters.Count);
			foreach (ObjectParameter parameter in base.Parameters)
			{
				TypeUsage typeUsage = parameter.TypeUsage;
				if (typeUsage == null)
				{
					base.ObjectContext.Perspective.TryGetTypeByName(parameter.MappableType.FullNameWithNesting(), ignoreCase: false, out typeUsage);
				}
				list.Add(typeUsage.Parameter(parameter.Name));
			}
		}
		return CqlQuery.CompileQueryCommandLambda(_queryText, base.ObjectContext.Perspective, null, list, null).Body;
	}
}
