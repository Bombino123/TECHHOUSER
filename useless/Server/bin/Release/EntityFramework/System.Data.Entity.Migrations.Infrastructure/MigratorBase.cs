using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.Migrations.Sql;
using System.Diagnostics;
using System.Xml.Linq;

namespace System.Data.Entity.Migrations.Infrastructure;

[DebuggerStepThrough]
public abstract class MigratorBase
{
	private MigratorBase _this;

	public virtual DbMigrationsConfiguration Configuration => _this.Configuration;

	internal virtual string TargetDatabase => _this.TargetDatabase;

	protected MigratorBase(MigratorBase innerMigrator)
	{
		if (innerMigrator == null)
		{
			_this = this;
			return;
		}
		_this = innerMigrator;
		MigratorBase migratorBase = innerMigrator;
		while (migratorBase._this != innerMigrator)
		{
			migratorBase = migratorBase._this;
		}
		migratorBase._this = this;
	}

	public virtual IEnumerable<string> GetPendingMigrations()
	{
		return _this.GetPendingMigrations();
	}

	public void Update()
	{
		Update(null);
	}

	public virtual void Update(string targetMigration)
	{
		_this.Update(targetMigration);
	}

	internal virtual string GetMigrationId(string migration)
	{
		return _this.GetMigrationId(migration);
	}

	public virtual IEnumerable<string> GetLocalMigrations()
	{
		return _this.GetLocalMigrations();
	}

	public virtual IEnumerable<string> GetDatabaseMigrations()
	{
		return _this.GetDatabaseMigrations();
	}

	internal virtual void AutoMigrate(string migrationId, VersionedModel sourceModel, VersionedModel targetModel, bool downgrading)
	{
		_this.AutoMigrate(migrationId, sourceModel, targetModel, downgrading);
	}

	internal virtual void ApplyMigration(DbMigration migration, DbMigration lastMigration)
	{
		_this.ApplyMigration(migration, lastMigration);
	}

	internal virtual void EnsureDatabaseExists(Action mustSucceedToKeepDatabase)
	{
		_this.EnsureDatabaseExists(mustSucceedToKeepDatabase);
	}

	internal virtual void RevertMigration(string migrationId, DbMigration migration, XDocument targetModel)
	{
		_this.RevertMigration(migrationId, migration, targetModel);
	}

	internal virtual void SeedDatabase()
	{
		_this.SeedDatabase();
	}

	internal virtual void ExecuteStatements(IEnumerable<MigrationStatement> migrationStatements)
	{
		_this.ExecuteStatements(migrationStatements);
	}

	internal virtual IEnumerable<MigrationStatement> GenerateStatements(IList<MigrationOperation> operations, string migrationId)
	{
		return _this.GenerateStatements(operations, migrationId);
	}

	internal virtual IEnumerable<DbQueryCommandTree> CreateDiscoveryQueryTrees()
	{
		return _this.CreateDiscoveryQueryTrees();
	}

	internal virtual void ExecuteSql(MigrationStatement migrationStatement, DbConnection connection, DbTransaction transaction, DbInterceptionContext interceptionContext)
	{
		_this.ExecuteSql(migrationStatement, connection, transaction, interceptionContext);
	}

	internal virtual void Upgrade(IEnumerable<string> pendingMigrations, string targetMigrationId, string lastMigrationId)
	{
		_this.Upgrade(pendingMigrations, targetMigrationId, lastMigrationId);
	}

	internal virtual void Downgrade(IEnumerable<string> pendingMigrations)
	{
		_this.Downgrade(pendingMigrations);
	}

	internal virtual void UpgradeHistory(IEnumerable<MigrationOperation> upgradeOperations)
	{
		_this.UpgradeHistory(upgradeOperations);
	}

	internal virtual bool HistoryExists()
	{
		return _this.HistoryExists();
	}
}
