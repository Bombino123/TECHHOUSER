using System.Data.Entity.Infrastructure.DependencyResolution;

namespace System.Data.Entity.Infrastructure.Interception;

internal class DbConfigurationDispatcher
{
	private readonly InternalDispatcher<IDbConfigurationInterceptor> _internalDispatcher = new InternalDispatcher<IDbConfigurationInterceptor>();

	public InternalDispatcher<IDbConfigurationInterceptor> InternalDispatcher => _internalDispatcher;

	public virtual void Loaded(DbConfigurationLoadedEventArgs loadedEventArgs, DbInterceptionContext interceptionContext)
	{
		DbConfigurationInterceptionContext clonedInterceptionContext = new DbConfigurationInterceptionContext(interceptionContext);
		_internalDispatcher.Dispatch(delegate(IDbConfigurationInterceptor i)
		{
			i.Loaded(loadedEventArgs, clonedInterceptionContext);
		});
	}
}
