using System.Data.Entity.Infrastructure;

namespace System.Data.Entity.Internal;

internal sealed class DefaultModelCacheKey : IDbModelCacheKey
{
	private readonly Type _contextType;

	private readonly string _providerName;

	private readonly Type _providerType;

	private readonly string _customKey;

	public DefaultModelCacheKey(Type contextType, string providerName, Type providerType, string customKey)
	{
		_contextType = contextType;
		_providerName = providerName;
		_providerType = providerType;
		_customKey = customKey;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj is DefaultModelCacheKey other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (_contextType.GetHashCode() * 397) ^ _providerName.GetHashCode() ^ _providerType.GetHashCode() ^ ((!string.IsNullOrWhiteSpace(_customKey)) ? _customKey.GetHashCode() : 0);
	}

	private bool Equals(DefaultModelCacheKey other)
	{
		if (_contextType == other._contextType && string.Equals(_providerName, other._providerName) && object.Equals(_providerType, other._providerType))
		{
			return string.Equals(_customKey, other._customKey);
		}
		return false;
	}
}
