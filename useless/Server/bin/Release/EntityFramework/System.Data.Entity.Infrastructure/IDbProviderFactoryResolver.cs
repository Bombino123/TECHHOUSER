using System.Data.Common;

namespace System.Data.Entity.Infrastructure;

public interface IDbProviderFactoryResolver
{
	DbProviderFactory ResolveProviderFactory(DbConnection connection);
}
