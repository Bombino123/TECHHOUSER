using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Resources;

namespace System.Data.Entity.Infrastructure.DependencyResolution;

internal class InvariantNameResolver : IDbDependencyResolver
{
	private readonly IProviderInvariantName _invariantName;

	private readonly Type _providerFactoryType;

	public InvariantNameResolver(DbProviderFactory providerFactory, string invariantName)
	{
		_invariantName = new ProviderInvariantName(invariantName);
		_providerFactoryType = providerFactory.GetType();
	}

	public virtual object GetService(Type type, object key)
	{
		if (type == typeof(IProviderInvariantName))
		{
			if (!(key is DbProviderFactory))
			{
				throw new ArgumentException(Strings.DbDependencyResolver_InvalidKey(typeof(DbProviderFactory).Name, typeof(IProviderInvariantName)));
			}
			if (key.GetType() == _providerFactoryType)
			{
				return _invariantName;
			}
		}
		return null;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is InvariantNameResolver invariantNameResolver))
		{
			return false;
		}
		if (_providerFactoryType == invariantNameResolver._providerFactoryType)
		{
			return _invariantName.Name == invariantNameResolver._invariantName.Name;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return _invariantName.Name.GetHashCode();
	}

	public IEnumerable<object> GetServices(Type type, object key)
	{
		return this.GetServiceAsServices(type, key);
	}
}
