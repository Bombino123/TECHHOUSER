using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity.Internal;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.Infrastructure.DependencyResolution;

internal class DbConfigurationManager
{
	private static readonly DbConfigurationManager _configManager = new DbConfigurationManager(new DbConfigurationLoader(), new DbConfigurationFinder());

	private EventHandler<DbConfigurationLoadedEventArgs> _loadedHandler;

	private readonly DbConfigurationLoader _loader;

	private readonly DbConfigurationFinder _finder;

	private readonly Lazy<InternalConfiguration> _configuration;

	private volatile DbConfiguration _newConfiguration;

	private volatile Type _newConfigurationType = typeof(DbConfiguration);

	private readonly object _lock = new object();

	private readonly ConcurrentDictionary<Assembly, object> _knownAssemblies = new ConcurrentDictionary<Assembly, object>();

	private readonly Lazy<List<Tuple<AppConfig, InternalConfiguration>>> _configurationOverrides = new Lazy<List<Tuple<AppConfig, InternalConfiguration>>>(() => new List<Tuple<AppConfig, InternalConfiguration>>());

	public static DbConfigurationManager Instance => _configManager;

	private bool ConfigurationSet => _configuration.IsValueCreated;

	public DbConfigurationManager(DbConfigurationLoader loader, DbConfigurationFinder finder)
	{
		_loader = loader;
		_finder = finder;
		_configuration = new Lazy<InternalConfiguration>(delegate
		{
			DbConfiguration obj = _newConfiguration ?? _newConfigurationType.CreateInstance<DbConfiguration>(Strings.CreateInstance_BadDbConfigurationType);
			obj.InternalConfiguration.Lock();
			return obj.InternalConfiguration;
		});
	}

	public virtual void AddLoadedHandler(EventHandler<DbConfigurationLoadedEventArgs> handler)
	{
		if (ConfigurationSet)
		{
			throw new InvalidOperationException(Strings.AddHandlerToInUseConfiguration);
		}
		_loadedHandler = (EventHandler<DbConfigurationLoadedEventArgs>)Delegate.Combine(_loadedHandler, handler);
	}

	public virtual void RemoveLoadedHandler(EventHandler<DbConfigurationLoadedEventArgs> handler)
	{
		_loadedHandler = (EventHandler<DbConfigurationLoadedEventArgs>)Delegate.Remove(_loadedHandler, handler);
	}

	public virtual void OnLoaded(InternalConfiguration configuration)
	{
		DbConfigurationLoadedEventArgs dbConfigurationLoadedEventArgs = new DbConfigurationLoadedEventArgs(configuration);
		_loadedHandler?.Invoke(configuration.Owner, dbConfigurationLoadedEventArgs);
		configuration.DispatchLoadedInterceptors(dbConfigurationLoadedEventArgs);
	}

	public virtual InternalConfiguration GetConfiguration()
	{
		if (_configurationOverrides.IsValueCreated)
		{
			lock (_lock)
			{
				if (_configurationOverrides.Value.Count != 0)
				{
					return _configurationOverrides.Value.Last().Item2;
				}
			}
		}
		return _configuration.Value;
	}

	public virtual void SetConfigurationType(Type configurationType)
	{
		_newConfigurationType = configurationType;
	}

	public virtual void SetConfiguration(InternalConfiguration configuration)
	{
		Type type = _loader.TryLoadFromConfig(AppConfig.DefaultInstance);
		if (type != null)
		{
			configuration = type.CreateInstance<DbConfiguration>(Strings.CreateInstance_BadDbConfigurationType).InternalConfiguration;
		}
		_newConfiguration = configuration.Owner;
		if (_configuration.Value.Owner.GetType() != configuration.Owner.GetType())
		{
			if (_configuration.Value.Owner.GetType() == typeof(DbConfiguration))
			{
				throw new InvalidOperationException(Strings.DefaultConfigurationUsedBeforeSet(configuration.Owner.GetType().Name));
			}
			throw new InvalidOperationException(Strings.ConfigurationSetTwice(configuration.Owner.GetType().Name, _configuration.Value.Owner.GetType().Name));
		}
	}

	public virtual void EnsureLoadedForContext(Type contextType)
	{
		EnsureLoadedForAssembly(contextType.Assembly(), contextType);
	}

	public virtual void EnsureLoadedForAssembly(Assembly assemblyHint, Type contextTypeHint)
	{
		if (contextTypeHint == typeof(DbContext) || _knownAssemblies.ContainsKey(assemblyHint))
		{
			return;
		}
		if (_configurationOverrides.IsValueCreated)
		{
			lock (_lock)
			{
				if (_configurationOverrides.Value.Count != 0)
				{
					return;
				}
			}
		}
		if (!ConfigurationSet)
		{
			Type type = _loader.TryLoadFromConfig(AppConfig.DefaultInstance) ?? _finder.TryFindConfigurationType(assemblyHint, _finder.TryFindContextType(assemblyHint, contextTypeHint));
			if (type != null)
			{
				SetConfigurationType(type);
			}
		}
		else if (!assemblyHint.IsDynamic && !_loader.AppConfigContainsDbConfigurationType(AppConfig.DefaultInstance))
		{
			contextTypeHint = _finder.TryFindContextType(assemblyHint, contextTypeHint);
			Type type2 = _finder.TryFindConfigurationType(assemblyHint, contextTypeHint);
			if (type2 != null)
			{
				if (_configuration.Value.Owner.GetType() == typeof(DbConfiguration))
				{
					throw new InvalidOperationException(Strings.ConfigurationNotDiscovered(type2.Name));
				}
				if (contextTypeHint != null && type2 != _configuration.Value.Owner.GetType())
				{
					throw new InvalidOperationException(Strings.SetConfigurationNotDiscovered(_configuration.Value.Owner.GetType().Name, contextTypeHint.Name));
				}
			}
		}
		_knownAssemblies.TryAdd(assemblyHint, null);
	}

	public virtual bool PushConfiguration(AppConfig config, Type contextType)
	{
		if (config == AppConfig.DefaultInstance && (contextType == typeof(DbContext) || _knownAssemblies.ContainsKey(contextType.Assembly())))
		{
			return false;
		}
		InternalConfiguration internalConfiguration = (_loader.TryLoadFromConfig(config) ?? _finder.TryFindConfigurationType(contextType) ?? typeof(DbConfiguration)).CreateInstance<DbConfiguration>(Strings.CreateInstance_BadDbConfigurationType).InternalConfiguration;
		internalConfiguration.SwitchInRootResolver(_configuration.Value.RootResolver);
		internalConfiguration.AddAppConfigResolver(new AppConfigDependencyResolver(config, internalConfiguration));
		lock (_lock)
		{
			_configurationOverrides.Value.Add(Tuple.Create(config, internalConfiguration));
		}
		internalConfiguration.Lock();
		return true;
	}

	public virtual void PopConfiguration(AppConfig config)
	{
		lock (_lock)
		{
			Tuple<AppConfig, InternalConfiguration> tuple = _configurationOverrides.Value.FirstOrDefault((Tuple<AppConfig, InternalConfiguration> c) => c.Item1 == config);
			if (tuple != null)
			{
				_configurationOverrides.Value.Remove(tuple);
			}
		}
	}
}
