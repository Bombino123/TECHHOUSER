using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.Internal;
using System.Data.Entity.Core.Common.QueryCache;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects.Internal;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Data.Entity.Core.Objects.ELinq;

internal class ELinqQueryState : ObjectQueryState
{
	private readonly Expression _expression;

	private Func<bool> _recompileRequired;

	private IEnumerable<Tuple<ObjectParameter, QueryParameterExpression>> _linqParameters;

	private bool _useCSharpNullComparisonBehavior;

	private bool _disableFilterOverProjectionSimplificationForCustomFunctions;

	private readonly ObjectQueryExecutionPlanFactory _objectQueryExecutionPlanFactory;

	internal virtual Expression Expression => _expression;

	internal ELinqQueryState(Type elementType, ObjectContext context, Expression expression, ObjectQueryExecutionPlanFactory objectQueryExecutionPlanFactory = null)
		: base(elementType, context, null, null)
	{
		_expression = expression;
		_useCSharpNullComparisonBehavior = context.ContextOptions.UseCSharpNullComparisonBehavior;
		_disableFilterOverProjectionSimplificationForCustomFunctions = context.ContextOptions.DisableFilterOverProjectionSimplificationForCustomFunctions;
		_objectQueryExecutionPlanFactory = objectQueryExecutionPlanFactory ?? new ObjectQueryExecutionPlanFactory();
	}

	internal ELinqQueryState(Type elementType, ObjectQuery query, Expression expression, ObjectQueryExecutionPlanFactory objectQueryExecutionPlanFactory = null)
		: base(elementType, query)
	{
		_expression = expression;
		_objectQueryExecutionPlanFactory = objectQueryExecutionPlanFactory ?? new ObjectQueryExecutionPlanFactory();
	}

	protected override TypeUsage GetResultType()
	{
		return CreateExpressionConverter().Convert().ResultType;
	}

	internal override ObjectQueryExecutionPlan GetExecutionPlan(MergeOption? forMergeOption)
	{
		ObjectQueryExecutionPlan objectQueryExecutionPlan = _cachedPlan;
		if (objectQueryExecutionPlan != null)
		{
			MergeOption? mergeOption = ObjectQueryState.GetMergeOption(forMergeOption, base.UserSpecifiedMergeOption);
			if ((mergeOption.HasValue && mergeOption.Value != objectQueryExecutionPlan.MergeOption) || _recompileRequired() || base.ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior != _useCSharpNullComparisonBehavior || base.ObjectContext.ContextOptions.DisableFilterOverProjectionSimplificationForCustomFunctions != _disableFilterOverProjectionSimplificationForCustomFunctions)
			{
				objectQueryExecutionPlan = null;
			}
		}
		if (objectQueryExecutionPlan == null)
		{
			_recompileRequired = null;
			ResetParameters();
			ExpressionConverter expressionConverter = CreateExpressionConverter();
			DbExpression dbExpression = expressionConverter.Convert();
			_recompileRequired = expressionConverter.RecompileRequired;
			MergeOption mergeOption2 = ObjectQueryState.EnsureMergeOption(forMergeOption, base.UserSpecifiedMergeOption, expressionConverter.PropagatedMergeOption);
			_useCSharpNullComparisonBehavior = base.ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior;
			_disableFilterOverProjectionSimplificationForCustomFunctions = base.ObjectContext.ContextOptions.DisableFilterOverProjectionSimplificationForCustomFunctions;
			_linqParameters = expressionConverter.GetParameters();
			if (_linqParameters != null && _linqParameters.Any())
			{
				ObjectParameterCollection objectParameterCollection = EnsureParameters();
				objectParameterCollection.SetReadOnly(isReadOnly: false);
				foreach (Tuple<ObjectParameter, QueryParameterExpression> linqParameter in _linqParameters)
				{
					ObjectParameter item = linqParameter.Item1;
					objectParameterCollection.Add(item);
				}
				objectParameterCollection.SetReadOnly(isReadOnly: true);
			}
			QueryCacheManager queryCacheManager = null;
			LinqQueryCacheKey linqQueryCacheKey = null;
			if (base.PlanCachingEnabled && !_recompileRequired() && ExpressionKeyGen.TryGenerateKey(dbExpression, out var key))
			{
				linqQueryCacheKey = new LinqQueryCacheKey(key, (base.Parameters != null) ? base.Parameters.Count : 0, (base.Parameters == null) ? null : base.Parameters.GetCacheKey(), (expressionConverter.PropagatedSpan == null) ? null : expressionConverter.PropagatedSpan.GetCacheKey(), mergeOption2, base.EffectiveStreamingBehavior, _useCSharpNullComparisonBehavior, base.ElementType);
				queryCacheManager = base.ObjectContext.MetadataWorkspace.GetQueryCacheManager();
				ObjectQueryExecutionPlan value = null;
				if (queryCacheManager.TryCacheLookup<LinqQueryCacheKey, ObjectQueryExecutionPlan>(linqQueryCacheKey, out value))
				{
					objectQueryExecutionPlan = value;
				}
			}
			if (objectQueryExecutionPlan == null)
			{
				DbQueryCommandTree tree = DbQueryCommandTree.FromValidExpression(base.ObjectContext.MetadataWorkspace, DataSpace.CSpace, dbExpression, !_useCSharpNullComparisonBehavior, _disableFilterOverProjectionSimplificationForCustomFunctions);
				objectQueryExecutionPlan = _objectQueryExecutionPlanFactory.Prepare(base.ObjectContext, tree, base.ElementType, mergeOption2, base.EffectiveStreamingBehavior, expressionConverter.PropagatedSpan, null, expressionConverter.AliasGenerator);
				if (linqQueryCacheKey != null)
				{
					QueryCacheEntry inQueryCacheEntry = new QueryCacheEntry(linqQueryCacheKey, objectQueryExecutionPlan);
					QueryCacheEntry outQueryCacheEntry = null;
					if (queryCacheManager.TryLookupAndAdd(inQueryCacheEntry, out outQueryCacheEntry))
					{
						objectQueryExecutionPlan = (ObjectQueryExecutionPlan)outQueryCacheEntry.GetTarget();
					}
				}
			}
			_cachedPlan = objectQueryExecutionPlan;
		}
		if (_linqParameters != null)
		{
			foreach (Tuple<ObjectParameter, QueryParameterExpression> linqParameter2 in _linqParameters)
			{
				ObjectParameter item2 = linqParameter2.Item1;
				QueryParameterExpression item3 = linqParameter2.Item2;
				if (item3 != null)
				{
					item2.Value = item3.EvaluateParameter(null);
				}
			}
		}
		return objectQueryExecutionPlan;
	}

	internal override ObjectQueryState Include<TElementType>(ObjectQuery<TElementType> sourceQuery, string includePath)
	{
		MethodInfo includeMethod = GetIncludeMethod(sourceQuery);
		Expression expression = Expression.Call(Expression.Constant(sourceQuery), includeMethod, Expression.Constant(includePath, typeof(string)));
		ObjectQueryState objectQueryState = new ELinqQueryState(base.ElementType, base.ObjectContext, expression);
		ApplySettingsTo(objectQueryState);
		return objectQueryState;
	}

	internal static MethodInfo GetIncludeMethod<TElementType>(ObjectQuery<TElementType> sourceQuery)
	{
		return sourceQuery.GetType().GetOnlyDeclaredMethod("Include");
	}

	internal override bool TryGetCommandText(out string commandText)
	{
		commandText = null;
		return false;
	}

	internal override bool TryGetExpression(out Expression expression)
	{
		expression = Expression;
		return true;
	}

	protected virtual ExpressionConverter CreateExpressionConverter()
	{
		return new ExpressionConverter(Funcletizer.CreateQueryFuncletizer(base.ObjectContext), _expression);
	}

	private void ResetParameters()
	{
		if (base.Parameters != null)
		{
			bool isReadOnly = ((ICollection<ObjectParameter>)base.Parameters).IsReadOnly;
			if (isReadOnly)
			{
				base.Parameters.SetReadOnly(isReadOnly: false);
			}
			base.Parameters.Clear();
			if (isReadOnly)
			{
				base.Parameters.SetReadOnly(isReadOnly: true);
			}
		}
		_linqParameters = null;
	}
}
