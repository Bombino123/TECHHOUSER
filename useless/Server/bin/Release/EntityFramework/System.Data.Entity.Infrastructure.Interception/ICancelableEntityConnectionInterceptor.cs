using System.Data.Entity.Core.EntityClient;

namespace System.Data.Entity.Infrastructure.Interception;

internal interface ICancelableEntityConnectionInterceptor : IDbInterceptor
{
	bool ConnectionOpening(EntityConnection connection, DbInterceptionContext interceptionContext);
}
