using System.Data.Entity.Utilities;

namespace System.Data.Entity.Migrations.Model;

public class MoveTableOperation : MigrationOperation
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
			return new MoveTableOperation(new DatabaseName(databaseName.Name, NewSchema).ToString(), databaseName.Schema)
			{
				IsSystem = IsSystem
			};
		}
	}

	public override bool IsDestructiveChange => false;

	public string ContextKey { get; internal set; }

	public bool IsSystem { get; internal set; }

	public CreateTableOperation CreateTableOperation { get; internal set; }

	public MoveTableOperation(string name, string newSchema, object anonymousArguments = null)
		: base(anonymousArguments)
	{
		Check.NotEmpty(name, "name");
		_name = name;
		_newSchema = newSchema;
	}
}
