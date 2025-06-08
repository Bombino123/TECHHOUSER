using System.Data.Entity.Core.EntityClient;

namespace System.Data.Entity.Infrastructure.Interception;

internal class CancelableEntityConnectionDispatcher
{
	private readonly InternalDispatcher<ICancelableEntityConnectionInterceptor> _internalDispatcher = new InternalDispatcher<ICancelableEntityConnectionInterceptor>();

	public InternalDispatcher<ICancelableEntityConnectionInterceptor> InternalDispatcher => _internalDispatcher;

	public virtual bool Opening(EntityConnection entityConnection, DbInterceptionContext interceptionContext)
	{
		return _internalDispatcher.Dispatch(result: true, (bool b, ICancelableEntityConnectionInterceptor i) => i.ConnectionOpening(entityConnection, interceptionContext) && b);
	}
}
