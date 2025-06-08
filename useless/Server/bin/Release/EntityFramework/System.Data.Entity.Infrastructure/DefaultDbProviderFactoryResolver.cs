using System.Data.Common;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Infrastructure;

internal class DefaultDbProviderFactoryResolver : IDbProviderFactoryResolver
{
	public DbProviderFactory ResolveProviderFactory(DbConnection connection)
	{
		Check.NotNull(connection, "connection");
		return DbProviderFactories.GetFactory(connection);
	}
}
