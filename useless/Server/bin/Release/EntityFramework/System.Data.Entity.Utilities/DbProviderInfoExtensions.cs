using System.Data.Entity.Infrastructure;

namespace System.Data.Entity.Utilities;

internal static class DbProviderInfoExtensions
{
	public static bool IsSqlCe(this DbProviderInfo providerInfo)
	{
		if (!string.IsNullOrWhiteSpace(providerInfo.ProviderInvariantName))
		{
			return providerInfo.ProviderInvariantName.StartsWith("System.Data.SqlServerCe", StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}
}
