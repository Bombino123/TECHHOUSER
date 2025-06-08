using System.Data.Entity.Utilities;

namespace System.Data.Entity.Migrations.Model;

public class SqlOperation : MigrationOperation
{
	private readonly string _sql;

	public virtual string Sql => _sql;

	public virtual bool SuppressTransaction { get; set; }

	public override bool IsDestructiveChange => true;

	public SqlOperation(string sql, object anonymousArguments = null)
		: base(anonymousArguments)
	{
		Check.NotEmpty(sql, "sql");
		_sql = sql;
	}
}
