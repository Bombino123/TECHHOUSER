using System.Data.Common;

namespace System.Data.Entity.Infrastructure.Interception;

public class DbCommandInterceptor : IDbCommandInterceptor, IDbInterceptor
{
	public virtual void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
	{
	}

	public virtual void NonQueryExecuted(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
	{
	}

	public virtual void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
	{
	}

	public virtual void ReaderExecuted(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
	{
	}

	public virtual void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
	{
	}

	public virtual void ScalarExecuted(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
	{
	}
}
