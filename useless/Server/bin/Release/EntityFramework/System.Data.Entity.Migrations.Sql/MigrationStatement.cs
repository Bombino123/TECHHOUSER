namespace System.Data.Entity.Migrations.Sql;

public class MigrationStatement
{
	public string Sql { get; set; }

	public bool SuppressTransaction { get; set; }

	public string BatchTerminator { get; set; }
}
