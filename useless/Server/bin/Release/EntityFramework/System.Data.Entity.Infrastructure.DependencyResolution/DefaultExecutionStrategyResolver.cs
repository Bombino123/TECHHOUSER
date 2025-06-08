using System.Collections.Generic;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Infrastructure.DependencyResolution;

internal class DefaultExecutionStrategyResolver : IDbDependencyResolver
{
	public object GetService(Type type, object key)
	{
		if (type == typeof(Func<IDbExecutionStrategy>))
		{
			Check.NotNull(key, "key");
			if (!(key is ExecutionStrategyKey))
			{
				throw new ArgumentException(Strings.DbDependencyResolver_InvalidKey(typeof(ExecutionStrategyKey).Name, "Func<IExecutionStrategy>"));
			}
			return (Func<IDbExecutionStrategy>)(() => new DefaultExecutionStrategy());
		}
		return null;
	}

	public IEnumerable<object> GetServices(Type type, object key)
	{
		return this.GetServiceAsServices(type, key);
	}
}
