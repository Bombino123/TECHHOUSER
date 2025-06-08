using System.Data.Entity.Core.Common.CommandTrees;

namespace System.Data.Entity.Infrastructure.Interception;

internal class DbCommandTreeDispatcher
{
	private readonly InternalDispatcher<IDbCommandTreeInterceptor> _internalDispatcher = new InternalDispatcher<IDbCommandTreeInterceptor>();

	public InternalDispatcher<IDbCommandTreeInterceptor> InternalDispatcher => _internalDispatcher;

	public virtual DbCommandTree Created(DbCommandTree commandTree, DbInterceptionContext interceptionContext)
	{
		return _internalDispatcher.Dispatch(commandTree, new DbCommandTreeInterceptionContext(interceptionContext), delegate(IDbCommandTreeInterceptor i, DbCommandTreeInterceptionContext c)
		{
			i.TreeCreated(c);
		});
	}
}
