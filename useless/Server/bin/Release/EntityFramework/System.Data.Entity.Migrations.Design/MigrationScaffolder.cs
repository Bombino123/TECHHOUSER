using System.Data.Entity.Utilities;

namespace System.Data.Entity.Migrations.Design;

public class MigrationScaffolder
{
	private readonly DbMigrator _migrator;

	private string _namespace;

	private bool _namespaceSpecified;

	public string Namespace
	{
		get
		{
			if (!_namespaceSpecified)
			{
				return _migrator.Configuration.MigrationsNamespace;
			}
			return _namespace;
		}
		set
		{
			_namespaceSpecified = _migrator.Configuration.MigrationsNamespace != value;
			_namespace = value;
		}
	}

	public MigrationScaffolder(DbMigrationsConfiguration migrationsConfiguration)
	{
		Check.NotNull(migrationsConfiguration, "migrationsConfiguration");
		_migrator = new DbMigrator(migrationsConfiguration);
	}

	public virtual ScaffoldedMigration Scaffold(string migrationName)
	{
		Check.NotEmpty(migrationName, "migrationName");
		return _migrator.Scaffold(migrationName, Namespace, ignoreChanges: false);
	}

	public virtual ScaffoldedMigration Scaffold(string migrationName, bool ignoreChanges)
	{
		Check.NotEmpty(migrationName, "migrationName");
		return _migrator.Scaffold(migrationName, Namespace, ignoreChanges);
	}

	public virtual ScaffoldedMigration ScaffoldInitialCreate()
	{
		return _migrator.ScaffoldInitialCreate(Namespace);
	}
}
