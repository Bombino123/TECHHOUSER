using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Infrastructure.DependencyResolution;

internal class DefaultInvariantNameResolver : IDbDependencyResolver
{
	public virtual object GetService(Type type, object key)
	{
		if (type == typeof(IProviderInvariantName))
		{
			return new ProviderInvariantName(((key as DbProviderFactory) ?? throw new ArgumentException(Strings.DbDependencyResolver_InvalidKey(typeof(DbProviderFactory).Name, typeof(IProviderInvariantName)))).GetProviderInvariantName());
		}
		return null;
	}

	public IEnumerable<object> GetServices(Type type, object key)
	{
		return this.GetServiceAsServices(type, key);
	}
}
