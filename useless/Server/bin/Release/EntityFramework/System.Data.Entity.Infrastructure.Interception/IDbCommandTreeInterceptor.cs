namespace System.Data.Entity.Infrastructure.Interception;

public interface IDbCommandTreeInterceptor : IDbInterceptor
{
	void TreeCreated(DbCommandTreeInterceptionContext interceptionContext);
}
