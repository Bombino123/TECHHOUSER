using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Migrations.Model;

public class UpdateDatabaseOperation : MigrationOperation
{
	public class Migration
	{
		private readonly string _migrationId;

		private readonly IList<MigrationOperation> _operations;

		public string MigrationId => _migrationId;

		public IList<MigrationOperation> Operations => _operations;

		internal Migration(string migrationId, IList<MigrationOperation> operations)
		{
			_migrationId = migrationId;
			_operations = operations;
		}
	}

	private readonly IList<DbQueryCommandTree> _historyQueryTrees;

	private readonly IList<Migration> _migrations = new List<Migration>();

	public IList<DbQueryCommandTree> HistoryQueryTrees => _historyQueryTrees;

	public IList<Migration> Migrations => _migrations;

	public override bool IsDestructiveChange => false;

	public UpdateDatabaseOperation(IList<DbQueryCommandTree> historyQueryTrees)
		: base(null)
	{
		Check.NotNull(historyQueryTrees, "historyQueryTrees");
		_historyQueryTrees = historyQueryTrees;
	}

	public void AddMigration(string migrationId, IList<MigrationOperation> operations)
	{
		Check.NotEmpty(migrationId, "migrationId");
		Check.NotNull(operations, "operations");
		_migrations.Add(new Migration(migrationId, operations));
	}
}
