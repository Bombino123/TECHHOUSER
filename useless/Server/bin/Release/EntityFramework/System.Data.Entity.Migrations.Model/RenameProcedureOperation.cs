using System.Data.Entity.Utilities;

namespace System.Data.Entity.Migrations.Model;

public class RenameProcedureOperation : MigrationOperation
{
	private readonly string _name;

	private readonly string _newName;

	public virtual string Name => _name;

	public virtual string NewName => _newName;

	public override MigrationOperation Inverse
	{
		get
		{
			DatabaseName databaseName = DatabaseName.Parse(_name);
			return new RenameProcedureOperation(new DatabaseName(DatabaseName.Parse(_newName).Name, databaseName.Schema).ToString(), databaseName.Name);
		}
	}

	public override bool IsDestructiveChange => false;

	public RenameProcedureOperation(string name, string newName, object anonymousArguments = null)
		: base(anonymousArguments)
	{
		Check.NotEmpty(name, "name");
		Check.NotEmpty(newName, "newName");
		_name = name;
		_newName = newName;
	}
}
