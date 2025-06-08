using System.Data.Entity.Utilities;

namespace System.Data.Entity.Migrations.Model;

public class RenameColumnOperation : MigrationOperation
{
	private readonly string _table;

	private readonly string _name;

	private string _newName;

	public virtual string Table => _table;

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

	public override MigrationOperation Inverse => new RenameColumnOperation(Table, NewName, Name);

	public override bool IsDestructiveChange => false;

	public RenameColumnOperation(string table, string name, string newName, object anonymousArguments = null)
		: base(anonymousArguments)
	{
		Check.NotEmpty(table, "table");
		Check.NotEmpty(name, "name");
		Check.NotEmpty(newName, "newName");
		_table = table;
		_name = name;
		_newName = newName;
	}
}
