using System.Collections.Generic;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Infrastructure.DependencyResolution;

public class TransactionHandlerResolver : IDbDependencyResolver
{
	private readonly Func<TransactionHandler> _transactionHandlerFactory;

	private readonly string _providerInvariantName;

	private readonly string _serverName;

	public TransactionHandlerResolver(Func<TransactionHandler> transactionHandlerFactory, string providerInvariantName, string serverName)
	{
		Check.NotNull(transactionHandlerFactory, "transactionHandlerFactory");
		_providerInvariantName = providerInvariantName;
		_serverName = serverName;
		_transactionHandlerFactory = transactionHandlerFactory;
	}

	public object GetService(Type type, object key)
	{
		if (type == typeof(Func<TransactionHandler>))
		{
			if (!(key is ExecutionStrategyKey executionStrategyKey))
			{
				throw new ArgumentException(Strings.DbDependencyResolver_InvalidKey(typeof(ExecutionStrategyKey).Name, "Func<TransactionHandler>"));
			}
			if (_providerInvariantName != null && !executionStrategyKey.ProviderInvariantName.Equals(_providerInvariantName, StringComparison.Ordinal))
			{
				return null;
			}
			if (_serverName != null && !_serverName.Equals(executionStrategyKey.ServerName, StringComparison.Ordinal))
			{
				return null;
			}
			return _transactionHandlerFactory;
		}
		return null;
	}

	public IEnumerable<object> GetServices(Type type, object key)
	{
		return this.GetServiceAsServices(type, key);
	}

	public override bool Equals(object obj)
	{
		if (!(obj is TransactionHandlerResolver transactionHandlerResolver))
		{
			return false;
		}
		if (_transactionHandlerFactory == transactionHandlerResolver._transactionHandlerFactory && _providerInvariantName == transactionHandlerResolver._providerInvariantName)
		{
			return _serverName == transactionHandlerResolver._serverName;
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = _transactionHandlerFactory.GetHashCode();
		if (_providerInvariantName != null)
		{
			num = num * 41 + _providerInvariantName.GetHashCode();
		}
		if (_serverName != null)
		{
			num = num * 41 + _serverName.GetHashCode();
		}
		return num;
	}
}
