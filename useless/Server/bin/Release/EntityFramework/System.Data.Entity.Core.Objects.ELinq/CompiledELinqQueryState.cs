using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.QueryCache;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects.Internal;
using System.Linq;
using System.Linq.Expressions;

namespace System.Data.Entity.Core.Objects.ELinq;

internal sealed class CompiledELinqQueryState : ELinqQueryState
{
	private sealed class CreateDonateableExpressionVisitor : EntityExpressionVisitor
	{
		private readonly Dictionary<ParameterExpression, object> _parameterToValueLookup;

		private CreateDonateableExpressionVisitor(Dictionary<ParameterExpression, object> parameterToValueLookup)
		{
			_parameterToValueLookup = parameterToValueLookup;
		}

		internal static Expression Replace(LambdaExpression query, ObjectContext objectContext, object[] parameterValues)
		{
			Dictionary<ParameterExpression, object> dictionary = query.Parameters.Skip(1).Zip(parameterValues).ToDictionary((KeyValuePair<ParameterExpression, object> pair) => pair.Key, (KeyValuePair<ParameterExpression, object> pair) => pair.Value);
			dictionary.Add(query.Parameters.First(), objectContext);
			return new CreateDonateableExpressionVisitor(dictionary).Visit(query.Body);
		}

		internal override Expression VisitParameter(ParameterExpression p)
		{
			if (_parameterToValueLookup.TryGetValue(p, out var value))
			{
				return Expression.Constant(value, p.Type);
			}
			return base.VisitParameter(p);
		}
	}

	private readonly Guid _cacheToken;

	private readonly object[] _parameterValues;

	private CompiledQueryCacheEntry _cacheEntry;

	private readonly ObjectQueryExecutionPlanFactory _objectQueryExecutionPlanFactory;

	internal override Expression Expression => CreateDonateableExpressionVisitor.Replace((LambdaExpression)base.Expression, base.ObjectContext, _parameterValues);

	internal CompiledELinqQueryState(Type elementType, ObjectContext context, LambdaExpression lambda, Guid cacheToken, object[] parameterValues, ObjectQueryExecutionPlanFactory objectQueryExecutionPlanFactory = null)
		: base(elementType, context, lambda)
	{
		_cacheToken = cacheToken;
		_parameterValues = parameterValues;
		EnsureParameters();
		base.Parameters.SetReadOnly(isReadOnly: true);
		_objectQueryExecutionPlanFactory = objectQueryExecutionPlanFactory ?? new ObjectQueryExecutionPlanFactory();
	}

	internal override ObjectQueryExecutionPlan GetExecutionPlan(MergeOption? forMergeOption)
	{
		ObjectQueryExecutionPlan objectQueryExecutionPlan = null;
		CompiledQueryCacheEntry value = _cacheEntry;
		bool useCSharpNullComparisonBehavior = base.ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior;
		bool disableFilterOverProjectionSimplificationForCustomFunctions = base.ObjectContext.ContextOptions.DisableFilterOverProjectionSimplificationForCustomFunctions;
		if (value != null)
		{
			MergeOption mergeOption = ObjectQueryState.EnsureMergeOption(forMergeOption, base.UserSpecifiedMergeOption, value.PropagatedMergeOption);
			objectQueryExecutionPlan = value.GetExecutionPlan(mergeOption, useCSharpNullComparisonBehavior);
			if (objectQueryExecutionPlan == null)
			{
				ExpressionConverter expressionConverter = CreateExpressionConverter();
				DbExpression query = expressionConverter.Convert();
				IEnumerable<Tuple<ObjectParameter, QueryParameterExpression>> parameters = expressionConverter.GetParameters();
				DbQueryCommandTree tree = DbQueryCommandTree.FromValidExpression(base.ObjectContext.MetadataWorkspace, DataSpace.CSpace, query, !useCSharpNullComparisonBehavior, disableFilterOverProjectionSimplificationForCustomFunctions);
				objectQueryExecutionPlan = _objectQueryExecutionPlanFactory.Prepare(base.ObjectContext, tree, base.ElementType, mergeOption, base.EffectiveStreamingBehavior, expressionConverter.PropagatedSpan, parameters, expressionConverter.AliasGenerator);
				objectQueryExecutionPlan = value.SetExecutionPlan(objectQueryExecutionPlan, useCSharpNullComparisonBehavior);
			}
		}
		else
		{
			QueryCacheManager queryCacheManager = base.ObjectContext.MetadataWorkspace.GetQueryCacheManager();
			CompiledQueryCacheKey compiledQueryCacheKey = new CompiledQueryCacheKey(_cacheToken);
			if (queryCacheManager.TryCacheLookup<CompiledQueryCacheKey, CompiledQueryCacheEntry>(compiledQueryCacheKey, out value))
			{
				_cacheEntry = value;
				MergeOption mergeOption2 = ObjectQueryState.EnsureMergeOption(forMergeOption, base.UserSpecifiedMergeOption, value.PropagatedMergeOption);
				objectQueryExecutionPlan = value.GetExecutionPlan(mergeOption2, useCSharpNullComparisonBehavior);
			}
			if (objectQueryExecutionPlan == null)
			{
				ExpressionConverter expressionConverter2 = CreateExpressionConverter();
				DbExpression query2 = expressionConverter2.Convert();
				IEnumerable<Tuple<ObjectParameter, QueryParameterExpression>> parameters2 = expressionConverter2.GetParameters();
				DbQueryCommandTree tree2 = DbQueryCommandTree.FromValidExpression(base.ObjectContext.MetadataWorkspace, DataSpace.CSpace, query2, !useCSharpNullComparisonBehavior, disableFilterOverProjectionSimplificationForCustomFunctions);
				if (value == null)
				{
					value = new CompiledQueryCacheEntry(compiledQueryCacheKey, expressionConverter2.PropagatedMergeOption);
					if (queryCacheManager.TryLookupAndAdd(value, out var outQueryCacheEntry))
					{
						value = (CompiledQueryCacheEntry)outQueryCacheEntry;
					}
					_cacheEntry = value;
				}
				MergeOption mergeOption3 = ObjectQueryState.EnsureMergeOption(forMergeOption, base.UserSpecifiedMergeOption, value.PropagatedMergeOption);
				objectQueryExecutionPlan = value.GetExecutionPlan(mergeOption3, useCSharpNullComparisonBehavior);
				if (objectQueryExecutionPlan == null)
				{
					objectQueryExecutionPlan = _objectQueryExecutionPlanFactory.Prepare(base.ObjectContext, tree2, base.ElementType, mergeOption3, base.EffectiveStreamingBehavior, expressionConverter2.PropagatedSpan, parameters2, expressionConverter2.AliasGenerator);
					objectQueryExecutionPlan = value.SetExecutionPlan(objectQueryExecutionPlan, useCSharpNullComparisonBehavior);
				}
			}
		}
		ObjectParameterCollection objectParameterCollection = EnsureParameters();
		if (objectQueryExecutionPlan.CompiledQueryParameters != null && objectQueryExecutionPlan.CompiledQueryParameters.Any())
		{
			objectParameterCollection.SetReadOnly(isReadOnly: false);
			objectParameterCollection.Clear();
			foreach (Tuple<ObjectParameter, QueryParameterExpression> compiledQueryParameter in objectQueryExecutionPlan.CompiledQueryParameters)
			{
				ObjectParameter objectParameter = compiledQueryParameter.Item1.ShallowCopy();
				QueryParameterExpression item = compiledQueryParameter.Item2;
				objectParameterCollection.Add(objectParameter);
				if (item != null)
				{
					objectParameter.Value = item.EvaluateParameter(_parameterValues);
				}
			}
		}
		objectParameterCollection.SetReadOnly(isReadOnly: true);
		return objectQueryExecutionPlan;
	}

	protected override TypeUsage GetResultType()
	{
		CompiledQueryCacheEntry cacheEntry = _cacheEntry;
		if (cacheEntry != null && cacheEntry.TryGetResultType(out var resultType))
		{
			return resultType;
		}
		return base.GetResultType();
	}

	protected override ExpressionConverter CreateExpressionConverter()
	{
		LambdaExpression lambdaExpression = (LambdaExpression)base.Expression;
		return new ExpressionConverter(Funcletizer.CreateCompiledQueryEvaluationFuncletizer(base.ObjectContext, lambdaExpression.Parameters.First(), new ReadOnlyCollection<ParameterExpression>(lambdaExpression.Parameters.Skip(1).ToList())), lambdaExpression.Body);
	}
}
