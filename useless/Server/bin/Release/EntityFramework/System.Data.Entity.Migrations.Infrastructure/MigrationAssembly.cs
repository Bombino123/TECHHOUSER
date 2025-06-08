using System.Collections.Generic;
using System.Data.Entity.Migrations.Utilities;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.Migrations.Infrastructure;

internal class MigrationAssembly
{
	private readonly IList<IMigrationMetadata> _migrations;

	public virtual IEnumerable<string> MigrationIds => _migrations.Select((IMigrationMetadata t) => t.Id).ToList();

	public static string CreateMigrationId(string migrationName)
	{
		return UtcNowGenerator.UtcNowAsMigrationIdTimestamp() + "_" + migrationName;
	}

	public static string CreateBootstrapMigrationId()
	{
		return new string('0', 15) + "_" + Strings.BootstrapMigration;
	}

	protected MigrationAssembly()
	{
	}

	public MigrationAssembly(Assembly migrationsAssembly, string migrationsNamespace)
	{
		_migrations = (from t in migrationsAssembly.GetAccessibleTypes()
			where t.IsSubclassOf(typeof(DbMigration)) && typeof(IMigrationMetadata).IsAssignableFrom(t) && t.GetPublicConstructor() != null && !t.IsAbstract() && !t.IsGenericType() && t.Namespace == migrationsNamespace
			select (IMigrationMetadata)Activator.CreateInstance(t) into mm
			where !string.IsNullOrWhiteSpace(mm.Id) && mm.Id.IsValidMigrationId()
			orderby mm.Id
			select mm).ToList();
	}

	public virtual string UniquifyName(string migrationName)
	{
		return _migrations.Select((IMigrationMetadata m) => m.GetType().Name).Uniquify(migrationName);
	}

	public virtual DbMigration GetMigration(string migrationId)
	{
		DbMigration dbMigration = (DbMigration)_migrations.SingleOrDefault((IMigrationMetadata m) => m.Id.StartsWith(migrationId, StringComparison.Ordinal));
		dbMigration?.Reset();
		return dbMigration;
	}
}
