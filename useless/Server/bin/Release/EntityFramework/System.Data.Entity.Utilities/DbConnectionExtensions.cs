using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.DependencyResolution;

namespace System.Data.Entity.Utilities;

internal static class DbConnectionExtensions
{
	public static string GetProviderInvariantName(this DbConnection connection)
	{
		return DbConfiguration.DependencyResolver.GetService<IProviderInvariantName>(DbProviderServices.GetProviderFactory(connection)).Name;
	}

	public static DbProviderInfo GetProviderInfo(this DbConnection connection, out DbProviderManifest providerManifest)
	{
		string text = DbConfiguration.DependencyResolver.GetService<IManifestTokenResolver>().ResolveManifestToken(connection);
		DbProviderInfo result = new DbProviderInfo(connection.GetProviderInvariantName(), text);
		providerManifest = DbProviderServices.GetProviderServices(connection).GetProviderManifest(text);
		return result;
	}

	public static DbProviderFactory GetProviderFactory(this DbConnection connection)
	{
		return DbConfiguration.DependencyResolver.GetService<IDbProviderFactoryResolver>().ResolveProviderFactory(connection);
	}
}
