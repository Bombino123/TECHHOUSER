using System.Data.Entity.Utilities;

namespace System.Data.Entity.Migrations.Model;

public class DropProcedureOperation : MigrationOperation
{
	private readonly string _name;

	public virtual string Name => _name;

	public override MigrationOperation Inverse => NotSupportedOperation.Instance;

	public override bool IsDestructiveChange => false;

	public DropProcedureOperation(string name, object anonymousArguments = null)
		: base(anonymousArguments)
	{
		Check.NotEmpty(name, "name");
		_name = name;
	}
}
