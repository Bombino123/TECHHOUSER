using System.Data.Entity.Migrations.Model;

namespace System.Data.Entity.Migrations.Infrastructure;

public interface IDbMigration
{
	void AddOperation(MigrationOperation migrationOperation);
}
