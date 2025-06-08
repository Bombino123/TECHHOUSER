using System.Data.Common;

namespace System.Data.Entity.Infrastructure.Interception;

internal interface ICancelableDbCommandInterceptor : IDbInterceptor
{
	bool CommandExecuting(DbCommand command, DbInterceptionContext interceptionContext);
}
