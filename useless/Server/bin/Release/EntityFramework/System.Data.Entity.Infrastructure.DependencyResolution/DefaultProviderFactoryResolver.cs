using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Resources;
using System.Data.SqlClient;
using System.Linq;

namespace System.Data.Entity.Infrastructure.DependencyResolution;

internal class DefaultProviderFactoryResolver : IDbDependencyResolver
{
	public virtual object GetService(Type type, object key)
	{
		return GetService(type, key, delegate(ArgumentException e, string n)
		{
			throw new ArgumentException(Strings.EntityClient_InvalidStoreProvider(n), e);
		});
	}

	private static object GetService(Type type, object key, Func<ArgumentException, string, object> handleFailedLookup)
	{
		if (type == typeof(DbProviderFactory))
		{
			string text = key as string;
			if (string.IsNullOrWhiteSpace(text))
			{
				throw new ArgumentException(Strings.DbDependencyResolver_NoProviderInvariantName(typeof(DbProviderFactory).Name));
			}
			try
			{
				return DbProviderFactories.GetFactory(text);
			}
			catch (ArgumentException arg)
			{
				if (string.Equals(text, "System.Data.SqlClient", StringComparison.OrdinalIgnoreCase))
				{
					return SqlClientFactory.Instance;
				}
				return handleFailedLookup(arg, text);
			}
		}
		return null;
	}

	public IEnumerable<object> GetServices(Type type, object key)
	{
		object service = GetService(type, key, (ArgumentException e, string n) => (object)null);
		if (service != null)
		{
			return new object[1] { service };
		}
		return Enumerable.Empty<object>();
	}
}
