using System.Data.Entity.Core.Objects;

namespace System.Data.Entity.Core.Common.QueryCache;

internal sealed class EntitySqlQueryCacheKey : QueryCacheKey
{
	private readonly int _hashCode;

	private readonly string _defaultContainer;

	private readonly string _eSqlStatement;

	private readonly string _parametersToken;

	private readonly int _parameterCount;

	private readonly string _includePathsToken;

	private readonly MergeOption _mergeOption;

	private readonly Type _resultType;

	private readonly bool _streaming;

	internal EntitySqlQueryCacheKey(string defaultContainerName, string eSqlStatement, int parameterCount, string parametersToken, string includePathsToken, MergeOption mergeOption, bool streaming, Type resultType)
	{
		_defaultContainer = defaultContainerName;
		_eSqlStatement = eSqlStatement;
		_parameterCount = parameterCount;
		_parametersToken = parametersToken;
		_includePathsToken = includePathsToken;
		_mergeOption = mergeOption;
		_streaming = streaming;
		_resultType = resultType;
		int num = _eSqlStatement.GetHashCode() ^ _mergeOption.GetHashCode();
		if (_parametersToken != null)
		{
			num ^= _parametersToken.GetHashCode();
		}
		if (_includePathsToken != null)
		{
			num ^= _includePathsToken.GetHashCode();
		}
		if (_defaultContainer != null)
		{
			num ^= _defaultContainer.GetHashCode();
		}
		_hashCode = num;
	}

	public override bool Equals(object otherObject)
	{
		if (typeof(EntitySqlQueryCacheKey) != otherObject.GetType())
		{
			return false;
		}
		EntitySqlQueryCacheKey entitySqlQueryCacheKey = (EntitySqlQueryCacheKey)otherObject;
		if (_parameterCount == entitySqlQueryCacheKey._parameterCount && _mergeOption == entitySqlQueryCacheKey._mergeOption && _streaming == entitySqlQueryCacheKey._streaming && Equals(entitySqlQueryCacheKey._defaultContainer, _defaultContainer) && Equals(entitySqlQueryCacheKey._eSqlStatement, _eSqlStatement) && Equals(entitySqlQueryCacheKey._includePathsToken, _includePathsToken) && Equals(entitySqlQueryCacheKey._parametersToken, _parametersToken))
		{
			return object.Equals(entitySqlQueryCacheKey._resultType, _resultType);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return _hashCode;
	}

	public override string ToString()
	{
		return string.Join("|", _defaultContainer, _eSqlStatement, _parametersToken, _includePathsToken, Enum.GetName(typeof(MergeOption), _mergeOption));
	}
}
