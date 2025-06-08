using System.Data.Common;

namespace System.Data.Entity.Infrastructure.Interception;

internal class CancelableDbCommandDispatcher
{
	private readonly InternalDispatcher<ICancelableDbCommandInterceptor> _internalDispatcher = new InternalDispatcher<ICancelableDbCommandInterceptor>();

	public InternalDispatcher<ICancelableDbCommandInterceptor> InternalDispatcher => _internalDispatcher;

	public virtual bool Executing(DbCommand command, DbInterceptionContext interceptionContext)
	{
		return _internalDispatcher.Dispatch(result: true, (bool b, ICancelableDbCommandInterceptor i) => i.CommandExecuting(command, interceptionContext) && b);
	}
}
