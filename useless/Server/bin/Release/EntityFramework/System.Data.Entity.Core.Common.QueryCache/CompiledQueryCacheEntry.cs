using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.Objects.Internal;

namespace System.Data.Entity.Core.Common.QueryCache;

internal sealed class CompiledQueryCacheEntry : QueryCacheEntry
{
	public readonly MergeOption? PropagatedMergeOption;

	private readonly ConcurrentDictionary<string, ObjectQueryExecutionPlan> _plans;

	internal CompiledQueryCacheEntry(QueryCacheKey queryCacheKey, MergeOption? mergeOption)
		: base(queryCacheKey, null)
	{
		PropagatedMergeOption = mergeOption;
		_plans = new ConcurrentDictionary<string, ObjectQueryExecutionPlan>();
	}

	internal ObjectQueryExecutionPlan GetExecutionPlan(MergeOption mergeOption, bool useCSharpNullComparisonBehavior)
	{
		string key = GenerateLocalCacheKey(mergeOption, useCSharpNullComparisonBehavior);
		_plans.TryGetValue(key, out var value);
		return value;
	}

	internal ObjectQueryExecutionPlan SetExecutionPlan(ObjectQueryExecutionPlan newPlan, bool useCSharpNullComparisonBehavior)
	{
		string key = GenerateLocalCacheKey(newPlan.MergeOption, useCSharpNullComparisonBehavior);
		return _plans.GetOrAdd(key, newPlan);
	}

	internal bool TryGetResultType(out TypeUsage resultType)
	{
		using (IEnumerator<ObjectQueryExecutionPlan> enumerator = _plans.Values.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				ObjectQueryExecutionPlan current = enumerator.Current;
				resultType = current.ResultType;
				return true;
			}
		}
		resultType = null;
		return false;
	}

	internal override object GetTarget()
	{
		return this;
	}

	private static string GenerateLocalCacheKey(MergeOption mergeOption, bool useCSharpNullComparisonBehavior)
	{
		if ((uint)mergeOption <= 3u)
		{
			return string.Join("", Enum.GetName(typeof(MergeOption), mergeOption), useCSharpNullComparisonBehavior);
		}
		throw new ArgumentOutOfRangeException("newPlan.MergeOption");
	}
}
