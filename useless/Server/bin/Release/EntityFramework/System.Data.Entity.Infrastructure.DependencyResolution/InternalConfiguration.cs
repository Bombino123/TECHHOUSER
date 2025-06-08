using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.Internal;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Infrastructure.DependencyResolution;

internal class InternalConfiguration
{
	private CompositeResolver<ResolverChain, ResolverChain> _resolvers;

	private RootDependencyResolver _rootResolver;

	private readonly Func<DbDispatchers> _dispatchers;

	private bool _isLocked;

	public static InternalConfiguration Instance
	{
		get
		{
			return DbConfigurationManager.Instance.GetConfiguration();
		}
		set
		{
			DbConfigurationManager.Instance.SetConfiguration(value);
		}
	}

	public virtual IDbDependencyResolver DependencyResolver => _resolvers;

	public virtual RootDependencyResolver RootResolver => _rootResolver;

	public virtual IDbDependencyResolver ResolverSnapshot
	{
		get
		{
			ResolverChain resolverChain = new ResolverChain();
			_resolvers.Second.Resolvers.Each(resolverChain.Add);
			_resolvers.First.Resolvers.Each(resolverChain.Add);
			return resolverChain;
		}
	}

	public virtual DbConfiguration Owner { get; set; }

	public InternalConfiguration(ResolverChain appConfigChain = null, ResolverChain normalResolverChain = null, RootDependencyResolver rootResolver = null, AppConfigDependencyResolver appConfigResolver = null, Func<DbDispatchers> dispatchers = null)
	{
		_rootResolver = rootResolver ?? new RootDependencyResolver();
		_resolvers = new CompositeResolver<ResolverChain, ResolverChain>(appConfigChain ?? new ResolverChain(), normalResolverChain ?? new ResolverChain());
		_resolvers.Second.Add(_rootResolver);
		_resolvers.First.Add(appConfigResolver ?? new AppConfigDependencyResolver(AppConfig.DefaultInstance, this));
		_dispatchers = dispatchers ?? ((Func<DbDispatchers>)(() => DbInterception.Dispatch));
	}

	public virtual void Lock()
	{
		List<IDbInterceptor> list = DependencyResolver.GetServices<IDbInterceptor>().ToList();
		list.Each(_dispatchers().AddInterceptor);
		DbConfigurationManager.Instance.OnLoaded(this);
		_isLocked = true;
		DependencyResolver.GetServices<IDbInterceptor>().Except(list).Each(_dispatchers().AddInterceptor);
	}

	public void DispatchLoadedInterceptors(DbConfigurationLoadedEventArgs loadedEventArgs)
	{
		_dispatchers().Configuration.Loaded(loadedEventArgs, new DbInterceptionContext());
	}

	public virtual void AddAppConfigResolver(IDbDependencyResolver resolver)
	{
		_resolvers.First.Add(resolver);
	}

	public virtual void AddDependencyResolver(IDbDependencyResolver resolver, bool overrideConfigFile = false)
	{
		(overrideConfigFile ? _resolvers.First : _resolvers.Second).Add(resolver);
	}

	public virtual void AddDefaultResolver(IDbDependencyResolver resolver)
	{
		_rootResolver.AddDefaultResolver(resolver);
	}

	public virtual void SetDefaultProviderServices(DbProviderServices provider, string invariantName)
	{
		_rootResolver.SetDefaultProviderServices(provider, invariantName);
	}

	public virtual void RegisterSingleton<TService>(TService instance) where TService : class
	{
		AddDependencyResolver(new SingletonDependencyResolver<TService>(instance, (object)null));
	}

	public virtual void RegisterSingleton<TService>(TService instance, object key) where TService : class
	{
		AddDependencyResolver(new SingletonDependencyResolver<TService>(instance, key));
	}

	public virtual void RegisterSingleton<TService>(TService instance, Func<object, bool> keyPredicate) where TService : class
	{
		AddDependencyResolver(new SingletonDependencyResolver<TService>(instance, keyPredicate));
	}

	public virtual TService GetService<TService>(object key)
	{
		return _resolvers.GetService<TService>(key);
	}

	public virtual void SwitchInRootResolver(RootDependencyResolver value)
	{
		ResolverChain resolverChain = new ResolverChain();
		resolverChain.Add(value);
		_resolvers.Second.Resolvers.Skip(1).Each(resolverChain.Add);
		_rootResolver = value;
		_resolvers = new CompositeResolver<ResolverChain, ResolverChain>(_resolvers.First, resolverChain);
	}

	public virtual void CheckNotLocked(string memberName)
	{
		if (_isLocked)
		{
			throw new InvalidOperationException(Strings.ConfigurationLocked(memberName));
		}
	}
}
