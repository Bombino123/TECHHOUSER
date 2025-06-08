using System.Data.Entity.Infrastructure.DependencyResolution;

namespace System.Data.Entity.Infrastructure.Interception;

public interface IDbConfigurationInterceptor : IDbInterceptor
{
	void Loaded(DbConfigurationLoadedEventArgs loadedEventArgs, DbConfigurationInterceptionContext interceptionContext);
}
