using System.Data.Entity.Utilities;

namespace System.Data.Entity.Migrations.Model;

public class AddPrimaryKeyOperation : PrimaryKeyOperation
{
	public override MigrationOperation Inverse
	{
		get
		{
			DropPrimaryKeyOperation dropPrimaryKeyOperation = new DropPrimaryKeyOperation
			{
				Name = base.Name,
				Table = base.Table,
				IsClustered = base.IsClustered
			};
			base.Columns.Each(delegate(string c)
			{
				dropPrimaryKeyOperation.Columns.Add(c);
			});
			return dropPrimaryKeyOperation;
		}
	}

	public AddPrimaryKeyOperation(object anonymousArguments = null)
		: base(anonymousArguments)
	{
	}
}
