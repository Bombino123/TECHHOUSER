using System.ComponentModel;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.ModelConfiguration.Utilities;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Linq.Expressions;

namespace System.Data.Entity.Migrations.Builders;

public class TableBuilder<TColumns>
{
	private readonly CreateTableOperation _createTableOperation;

	private readonly DbMigration _migration;

	public TableBuilder(CreateTableOperation createTableOperation, DbMigration migration)
	{
		Check.NotNull(createTableOperation, "createTableOperation");
		_createTableOperation = createTableOperation;
		_migration = migration;
	}

	public TableBuilder<TColumns> PrimaryKey(Expression<Func<TColumns, object>> keyExpression, string name = null, bool clustered = true, object anonymousArguments = null)
	{
		Check.NotNull(keyExpression, "keyExpression");
		AddPrimaryKeyOperation addPrimaryKeyOperation = new AddPrimaryKeyOperation(anonymousArguments)
		{
			Name = name,
			IsClustered = clustered
		};
		(from p in keyExpression.GetSimplePropertyAccessList()
			select _createTableOperation.Columns.Single((ColumnModel c) => c.ApiPropertyInfo == p.Single())).Each(delegate(ColumnModel c)
		{
			addPrimaryKeyOperation.Columns.Add(c.Name);
		});
		_createTableOperation.PrimaryKey = addPrimaryKeyOperation;
		return this;
	}

	public TableBuilder<TColumns> Index(Expression<Func<TColumns, object>> indexExpression, string name = null, bool unique = false, bool clustered = false, object anonymousArguments = null)
	{
		Check.NotNull(indexExpression, "indexExpression");
		CreateIndexOperation createIndexOperation = new CreateIndexOperation(anonymousArguments)
		{
			Name = name,
			Table = _createTableOperation.Name,
			IsUnique = unique,
			IsClustered = clustered
		};
		(from p in indexExpression.GetSimplePropertyAccessList()
			select _createTableOperation.Columns.Single((ColumnModel c) => c.ApiPropertyInfo == p.Single())).Each(delegate(ColumnModel c)
		{
			createIndexOperation.Columns.Add(c.Name);
		});
		_migration.AddOperation(createIndexOperation);
		return this;
	}

	public TableBuilder<TColumns> ForeignKey(string principalTable, Expression<Func<TColumns, object>> dependentKeyExpression, bool cascadeDelete = false, string name = null, object anonymousArguments = null)
	{
		Check.NotEmpty(principalTable, "principalTable");
		Check.NotNull(dependentKeyExpression, "dependentKeyExpression");
		AddForeignKeyOperation addForeignKeyOperation = new AddForeignKeyOperation(anonymousArguments)
		{
			Name = name,
			PrincipalTable = principalTable,
			DependentTable = _createTableOperation.Name,
			CascadeDelete = cascadeDelete
		};
		(from p in dependentKeyExpression.GetSimplePropertyAccessList()
			select _createTableOperation.Columns.Single((ColumnModel c) => c.ApiPropertyInfo == p.Single())).Each(delegate(ColumnModel c)
		{
			addForeignKeyOperation.DependentColumns.Add(c.Name);
		});
		_migration.AddOperation(addForeignKeyOperation);
		return this;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override string ToString()
	{
		return base.ToString();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override bool Equals(object obj)
	{
		return base.Equals(obj);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public new Type GetType()
	{
		return base.GetType();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected new object MemberwiseClone()
	{
		return base.MemberwiseClone();
	}
}
