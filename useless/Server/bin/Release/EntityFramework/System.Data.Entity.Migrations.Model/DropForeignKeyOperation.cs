using System.Data.Entity.Utilities;

namespace System.Data.Entity.Migrations.Model;

public class DropForeignKeyOperation : ForeignKeyOperation
{
	private readonly AddForeignKeyOperation _inverse;

	public override MigrationOperation Inverse => _inverse;

	public override bool IsDestructiveChange => false;

	public DropForeignKeyOperation(object anonymousArguments = null)
		: base(anonymousArguments)
	{
	}

	public DropForeignKeyOperation(AddForeignKeyOperation inverse, object anonymousArguments = null)
		: base(anonymousArguments)
	{
		Check.NotNull(inverse, "inverse");
		_inverse = inverse;
	}

	public virtual DropIndexOperation CreateDropIndexOperation()
	{
		DropIndexOperation dropIndexOperation = new DropIndexOperation(_inverse.CreateCreateIndexOperation())
		{
			Table = base.DependentTable
		};
		base.DependentColumns.Each(delegate(string c)
		{
			dropIndexOperation.Columns.Add(c);
		});
		return dropIndexOperation;
	}
}
