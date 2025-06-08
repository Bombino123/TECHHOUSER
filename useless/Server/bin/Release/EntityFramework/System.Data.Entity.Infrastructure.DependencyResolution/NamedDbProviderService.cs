using System.Data.Entity.Core.Common;

namespace System.Data.Entity.Infrastructure.DependencyResolution;

internal class NamedDbProviderService
{
	private readonly string _invariantName;

	private readonly DbProviderServices _providerServices;

	public string InvariantName => _invariantName;

	public DbProviderServices ProviderServices => _providerServices;

	public NamedDbProviderService(string invariantName, DbProviderServices providerServices)
	{
		_invariantName = invariantName;
		_providerServices = providerServices;
	}
}
