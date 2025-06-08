using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.Migrations.Sql;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Xml.Linq;

namespace System.Data.Entity.Migrations.Infrastructure;

public class MigratorLoggingDecorator : MigratorBase
{
	private readonly MigrationsLogger _logger;

	private string _lastInfoMessage;

	public MigratorLoggingDecorator(MigratorBase innerMigrator, MigrationsLogger logger)
		: base(innerMigrator)
	{
		Check.NotNull(innerMigrator, "innerMigrator");
		Check.NotNull(logger, "logger");
		_logger = logger;
		_logger.Verbose(Strings.LoggingTargetDatabase(base.TargetDatabase));
	}

	internal override void AutoMigrate(string migrationId, VersionedModel sourceModel, VersionedModel targetModel, bool downgrading)
	{
		_logger.Info(downgrading ? Strings.LoggingRevertAutoMigrate(migrationId) : Strings.LoggingAutoMigrate(migrationId));
		base.AutoMigrate(migrationId, sourceModel, targetModel, downgrading);
	}

	internal override void ExecuteSql(MigrationStatement migrationStatement, DbConnection connection, DbTransaction transaction, DbInterceptionContext interceptionContext)
	{
		_logger.Verbose(migrationStatement.Sql);
		DbProviderServices.GetProviderServices(connection)?.RegisterInfoMessageHandler(connection, delegate(string message)
		{
			if (!string.Equals(message, _lastInfoMessage, StringComparison.OrdinalIgnoreCase))
			{
				_logger.Warning(message);
				_lastInfoMessage = message;
			}
		});
		base.ExecuteSql(migrationStatement, connection, transaction, interceptionContext);
	}

	internal override void Upgrade(IEnumerable<string> pendingMigrations, string targetMigrationId, string lastMigrationId)
	{
		int num = pendingMigrations.Count();
		_logger.Info((num > 0) ? Strings.LoggingPendingMigrations(num, pendingMigrations.Join()) : (string.IsNullOrWhiteSpace(targetMigrationId) ? Strings.LoggingNoExplicitMigrations : Strings.LoggingAlreadyAtTarget(targetMigrationId)));
		base.Upgrade(pendingMigrations, targetMigrationId, lastMigrationId);
	}

	internal override void Downgrade(IEnumerable<string> pendingMigrations)
	{
		IEnumerable<string> enumerable = pendingMigrations.Take(pendingMigrations.Count() - 1);
		_logger.Info(Strings.LoggingPendingMigrationsDown(enumerable.Count(), enumerable.Join()));
		base.Downgrade(pendingMigrations);
	}

	internal override void ApplyMigration(DbMigration migration, DbMigration lastMigration)
	{
		_logger.Info(Strings.LoggingApplyMigration(((IMigrationMetadata)migration).Id));
		base.ApplyMigration(migration, lastMigration);
	}

	internal override void RevertMigration(string migrationId, DbMigration migration, XDocument targetModel)
	{
		_logger.Info(Strings.LoggingRevertMigration(migrationId));
		base.RevertMigration(migrationId, migration, targetModel);
	}

	internal override void SeedDatabase()
	{
		_logger.Info(Strings.LoggingSeedingDatabase);
		base.SeedDatabase();
	}

	internal override void UpgradeHistory(IEnumerable<MigrationOperation> upgradeOperations)
	{
		_logger.Info(Strings.UpgradingHistoryTable);
		base.UpgradeHistory(upgradeOperations);
	}
}
