using System.Data.Common;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Resources;

namespace System.Data.Entity.Utilities;

internal static class DbProviderServicesExtensions
{
	public static string GetProviderManifestTokenChecked(this DbProviderServices providerServices, DbConnection connection)
	{
		try
		{
			return providerServices.GetProviderManifestToken(connection);
		}
		catch (ProviderIncompatibleException innerException)
		{
			throw new ProviderIncompatibleException(Strings.FailedToGetProviderInformation, innerException);
		}
	}
}
