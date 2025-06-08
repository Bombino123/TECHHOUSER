using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.Internal;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.Infrastructure.DependencyResolution;

internal class AppConfigDependencyResolver : IDbDependencyResolver
{
	private readonly AppConfig _appConfig;

	private readonly InternalConfiguration _internalConfiguration;

	private readonly ConcurrentDictionary<Tuple<Type, object>, Func<object>> _serviceFactories = new ConcurrentDictionary<Tuple<Type, object>, Func<object>>();

	private readonly ConcurrentDictionary<Tuple<Type, object>, IEnumerable<Func<object>>> _servicesFactories = new ConcurrentDictionary<Tuple<Type, object>, IEnumerable<Func<object>>>();

	private readonly Dictionary<string, DbProviderServices> _providerFactories = new Dictionary<string, DbProviderServices>();

	private bool _providersRegistered;

	private readonly ProviderServicesFactory _providerServicesFactory;

	public AppConfigDependencyResolver()
	{
	}

	public AppConfigDependencyResolver(AppConfig appConfig, InternalConfiguration internalConfiguration, ProviderServicesFactory providerServicesFactory = null)
	{
		_appConfig = appConfig;
		_internalConfiguration = internalConfiguration;
		_providerServicesFactory = providerServicesFactory ?? new ProviderServicesFactory();
	}

	public virtual object GetService(Type type, object key)
	{
		return _serviceFactories.GetOrAdd(Tuple.Create(type, key), (Tuple<Type, object> t) => GetServiceFactory(type, key as string))();
	}

	public IEnumerable<object> GetServices(Type type, object key)
	{
		return (from f in _servicesFactories.GetOrAdd(Tuple.Create(type, key), (Tuple<Type, object> t) => GetServicesFactory(type, key))
			select f() into s
			where s != null
			select s).ToList();
	}

	public virtual IEnumerable<Func<object>> GetServicesFactory(Type type, object key)
	{
		if (type == typeof(IDbInterceptor))
		{
			return _appConfig.Interceptors.Select((Func<IDbInterceptor, Func<object>>)((IDbInterceptor i) => () => i)).ToList();
		}
		return new List<Func<object>> { GetServiceFactory(type, key as string) };
	}

	public virtual Func<object> GetServiceFactory(Type type, string name)
	{
		if (!_providersRegistered)
		{
			lock (_providerFactories)
			{
				if (!_providersRegistered)
				{
					RegisterDbProviderServices();
					_providersRegistered = true;
				}
			}
		}
		if (!string.IsNullOrWhiteSpace(name) && type == typeof(DbProviderServices))
		{
			_providerFactories.TryGetValue(name, out var providerFactory);
			return () => providerFactory;
		}
		if (type == typeof(IDbConnectionFactory))
		{
			if (!Database.DefaultConnectionFactoryChanged)
			{
				IDbConnectionFactory dbConnectionFactory = _appConfig.TryGetDefaultConnectionFactory();
				if (dbConnectionFactory != null)
				{
					Database.DefaultConnectionFactory = dbConnectionFactory;
				}
			}
			return () => (!Database.DefaultConnectionFactoryChanged) ? null : Database.SetDefaultConnectionFactory;
		}
		Type type2 = type.TryGetElementType(typeof(IDatabaseInitializer<>));
		if (type2 != null)
		{
			object initializer = _appConfig.Initializers.TryGetInitializer(type2);
			return () => initializer;
		}
		return () => (object)null;
	}

	private void RegisterDbProviderServices()
	{
		IList<NamedDbProviderService> dbProviderServices = _appConfig.DbProviderServices;
		if (dbProviderServices.All((NamedDbProviderService p) => p.InvariantName != "System.Data.SqlClient"))
		{
			RegisterSqlServerProvider();
		}
		dbProviderServices.Each(delegate(NamedDbProviderService p)
		{
			_providerFactories[p.InvariantName] = p.ProviderServices;
			_internalConfiguration.AddDefaultResolver(p.ProviderServices);
		});
	}

	private void RegisterSqlServerProvider()
	{
		string providerTypeName = string.Format(CultureInfo.InvariantCulture, "System.Data.Entity.SqlServer.SqlProviderServices, EntityFramework.SqlServer, Version={0}, Culture=neutral, PublicKeyToken=b77a5c561934e089", new object[1] { new AssemblyName(typeof(DbContext).Assembly().FullName).Version });
		DbProviderServices dbProviderServices = _providerServicesFactory.TryGetInstance(providerTypeName);
		if (dbProviderServices != null)
		{
			_internalConfiguration.SetDefaultProviderServices(dbProviderServices, "System.Data.SqlClient");
		}
	}
}
