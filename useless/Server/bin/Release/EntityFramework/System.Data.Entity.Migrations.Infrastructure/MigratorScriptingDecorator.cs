using System.Collections.Generic;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.Migrations.Sql;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Text;

namespace System.Data.Entity.Migrations.Infrastructure;

public class MigratorScriptingDecorator : MigratorBase
{
	private readonly StringBuilder _sqlBuilder = new StringBuilder();

	private UpdateDatabaseOperation _updateDatabaseOperation;

	public MigratorScriptingDecorator(MigratorBase innerMigrator)
		: base(innerMigrator)
	{
		Check.NotNull(innerMigrator, "innerMigrator");
	}

	public string ScriptUpdate(string sourceMigration, string targetMigration)
	{
		_sqlBuilder.Clear();
		if (string.IsNullOrWhiteSpace(sourceMigration))
		{
			Update(targetMigration);
		}
		else
		{
			if (sourceMigration.IsAutomaticMigration())
			{
				throw Error.AutoNotValidForScriptWindows(sourceMigration);
			}
			string sourceMigrationId = GetMigrationId(sourceMigration);
			IEnumerable<string> enumerable = from m in GetLocalMigrations()
				where string.CompareOrdinal(m, sourceMigrationId) > 0
				select m;
			string targetMigrationId = null;
			if (!string.IsNullOrWhiteSpace(targetMigration))
			{
				if (targetMigration.IsAutomaticMigration())
				{
					throw Error.AutoNotValidForScriptWindows(targetMigration);
				}
				targetMigrationId = GetMigrationId(targetMigration);
				if (string.CompareOrdinal(sourceMigrationId, targetMigrationId) > 0)
				{
					throw Error.DownScriptWindowsNotSupported();
				}
				enumerable = enumerable.Where((string m) => string.CompareOrdinal(m, targetMigrationId) <= 0);
			}
			_updateDatabaseOperation = ((sourceMigration == "0") ? new UpdateDatabaseOperation(base.CreateDiscoveryQueryTrees().ToList()) : null);
			Upgrade(enumerable, targetMigrationId, sourceMigrationId);
			if (_updateDatabaseOperation != null)
			{
				ExecuteStatements(base.GenerateStatements(new UpdateDatabaseOperation[1] { _updateDatabaseOperation }, null));
			}
		}
		return _sqlBuilder.ToString();
	}

	internal override IEnumerable<MigrationStatement> GenerateStatements(IList<MigrationOperation> operations, string migrationId)
	{
		if (_updateDatabaseOperation == null)
		{
			return base.GenerateStatements(operations, migrationId);
		}
		_updateDatabaseOperation.AddMigration(migrationId, operations);
		return Enumerable.Empty<MigrationStatement>();
	}

	internal override void EnsureDatabaseExists(Action mustSucceedToKeepDatabase)
	{
		mustSucceedToKeepDatabase();
	}

	internal override void ExecuteStatements(IEnumerable<MigrationStatement> migrationStatements)
	{
		BuildSqlScript(migrationStatements, _sqlBuilder);
	}

	internal static void BuildSqlScript(IEnumerable<MigrationStatement> migrationStatements, StringBuilder sqlBuilder)
	{
		foreach (MigrationStatement migrationStatement in migrationStatements)
		{
			if (!string.IsNullOrWhiteSpace(migrationStatement.Sql))
			{
				if (!string.IsNullOrWhiteSpace(migrationStatement.BatchTerminator) && sqlBuilder.Length > 0)
				{
					sqlBuilder.AppendLine(migrationStatement.BatchTerminator);
					sqlBuilder.AppendLine();
				}
				sqlBuilder.AppendLine(migrationStatement.Sql);
			}
		}
	}

	internal override void SeedDatabase()
	{
	}

	internal override bool HistoryExists()
	{
		return false;
	}
}
