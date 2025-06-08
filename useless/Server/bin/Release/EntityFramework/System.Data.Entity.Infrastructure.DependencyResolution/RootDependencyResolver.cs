using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.Infrastructure.Pluralization;
using System.Data.Entity.Internal;
using System.Data.Entity.Migrations.History;
using System.Data.Entity.ModelConfiguration.Utilities;
using System.Linq;

namespace System.Data.Entity.Infrastructure.DependencyResolution;

internal class RootDependencyResolver : IDbDependencyResolver
{
	private readonly ResolverChain _defaultProviderResolvers = new ResolverChain();

	private readonly ResolverChain _defaultResolvers = new ResolverChain();

	private readonly ResolverChain _resolvers = new ResolverChain();

	private readonly DatabaseInitializerResolver _databaseInitializerResolver;

	public DatabaseInitializerResolver DatabaseInitializerResolver => _databaseInitializerResolver;

	public RootDependencyResolver()
		: this(new DefaultProviderServicesResolver(), new DatabaseInitializerResolver())
	{
	}

	public RootDependencyResolver(DefaultProviderServicesResolver defaultProviderServicesResolver, DatabaseInitializerResolver databaseInitializerResolver)
	{
		_databaseInitializerResolver = databaseInitializerResolver;
		_resolvers.Add(new TransactionContextInitializerResolver());
		_resolvers.Add(_databaseInitializerResolver);
		_resolvers.Add(new DefaultExecutionStrategyResolver());
		_resolvers.Add(new CachingDependencyResolver(defaultProviderServicesResolver));
		_resolvers.Add(new CachingDependencyResolver(new DefaultProviderFactoryResolver()));
		_resolvers.Add(new CachingDependencyResolver(new DefaultInvariantNameResolver()));
		_resolvers.Add(new SingletonDependencyResolver<IDbConnectionFactory>(new LocalDbConnectionFactory()));
		_resolvers.Add(new SingletonDependencyResolver<Func<DbContext, IDbModelCacheKey>>(new DefaultModelCacheKeyFactory().Create));
		_resolvers.Add(new SingletonDependencyResolver<IManifestTokenResolver>(new DefaultManifestTokenResolver()));
		_resolvers.Add(new SingletonDependencyResolver<Func<DbConnection, string, HistoryContext>>(HistoryContext.DefaultFactory));
		_resolvers.Add(new SingletonDependencyResolver<IPluralizationService>(new EnglishPluralizationService()));
		_resolvers.Add(new SingletonDependencyResolver<AttributeProvider>(new AttributeProvider()));
		_resolvers.Add(new SingletonDependencyResolver<Func<DbContext, Action<string>, DatabaseLogFormatter>>((DbContext c, Action<string> w) => new DatabaseLogFormatter(c, w)));
		_resolvers.Add(new SingletonDependencyResolver<Func<TransactionHandler>>(() => new DefaultTransactionHandler(), (object k) => k is ExecutionStrategyKey));
		_resolvers.Add(new SingletonDependencyResolver<IDbProviderFactoryResolver>(new DefaultDbProviderFactoryResolver()));
		_resolvers.Add(new SingletonDependencyResolver<Func<IMetadataAnnotationSerializer>>(() => new ClrTypeAnnotationSerializer(), "ClrType"));
		_resolvers.Add(new SingletonDependencyResolver<Func<IMetadataAnnotationSerializer>>(() => new IndexAnnotationSerializer(), "Index"));
	}

	public virtual object GetService(Type type, object key)
	{
		return _defaultResolvers.GetService(type, key) ?? _defaultProviderResolvers.GetService(type, key) ?? _resolvers.GetService(type, key);
	}

	public virtual void AddDefaultResolver(IDbDependencyResolver resolver)
	{
		_defaultResolvers.Add(resolver);
	}

	public virtual void SetDefaultProviderServices(DbProviderServices provider, string invariantName)
	{
		_defaultProviderResolvers.Add(new SingletonDependencyResolver<DbProviderServices>(provider, invariantName));
		_defaultProviderResolvers.Add(provider);
	}

	public IEnumerable<object> GetServices(Type type, object key)
	{
		return _defaultResolvers.GetServices(type, key).Concat(_resolvers.GetServices(type, key));
	}
}
