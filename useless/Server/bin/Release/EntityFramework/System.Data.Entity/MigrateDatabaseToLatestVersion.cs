using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Migrations;
using System.Data.Entity.Utilities;

namespace System.Data.Entity;

public class MigrateDatabaseToLatestVersion<TContext, TMigrationsConfiguration> : IDatabaseInitializer<TContext> where TContext : DbContext where TMigrationsConfiguration : DbMigrationsConfiguration<TContext>, new()
{
	private readonly DbMigrationsConfiguration _config;

	private readonly bool _useSuppliedContext;

	static MigrateDatabaseToLatestVersion()
	{
		DbConfigurationManager.Instance.EnsureLoadedForContext(typeof(TContext));
	}

	public MigrateDatabaseToLatestVersion()
		: this(useSuppliedContext: false)
	{
	}

	public MigrateDatabaseToLatestVersion(bool useSuppliedContext)
		: this(useSuppliedContext, new TMigrationsConfiguration())
	{
	}

	public MigrateDatabaseToLatestVersion(bool useSuppliedContext, TMigrationsConfiguration configuration)
	{
		Check.NotNull(configuration, "configuration");
		_config = configuration;
		_useSuppliedContext = useSuppliedContext;
	}

	public MigrateDatabaseToLatestVersion(string connectionStringName)
	{
		Check.NotEmpty(connectionStringName, "connectionStringName");
		_config = new TMigrationsConfiguration
		{
			TargetDatabase = new DbConnectionInfo(connectionStringName)
		};
	}

	public virtual void InitializeDatabase(TContext context)
	{
		Check.NotNull(context, "context");
		new DbMigrator(_config, _useSuppliedContext ? context : null).Update();
	}
}
