using System.Data.Entity.Utilities;

namespace System.Data.Entity.Infrastructure.Interception;

public static class DbInterception
{
	private static readonly Lazy<DbDispatchers> _dispatchers = new Lazy<DbDispatchers>(() => new DbDispatchers());

	public static DbDispatchers Dispatch => _dispatchers.Value;

	public static void Add(IDbInterceptor interceptor)
	{
		Check.NotNull(interceptor, "interceptor");
		_dispatchers.Value.AddInterceptor(interceptor);
	}

	public static void Remove(IDbInterceptor interceptor)
	{
		Check.NotNull(interceptor, "interceptor");
		_dispatchers.Value.RemoveInterceptor(interceptor);
	}
}
