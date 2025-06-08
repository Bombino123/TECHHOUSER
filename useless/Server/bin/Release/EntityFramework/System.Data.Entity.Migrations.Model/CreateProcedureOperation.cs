namespace System.Data.Entity.Migrations.Model;

public class CreateProcedureOperation : ProcedureOperation
{
	public override MigrationOperation Inverse => new DropProcedureOperation(Name);

	public CreateProcedureOperation(string name, string bodySql, object anonymousArguments = null)
		: base(name, bodySql, anonymousArguments)
	{
	}
}
