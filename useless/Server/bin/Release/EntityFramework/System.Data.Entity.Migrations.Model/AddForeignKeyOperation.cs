using System.Collections.Generic;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Migrations.Model;

public class AddForeignKeyOperation : ForeignKeyOperation
{
	private readonly List<string> _principalColumns = new List<string>();

	public IList<string> PrincipalColumns => _principalColumns;

	public bool CascadeDelete { get; set; }

	public override MigrationOperation Inverse
	{
		get
		{
			DropForeignKeyOperation dropForeignKeyOperation = new DropForeignKeyOperation
			{
				Name = base.Name,
				PrincipalTable = base.PrincipalTable,
				DependentTable = base.DependentTable
			};
			base.DependentColumns.Each(delegate(string c)
			{
				dropForeignKeyOperation.DependentColumns.Add(c);
			});
			return dropForeignKeyOperation;
		}
	}

	public override bool IsDestructiveChange => false;

	public AddForeignKeyOperation(object anonymousArguments = null)
		: base(anonymousArguments)
	{
	}

	public virtual CreateIndexOperation CreateCreateIndexOperation()
	{
		CreateIndexOperation createIndexOperation = new CreateIndexOperation
		{
			Table = base.DependentTable
		};
		base.DependentColumns.Each(delegate(string c)
		{
			createIndexOperation.Columns.Add(c);
		});
		return createIndexOperation;
	}
}
