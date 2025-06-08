using System.Data.Entity.Utilities;

namespace System.Data.Entity.Infrastructure;

public sealed class DbProviderInfo
{
	private readonly string _providerInvariantName;

	private readonly string _providerManifestToken;

	public string ProviderInvariantName => _providerInvariantName;

	public string ProviderManifestToken => _providerManifestToken;

	public DbProviderInfo(string providerInvariantName, string providerManifestToken)
	{
		Check.NotEmpty(providerInvariantName, "providerInvariantName");
		Check.NotNull(providerManifestToken, "providerManifestToken");
		_providerInvariantName = providerInvariantName;
		_providerManifestToken = providerManifestToken;
	}

	private bool Equals(DbProviderInfo other)
	{
		if (string.Equals(_providerInvariantName, other._providerInvariantName))
		{
			return string.Equals(_providerManifestToken, other._providerManifestToken);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is DbProviderInfo other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (_providerInvariantName.GetHashCode() * 397) ^ _providerManifestToken.GetHashCode();
	}
}
