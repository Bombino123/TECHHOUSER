using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Resources;
using System.Linq;

namespace System.Data.Entity.Infrastructure.DependencyResolution;

internal class DefaultProviderServicesResolver : IDbDependencyResolver
{
	public virtual object GetService(Type type, object key)
	{
		if (type == typeof(DbProviderServices))
		{
			throw new InvalidOperationException(Strings.EF6Providers_NoProviderFound(CheckKey(key)));
		}
		return null;
	}

	private static string CheckKey(object key)
	{
		string obj = key as string;
		if (string.IsNullOrWhiteSpace(obj))
		{
			throw new ArgumentException(Strings.DbDependencyResolver_NoProviderInvariantName(typeof(DbProviderServices).Name));
		}
		return obj;
	}

	public virtual IEnumerable<object> GetServices(Type type, object key)
	{
		if (type == typeof(DbProviderServices))
		{
			CheckKey(key);
		}
		return Enumerable.Empty<object>();
	}
}
