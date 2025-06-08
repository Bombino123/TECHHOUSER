using System.Data.Entity.Core.Objects;

namespace System.Data.Entity.Core.Common.QueryCache;

internal class ShaperFactoryQueryCacheKey<T> : QueryCacheKey
{
	private readonly string _columnMapKey;

	private readonly MergeOption _mergeOption;

	private readonly bool _isValueLayer;

	private readonly bool _streaming;

	internal ShaperFactoryQueryCacheKey(string columnMapKey, MergeOption mergeOption, bool streaming, bool isValueLayer)
	{
		_columnMapKey = columnMapKey;
		_mergeOption = mergeOption;
		_isValueLayer = isValueLayer;
		_streaming = streaming;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ShaperFactoryQueryCacheKey<T> shaperFactoryQueryCacheKey))
		{
			return false;
		}
		if (_columnMapKey.Equals(shaperFactoryQueryCacheKey._columnMapKey, QueryCacheKey._stringComparison) && _mergeOption == shaperFactoryQueryCacheKey._mergeOption && _isValueLayer == shaperFactoryQueryCacheKey._isValueLayer)
		{
			return _streaming == shaperFactoryQueryCacheKey._streaming;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return _columnMapKey.GetHashCode();
	}
}
