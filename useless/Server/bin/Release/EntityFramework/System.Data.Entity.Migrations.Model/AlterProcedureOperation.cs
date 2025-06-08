namespace System.Data.Entity.Migrations.Model;

public class AlterProcedureOperation : ProcedureOperation
{
	public override MigrationOperation Inverse => NotSupportedOperation.Instance;

	public AlterProcedureOperation(string name, string bodySql, object anonymousArguments = null)
		: base(name, bodySql, anonymousArguments)
	{
	}
}
