using System.Data.Entity.Utilities;

namespace System.Data.Entity.Migrations.Model;

public class CreateIndexOperation : IndexOperation
{
	public bool IsUnique { get; set; }

	public override MigrationOperation Inverse
	{
		get
		{
			DropIndexOperation dropIndexOperation = new DropIndexOperation(this)
			{
				Name = base.Name,
				Table = base.Table
			};
			base.Columns.Each(delegate(string c)
			{
				dropIndexOperation.Columns.Add(c);
			});
			return dropIndexOperation;
		}
	}

	public override bool IsDestructiveChange => false;

	public bool IsClustered { get; set; }

	public CreateIndexOperation(object anonymousArguments = null)
		: base(anonymousArguments)
	{
	}
}
