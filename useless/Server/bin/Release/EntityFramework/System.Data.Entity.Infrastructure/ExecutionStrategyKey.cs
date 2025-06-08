using System.Data.Entity.Utilities;

namespace System.Data.Entity.Infrastructure;

public class ExecutionStrategyKey
{
	public string ProviderInvariantName { get; private set; }

	public string ServerName { get; private set; }

	public ExecutionStrategyKey(string providerInvariantName, string serverName)
	{
		Check.NotEmpty(providerInvariantName, "providerInvariantName");
		ProviderInvariantName = providerInvariantName;
		ServerName = serverName;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ExecutionStrategyKey executionStrategyKey))
		{
			return false;
		}
		if (ProviderInvariantName.Equals(executionStrategyKey.ProviderInvariantName, StringComparison.Ordinal))
		{
			if (ServerName != null || executionStrategyKey.ServerName != null)
			{
				if (ServerName != null)
				{
					return ServerName.Equals(executionStrategyKey.ServerName, StringComparison.Ordinal);
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (ServerName != null)
		{
			return ProviderInvariantName.GetHashCode() ^ ServerName.GetHashCode();
		}
		return ProviderInvariantName.GetHashCode();
	}
}
