using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Infrastructure;
using System.Data.Entity.Migrations.Sql;

namespace System.Data.Entity.Internal;

internal class DatabaseCreator
{
	private readonly IDbDependencyResolver _resolver;

	public DatabaseCreator()
		: this(DbConfiguration.DependencyResolver)
	{
	}

	public DatabaseCreator(IDbDependencyResolver resolver)
	{
		_resolver = resolver;
	}

	public virtual void CreateDatabase(InternalContext internalContext, Func<DbMigrationsConfiguration, DbContext, MigratorBase> createMigrator, ObjectContext objectContext)
	{
		if (internalContext.CodeFirstModel != null && _resolver.GetService<Func<MigrationSqlGenerator>>(internalContext.ProviderName) != null)
		{
			createMigrator(internalContext.MigrationsConfiguration, internalContext.Owner).Update();
		}
		else
		{
			internalContext.DatabaseOperations.Create(objectContext);
			internalContext.SaveMetadataToDatabase();
		}
		internalContext.MarkDatabaseInitialized();
	}
}
