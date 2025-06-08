using System.Data.Entity.Utilities;

namespace System.Data.Entity.Migrations.Model;

public class DropPrimaryKeyOperation : PrimaryKeyOperation
{
	public override MigrationOperation Inverse
	{
		get
		{
			AddPrimaryKeyOperation addPrimaryKeyOperation = new AddPrimaryKeyOperation
			{
				Name = base.Name,
				Table = base.Table,
				IsClustered = base.IsClustered
			};
			base.Columns.Each(delegate(string c)
			{
				addPrimaryKeyOperation.Columns.Add(c);
			});
			return addPrimaryKeyOperation;
		}
	}

	public CreateTableOperation CreateTableOperation { get; internal set; }

	public DropPrimaryKeyOperation(object anonymousArguments = null)
		: base(anonymousArguments)
	{
	}
}
