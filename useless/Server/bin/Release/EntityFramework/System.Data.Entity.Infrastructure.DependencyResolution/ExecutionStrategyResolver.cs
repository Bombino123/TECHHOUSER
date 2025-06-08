using System.Collections.Generic;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Infrastructure.DependencyResolution;

public class ExecutionStrategyResolver<T> : IDbDependencyResolver where T : IDbExecutionStrategy
{
	private readonly Func<T> _getExecutionStrategy;

	private readonly string _providerInvariantName;

	private readonly string _serverName;

	public ExecutionStrategyResolver(string providerInvariantName, string serverName, Func<T> getExecutionStrategy)
	{
		Check.NotEmpty(providerInvariantName, "providerInvariantName");
		Check.NotNull(getExecutionStrategy, "getExecutionStrategy");
		_providerInvariantName = providerInvariantName;
		_serverName = serverName;
		_getExecutionStrategy = getExecutionStrategy;
	}

	public object GetService(Type type, object key)
	{
		if (type == typeof(Func<IDbExecutionStrategy>))
		{
			if (!(key is ExecutionStrategyKey executionStrategyKey))
			{
				throw new ArgumentException(Strings.DbDependencyResolver_InvalidKey(typeof(ExecutionStrategyKey).Name, "Func<IExecutionStrategy>"));
			}
			if (!executionStrategyKey.ProviderInvariantName.Equals(_providerInvariantName, StringComparison.Ordinal))
			{
				return null;
			}
			if (_serverName != null && !_serverName.Equals(executionStrategyKey.ServerName, StringComparison.Ordinal))
			{
				return null;
			}
			return _getExecutionStrategy;
		}
		return null;
	}

	public IEnumerable<object> GetServices(Type type, object key)
	{
		return this.GetServiceAsServices(type, key);
	}
}
