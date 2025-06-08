using System.Configuration;
using System.Data.Common;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.Internal;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace System.Data.Entity.Infrastructure;

public class DbContextInfo
{
	[ThreadStatic]
	private static DbContextInfo _currentInfo;

	private readonly Type _contextType;

	private readonly DbProviderInfo _modelProviderInfo;

	private readonly DbConnectionInfo _connectionInfo;

	private readonly AppConfig _appConfig;

	private readonly Func<DbContext> _activator;

	private readonly string _connectionString;

	private readonly string _connectionProviderName;

	private readonly bool _isConstructible;

	private readonly DbConnectionStringOrigin _connectionStringOrigin;

	private readonly string _connectionStringName;

	private readonly Func<IDbDependencyResolver> _resolver = () => DbConfiguration.DependencyResolver;

	private Action<DbModelBuilder> _onModelCreating;

	public virtual Type ContextType => _contextType;

	public virtual bool IsConstructible => _isConstructible;

	public virtual string ConnectionString => _connectionString;

	public virtual string ConnectionStringName => _connectionStringName;

	public virtual string ConnectionProviderName => _connectionProviderName;

	public virtual DbConnectionStringOrigin ConnectionStringOrigin => _connectionStringOrigin;

	public virtual Action<DbModelBuilder> OnModelCreating
	{
		get
		{
			return _onModelCreating;
		}
		set
		{
			_onModelCreating = value;
		}
	}

	internal static DbContextInfo CurrentInfo
	{
		get
		{
			return _currentInfo;
		}
		set
		{
			_currentInfo = value;
		}
	}

	public DbContextInfo(Type contextType)
		: this(contextType, (Func<IDbDependencyResolver>)null)
	{
	}

	internal DbContextInfo(Type contextType, Func<IDbDependencyResolver> resolver)
		: this(Check.NotNull(contextType, "contextType"), null, AppConfig.DefaultInstance, null, resolver)
	{
	}

	public DbContextInfo(Type contextType, DbConnectionInfo connectionInfo)
		: this(Check.NotNull(contextType, "contextType"), null, AppConfig.DefaultInstance, Check.NotNull(connectionInfo, "connectionInfo"))
	{
	}

	[Obsolete("The application configuration can contain multiple settings that affect the connection used by a DbContext. To ensure all configuration is taken into account, use a DbContextInfo constructor that accepts System.Configuration.Configuration")]
	public DbContextInfo(Type contextType, ConnectionStringSettingsCollection connectionStringSettings)
		: this(Check.NotNull(contextType, "contextType"), null, new AppConfig(Check.NotNull<ConnectionStringSettingsCollection>(connectionStringSettings, "connectionStringSettings")), null)
	{
	}

	public DbContextInfo(Type contextType, Configuration config)
		: this(Check.NotNull(contextType, "contextType"), null, new AppConfig(Check.NotNull<Configuration>(config, "config")), null)
	{
	}

	public DbContextInfo(Type contextType, Configuration config, DbConnectionInfo connectionInfo)
		: this(Check.NotNull(contextType, "contextType"), null, new AppConfig(Check.NotNull<Configuration>(config, "config")), Check.NotNull(connectionInfo, "connectionInfo"))
	{
	}

	public DbContextInfo(Type contextType, DbProviderInfo modelProviderInfo)
		: this(Check.NotNull(contextType, "contextType"), Check.NotNull(modelProviderInfo, "modelProviderInfo"), AppConfig.DefaultInstance, null)
	{
	}

	public DbContextInfo(Type contextType, Configuration config, DbProviderInfo modelProviderInfo)
		: this(Check.NotNull(contextType, "contextType"), Check.NotNull(modelProviderInfo, "modelProviderInfo"), new AppConfig(Check.NotNull<Configuration>(config, "config")), null)
	{
	}

	internal DbContextInfo(DbContext context, Func<IDbDependencyResolver> resolver = null)
	{
		Check.NotNull(context, "context");
		_resolver = resolver ?? ((Func<IDbDependencyResolver>)(() => DbConfiguration.DependencyResolver));
		_contextType = context.GetType();
		_appConfig = AppConfig.DefaultInstance;
		InternalContext internalContext = context.InternalContext;
		_connectionProviderName = internalContext.ProviderName;
		_connectionInfo = new DbConnectionInfo(internalContext.OriginalConnectionString, _connectionProviderName);
		_connectionString = internalContext.OriginalConnectionString;
		_connectionStringName = internalContext.ConnectionStringName;
		_connectionStringOrigin = internalContext.ConnectionStringOrigin;
	}

	private DbContextInfo(Type contextType, DbProviderInfo modelProviderInfo, AppConfig config, DbConnectionInfo connectionInfo, Func<IDbDependencyResolver> resolver = null)
	{
		if (!typeof(DbContext).IsAssignableFrom(contextType))
		{
			throw new ArgumentOutOfRangeException("contextType");
		}
		_resolver = resolver ?? ((Func<IDbDependencyResolver>)(() => DbConfiguration.DependencyResolver));
		_contextType = contextType;
		_modelProviderInfo = modelProviderInfo;
		_appConfig = config;
		_connectionInfo = connectionInfo;
		_activator = CreateActivator();
		if (_activator == null)
		{
			return;
		}
		DbContext dbContext = CreateInstance();
		if (dbContext != null)
		{
			_isConstructible = true;
			using (dbContext)
			{
				_connectionString = DbInterception.Dispatch.Connection.GetConnectionString(dbContext.InternalContext.Connection, new DbInterceptionContext().WithDbContext(dbContext));
				_connectionStringName = dbContext.InternalContext.ConnectionStringName;
				_connectionProviderName = dbContext.InternalContext.ProviderName;
				_connectionStringOrigin = dbContext.InternalContext.ConnectionStringOrigin;
			}
		}
	}

	public virtual DbContext CreateInstance()
	{
		bool flag = DbConfigurationManager.Instance.PushConfiguration(_appConfig, _contextType);
		CurrentInfo = this;
		DbContext dbContext = null;
		try
		{
			try
			{
				dbContext = ((_activator == null) ? null : _activator());
			}
			catch (TargetInvocationException ex)
			{
				ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
				throw ex.InnerException;
			}
			if (dbContext == null)
			{
				return null;
			}
			dbContext.InternalContext.OnDisposing += delegate
			{
				CurrentInfo = null;
			};
			if (flag)
			{
				dbContext.InternalContext.OnDisposing += delegate
				{
					DbConfigurationManager.Instance.PopConfiguration(_appConfig);
				};
			}
			dbContext.InternalContext.ApplyContextInfo(this);
			return dbContext;
		}
		catch (Exception)
		{
			dbContext?.Dispose();
			throw;
		}
		finally
		{
			if (dbContext == null)
			{
				CurrentInfo = null;
				if (flag)
				{
					DbConfigurationManager.Instance.PopConfiguration(_appConfig);
				}
			}
		}
	}

	internal void ConfigureContext(DbContext context)
	{
		if (_modelProviderInfo != null)
		{
			context.InternalContext.ModelProviderInfo = _modelProviderInfo;
		}
		context.InternalContext.AppConfig = _appConfig;
		if (_connectionInfo != null)
		{
			context.InternalContext.OverrideConnection(new LazyInternalConnection(context, _connectionInfo));
		}
		else if (_modelProviderInfo != null && _appConfig == AppConfig.DefaultInstance)
		{
			context.InternalContext.OverrideConnection(new EagerInternalConnection(context, _resolver().GetService<DbProviderFactory>(_modelProviderInfo.ProviderInvariantName).CreateConnection(), connectionOwned: true));
		}
		if (_onModelCreating != null)
		{
			context.InternalContext.OnModelCreating = _onModelCreating;
		}
	}

	private Func<DbContext> CreateActivator()
	{
		if (_contextType.GetPublicConstructor() != null)
		{
			return () => (DbContext)Activator.CreateInstance(_contextType);
		}
		Func<DbContext> service = _resolver().GetService<Func<DbContext>>(_contextType);
		if (service != null)
		{
			return service;
		}
		Type type = (from t in _contextType.Assembly().GetAccessibleTypes()
			where t.IsClass() && typeof(IDbContextFactory<>).MakeGenericType(_contextType).IsAssignableFrom(t)
			select t).FirstOrDefault();
		if (type == null)
		{
			return null;
		}
		if (type.GetPublicConstructor() == null)
		{
			throw Error.DbContextServices_MissingDefaultCtor(type);
		}
		return ((IDbContextFactory<DbContext>)Activator.CreateInstance(type)).Create;
	}
}
