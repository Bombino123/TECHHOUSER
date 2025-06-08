using System.Data.Entity.Core.Objects;

namespace System.Data.Entity.Core.Common.QueryCache;

internal sealed class LinqQueryCacheKey : QueryCacheKey
{
	private readonly int _hashCode;

	private readonly string _expressionKey;

	private readonly string _parametersToken;

	private readonly int _parameterCount;

	private readonly string _includePathsToken;

	private readonly MergeOption _mergeOption;

	private readonly Type _resultType;

	private readonly bool _streaming;

	private readonly bool _useCSharpNullComparisonBehavior;

	internal LinqQueryCacheKey(string expressionKey, int parameterCount, string parametersToken, string includePathsToken, MergeOption mergeOption, bool streaming, bool useCSharpNullComparisonBehavior, Type resultType)
	{
		_expressionKey = expressionKey;
		_parameterCount = parameterCount;
		_parametersToken = parametersToken;
		_includePathsToken = includePathsToken;
		_mergeOption = mergeOption;
		_streaming = streaming;
		_resultType = resultType;
		_useCSharpNullComparisonBehavior = useCSharpNullComparisonBehavior;
		int num = _expressionKey.GetHashCode() ^ _mergeOption.GetHashCode();
		if (_parametersToken != null)
		{
			num ^= _parametersToken.GetHashCode();
		}
		if (_includePathsToken != null)
		{
			num ^= _includePathsToken.GetHashCode();
		}
		num ^= _useCSharpNullComparisonBehavior.GetHashCode();
		_hashCode = num;
	}

	public override bool Equals(object otherObject)
	{
		if (typeof(LinqQueryCacheKey) != otherObject.GetType())
		{
			return false;
		}
		LinqQueryCacheKey linqQueryCacheKey = (LinqQueryCacheKey)otherObject;
		if (_parameterCount == linqQueryCacheKey._parameterCount && _mergeOption == linqQueryCacheKey._mergeOption && _streaming == linqQueryCacheKey._streaming && Equals(linqQueryCacheKey._expressionKey, _expressionKey) && Equals(linqQueryCacheKey._includePathsToken, _includePathsToken) && Equals(linqQueryCacheKey._parametersToken, _parametersToken) && object.Equals(linqQueryCacheKey._resultType, _resultType))
		{
			return object.Equals(linqQueryCacheKey._useCSharpNullComparisonBehavior, _useCSharpNullComparisonBehavior);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return _hashCode;
	}

	public override string ToString()
	{
		string[] obj = new string[5]
		{
			_expressionKey,
			_parametersToken,
			_includePathsToken,
			Enum.GetName(typeof(MergeOption), _mergeOption),
			null
		};
		bool useCSharpNullComparisonBehavior = _useCSharpNullComparisonBehavior;
		obj[4] = useCSharpNullComparisonBehavior.ToString();
		return string.Join("|", obj);
	}
}
