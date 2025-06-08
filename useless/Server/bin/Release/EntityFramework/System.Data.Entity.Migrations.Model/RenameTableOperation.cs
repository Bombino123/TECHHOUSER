using System.Data.Entity.Utilities;

namespace System.Data.Entity.Migrations.Model;

public class RenameTableOperation : MigrationOperation
{
	private readonly string _name;

	private string _newName;

	public virtual string Name => _name;

	public virtual string NewName
	{
		get
		{
			return _newName;
		}
		internal set
		{
			_newName = value;
		}
	}

	public override MigrationOperation Inverse
	{
		get
		{
			DatabaseName databaseName = DatabaseName.Parse(_name);
			return new RenameTableOperation(new DatabaseName(DatabaseName.Parse(_newName).Name, databaseName.Schema).ToString(), databaseName.Name);
		}
	}

	public override bool IsDestructiveChange => false;

	public RenameTableOperation(string name, string newName, object anonymousArguments = null)
		: base(anonymousArguments)
	{
		Check.NotEmpty(name, "name");
		Check.NotEmpty(newName, "newName");
		_name = name;
		_newName = newName;
	}
}
