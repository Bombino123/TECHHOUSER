using System.Collections.Concurrent;
using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Infrastructure;

public class DefaultManifestTokenResolver : IManifestTokenResolver
{
	private readonly ConcurrentDictionary<Tuple<Type, string, string>, string> _cachedTokens = new ConcurrentDictionary<Tuple<Type, string, string>, string>();

	public string ResolveManifestToken(DbConnection connection)
	{
		Check.NotNull(connection, "connection");
		DbInterceptionContext interceptionContext = new DbInterceptionContext();
		Tuple<Type, string, string> key = Tuple.Create(connection.GetType(), DbInterception.Dispatch.Connection.GetDataSource(connection, interceptionContext), DbInterception.Dispatch.Connection.GetDatabase(connection, interceptionContext));
		return _cachedTokens.GetOrAdd(key, (Tuple<Type, string, string> k) => DbProviderServices.GetProviderServices(connection).GetProviderManifestTokenChecked(connection));
	}
}
