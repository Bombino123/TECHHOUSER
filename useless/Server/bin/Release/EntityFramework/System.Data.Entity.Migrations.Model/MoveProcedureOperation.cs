using System.Data.Entity.Utilities;

namespace System.Data.Entity.Migrations.Model;

public class MoveProcedureOperation : MigrationOperation
{
	private readonly string _name;

	private readonly string _newSchema;

	public virtual string Name => _name;

	public virtual string NewSchema => _newSchema;

	public override MigrationOperation Inverse
	{
		get
		{
			DatabaseName databaseName = DatabaseName.Parse(_name);
			return new MoveProcedureOperation(new DatabaseName(databaseName.Name, NewSchema).ToString(), databaseName.Schema);
		}
	}

	public override bool IsDestructiveChange => false;

	public MoveProcedureOperation(string name, string newSchema, object anonymousArguments = null)
		: base(anonymousArguments)
	{
		Check.NotEmpty(name, "name");
		_name = name;
		_newSchema = newSchema;
	}
}
