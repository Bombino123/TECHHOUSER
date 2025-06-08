using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.Migrations.Builders;
using System.Data.Entity.Migrations.Edm;
using System.Data.Entity.Migrations.Infrastructure;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.IO;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.Migrations;

public abstract class DbMigration : IDbMigration
{
	private readonly List<MigrationOperation> _operations = new List<MigrationOperation>();

	internal IEnumerable<MigrationOperation> Operations => _operations;

	public abstract void Up();

	public virtual void Down()
	{
	}

	public void CreateStoredProcedure(string name, string body, object anonymousArguments = null)
	{
		Check.NotEmpty(name, "name");
		CreateStoredProcedure(name, (Func<ParameterBuilder, object>)((ParameterBuilder _) => new { }), body, anonymousArguments);
	}

	public void CreateStoredProcedure<TParameters>(string name, Func<ParameterBuilder, TParameters> parametersAction, string body, object anonymousArguments = null)
	{
		Check.NotEmpty(name, "name");
		Check.NotNull(parametersAction, "parametersAction");
		CreateProcedureOperation createProcedureOperation = new CreateProcedureOperation(name, body, anonymousArguments);
		AddOperation(createProcedureOperation);
		TParameters parameters = parametersAction(new ParameterBuilder());
		parameters.GetType().GetNonIndexerProperties().Each(delegate(PropertyInfo p, int i)
		{
			if (p.GetValue(parameters, null) is ParameterModel parameterModel)
			{
				if (string.IsNullOrWhiteSpace(parameterModel.Name))
				{
					parameterModel.Name = p.Name;
				}
				createProcedureOperation.Parameters.Add(parameterModel);
			}
		});
	}

	public void AlterStoredProcedure(string name, string body, object anonymousArguments = null)
	{
		Check.NotEmpty(name, "name");
		AlterStoredProcedure(name, (Func<ParameterBuilder, object>)((ParameterBuilder _) => new { }), body, anonymousArguments);
	}

	public void AlterStoredProcedure<TParameters>(string name, Func<ParameterBuilder, TParameters> parametersAction, string body, object anonymousArguments = null)
	{
		Check.NotEmpty(name, "name");
		Check.NotNull(parametersAction, "parametersAction");
		AlterProcedureOperation alterProcedureOperation = new AlterProcedureOperation(name, body, anonymousArguments);
		AddOperation(alterProcedureOperation);
		TParameters parameters = parametersAction(new ParameterBuilder());
		parameters.GetType().GetNonIndexerProperties().Each(delegate(PropertyInfo p, int i)
		{
			if (p.GetValue(parameters, null) is ParameterModel parameterModel)
			{
				if (string.IsNullOrWhiteSpace(parameterModel.Name))
				{
					parameterModel.Name = p.Name;
				}
				alterProcedureOperation.Parameters.Add(parameterModel);
			}
		});
	}

	public void DropStoredProcedure(string name, object anonymousArguments = null)
	{
		Check.NotEmpty(name, "name");
		AddOperation(new DropProcedureOperation(name, anonymousArguments));
	}

	protected internal TableBuilder<TColumns> CreateTable<TColumns>(string name, Func<ColumnBuilder, TColumns> columnsAction, object anonymousArguments = null)
	{
		Check.NotEmpty(name, "name");
		Check.NotNull(columnsAction, "columnsAction");
		return CreateTable(name, columnsAction, null, anonymousArguments);
	}

	protected internal TableBuilder<TColumns> CreateTable<TColumns>(string name, Func<ColumnBuilder, TColumns> columnsAction, IDictionary<string, object> annotations, object anonymousArguments = null)
	{
		Check.NotEmpty(name, "name");
		Check.NotNull(columnsAction, "columnsAction");
		CreateTableOperation createTableOperation = new CreateTableOperation(name, annotations, anonymousArguments);
		AddOperation(createTableOperation);
		AddColumns(columnsAction(new ColumnBuilder()), createTableOperation.Columns);
		return new TableBuilder<TColumns>(createTableOperation, this);
	}

	protected internal void AlterTableAnnotations<TColumns>(string name, Func<ColumnBuilder, TColumns> columnsAction, IDictionary<string, AnnotationValues> annotations, object anonymousArguments = null)
	{
		Check.NotEmpty(name, "name");
		Check.NotNull(columnsAction, "columnsAction");
		AlterTableOperation alterTableOperation = new AlterTableOperation(name, annotations, anonymousArguments);
		AddColumns(columnsAction(new ColumnBuilder()), alterTableOperation.Columns);
		AddOperation(alterTableOperation);
	}

	private static void AddColumns<TColumns>(TColumns columns, ICollection<ColumnModel> columnModels)
	{
		columns.GetType().GetNonIndexerProperties().Each(delegate(PropertyInfo p, int i)
		{
			if (p.GetValue(columns, null) is ColumnModel columnModel)
			{
				columnModel.ApiPropertyInfo = p;
				if (string.IsNullOrWhiteSpace(columnModel.Name))
				{
					columnModel.Name = p.Name;
				}
				columnModels.Add(columnModel);
			}
		});
	}

	protected internal void AddForeignKey(string dependentTable, string dependentColumn, string principalTable, string principalColumn = null, bool cascadeDelete = false, string name = null, object anonymousArguments = null)
	{
		Check.NotEmpty(dependentTable, "dependentTable");
		Check.NotEmpty(dependentColumn, "dependentColumn");
		Check.NotEmpty(principalTable, "principalTable");
		AddForeignKey(dependentTable, new string[1] { dependentColumn }, principalTable, (principalColumn == null) ? null : new string[1] { principalColumn }, cascadeDelete, name, anonymousArguments);
	}

	protected internal void AddForeignKey(string dependentTable, string[] dependentColumns, string principalTable, string[] principalColumns = null, bool cascadeDelete = false, string name = null, object anonymousArguments = null)
	{
		Check.NotEmpty(dependentTable, "dependentTable");
		Check.NotNull(dependentColumns, "dependentColumns");
		Check.NotEmpty(principalTable, "principalTable");
		if (!dependentColumns.Any())
		{
			throw new ArgumentException(Strings.CollectionEmpty("dependentColumns", "AddForeignKey"));
		}
		AddForeignKeyOperation addForeignKeyOperation = new AddForeignKeyOperation(anonymousArguments)
		{
			DependentTable = dependentTable,
			PrincipalTable = principalTable,
			CascadeDelete = cascadeDelete,
			Name = name
		};
		dependentColumns.Each(delegate(string c)
		{
			addForeignKeyOperation.DependentColumns.Add(c);
		});
		principalColumns?.Each(delegate(string c)
		{
			addForeignKeyOperation.PrincipalColumns.Add(c);
		});
		AddOperation(addForeignKeyOperation);
	}

	protected internal void DropForeignKey(string dependentTable, string name, object anonymousArguments = null)
	{
		Check.NotEmpty(dependentTable, "dependentTable");
		Check.NotEmpty(name, "name");
		DropForeignKeyOperation migrationOperation = new DropForeignKeyOperation(anonymousArguments)
		{
			DependentTable = dependentTable,
			Name = name
		};
		AddOperation(migrationOperation);
	}

	protected internal void DropForeignKey(string dependentTable, string dependentColumn, string principalTable, object anonymousArguments = null)
	{
		Check.NotEmpty(dependentTable, "dependentTable");
		Check.NotEmpty(dependentColumn, "dependentColumn");
		Check.NotEmpty(principalTable, "principalTable");
		DropForeignKey(dependentTable, new string[1] { dependentColumn }, principalTable, anonymousArguments);
	}

	[Obsolete("The principalColumn parameter is no longer required and can be removed.")]
	protected internal void DropForeignKey(string dependentTable, string dependentColumn, string principalTable, string principalColumn, object anonymousArguments = null)
	{
		Check.NotEmpty(dependentTable, "dependentTable");
		Check.NotEmpty(dependentColumn, "dependentColumn");
		Check.NotEmpty(principalTable, "principalTable");
		DropForeignKey(dependentTable, new string[1] { dependentColumn }, principalTable, anonymousArguments);
	}

	protected internal void DropForeignKey(string dependentTable, string[] dependentColumns, string principalTable, object anonymousArguments = null)
	{
		Check.NotEmpty(dependentTable, "dependentTable");
		Check.NotNull(dependentColumns, "dependentColumns");
		Check.NotEmpty(principalTable, "principalTable");
		if (!dependentColumns.Any())
		{
			throw new ArgumentException(Strings.CollectionEmpty("dependentColumns", "DropForeignKey"));
		}
		DropForeignKeyOperation dropForeignKeyOperation = new DropForeignKeyOperation(anonymousArguments)
		{
			DependentTable = dependentTable,
			PrincipalTable = principalTable
		};
		dependentColumns.Each(delegate(string c)
		{
			dropForeignKeyOperation.DependentColumns.Add(c);
		});
		AddOperation(dropForeignKeyOperation);
	}

	protected internal void DropTable(string name, object anonymousArguments = null)
	{
		Check.NotEmpty(name, "name");
		DropTable(name, null, null, anonymousArguments);
	}

	protected internal void DropTable(string name, IDictionary<string, IDictionary<string, object>> removedColumnAnnotations, object anonymousArguments = null)
	{
		Check.NotEmpty(name, "name");
		DropTable(name, null, removedColumnAnnotations, anonymousArguments);
	}

	protected internal void DropTable(string name, IDictionary<string, object> removedAnnotations, object anonymousArguments = null)
	{
		Check.NotEmpty(name, "name");
		DropTable(name, removedAnnotations, null, anonymousArguments);
	}

	protected internal void DropTable(string name, IDictionary<string, object> removedAnnotations, IDictionary<string, IDictionary<string, object>> removedColumnAnnotations, object anonymousArguments = null)
	{
		Check.NotEmpty(name, "name");
		AddOperation(new DropTableOperation(name, removedAnnotations, removedColumnAnnotations, anonymousArguments));
	}

	protected internal void MoveTable(string name, string newSchema, object anonymousArguments = null)
	{
		Check.NotEmpty(name, "name");
		AddOperation(new MoveTableOperation(name, newSchema, anonymousArguments));
	}

	protected internal void MoveStoredProcedure(string name, string newSchema, object anonymousArguments = null)
	{
		Check.NotEmpty(name, "name");
		AddOperation(new MoveProcedureOperation(name, newSchema, anonymousArguments));
	}

	protected internal void RenameTable(string name, string newName, object anonymousArguments = null)
	{
		Check.NotEmpty(name, "name");
		Check.NotEmpty(newName, "newName");
		AddOperation(new RenameTableOperation(name, newName, anonymousArguments));
	}

	protected internal void RenameStoredProcedure(string name, string newName, object anonymousArguments = null)
	{
		Check.NotEmpty(name, "name");
		Check.NotEmpty(newName, "newName");
		AddOperation(new RenameProcedureOperation(name, newName, anonymousArguments));
	}

	protected internal void RenameColumn(string table, string name, string newName, object anonymousArguments = null)
	{
		Check.NotEmpty(table, "table");
		Check.NotEmpty(name, "name");
		Check.NotEmpty(newName, "newName");
		AddOperation(new RenameColumnOperation(table, name, newName, anonymousArguments));
	}

	protected internal void AddColumn(string table, string name, Func<ColumnBuilder, ColumnModel> columnAction, object anonymousArguments = null)
	{
		Check.NotEmpty(table, "table");
		Check.NotEmpty(name, "name");
		Check.NotNull(columnAction, "columnAction");
		ColumnModel columnModel = columnAction(new ColumnBuilder());
		columnModel.Name = name;
		AddOperation(new AddColumnOperation(table, columnModel, anonymousArguments));
	}

	protected internal void DropColumn(string table, string name, object anonymousArguments = null)
	{
		Check.NotEmpty(table, "table");
		Check.NotEmpty(name, "name");
		DropColumn(table, name, null, anonymousArguments);
	}

	protected internal void DropColumn(string table, string name, IDictionary<string, object> removedAnnotations, object anonymousArguments = null)
	{
		Check.NotEmpty(table, "table");
		Check.NotEmpty(name, "name");
		AddOperation(new DropColumnOperation(table, name, removedAnnotations, anonymousArguments));
	}

	protected internal void AlterColumn(string table, string name, Func<ColumnBuilder, ColumnModel> columnAction, object anonymousArguments = null)
	{
		Check.NotEmpty(table, "table");
		Check.NotEmpty(name, "name");
		Check.NotNull(columnAction, "columnAction");
		ColumnModel columnModel = columnAction(new ColumnBuilder());
		columnModel.Name = name;
		AddOperation(new AlterColumnOperation(table, columnModel, isDestructiveChange: false, anonymousArguments));
	}

	protected internal void AddPrimaryKey(string table, string column, string name = null, bool clustered = true, object anonymousArguments = null)
	{
		Check.NotEmpty(table, "table");
		Check.NotEmpty(column, "column");
		AddPrimaryKey(table, new string[1] { column }, name, clustered, anonymousArguments);
	}

	protected internal void AddPrimaryKey(string table, string[] columns, string name = null, bool clustered = true, object anonymousArguments = null)
	{
		Check.NotEmpty(table, "table");
		Check.NotNull(columns, "columns");
		if (!columns.Any())
		{
			throw new ArgumentException(Strings.CollectionEmpty("columns", "AddPrimaryKey"));
		}
		AddPrimaryKeyOperation addPrimaryKeyOperation = new AddPrimaryKeyOperation(anonymousArguments)
		{
			Table = table,
			Name = name,
			IsClustered = clustered
		};
		columns.Each(delegate(string c)
		{
			addPrimaryKeyOperation.Columns.Add(c);
		});
		AddOperation(addPrimaryKeyOperation);
	}

	protected internal void DropPrimaryKey(string table, string name, object anonymousArguments = null)
	{
		Check.NotEmpty(table, "table");
		Check.NotEmpty(name, "name");
		DropPrimaryKeyOperation migrationOperation = new DropPrimaryKeyOperation(anonymousArguments)
		{
			Table = table,
			Name = name
		};
		AddOperation(migrationOperation);
	}

	protected internal void DropPrimaryKey(string table, object anonymousArguments = null)
	{
		Check.NotEmpty(table, "table");
		DropPrimaryKeyOperation migrationOperation = new DropPrimaryKeyOperation(anonymousArguments)
		{
			Table = table
		};
		AddOperation(migrationOperation);
	}

	protected internal void CreateIndex(string table, string column, bool unique = false, string name = null, bool clustered = false, object anonymousArguments = null)
	{
		Check.NotEmpty(table, "table");
		Check.NotEmpty(column, "column");
		CreateIndex(table, new string[1] { column }, unique, name, clustered, anonymousArguments);
	}

	protected internal void CreateIndex(string table, string[] columns, bool unique = false, string name = null, bool clustered = false, object anonymousArguments = null)
	{
		Check.NotEmpty(table, "table");
		Check.NotNull(columns, "columns");
		if (!columns.Any())
		{
			throw new ArgumentException(Strings.CollectionEmpty("columns", "CreateIndex"));
		}
		CreateIndexOperation createIndexOperation = new CreateIndexOperation(anonymousArguments)
		{
			Table = table,
			IsUnique = unique,
			Name = name,
			IsClustered = clustered
		};
		columns.Each(delegate(string c)
		{
			createIndexOperation.Columns.Add(c);
		});
		AddOperation(createIndexOperation);
	}

	protected internal void DropIndex(string table, string name, object anonymousArguments = null)
	{
		Check.NotEmpty(table, "table");
		Check.NotEmpty(name, "name");
		DropIndexOperation migrationOperation = new DropIndexOperation(anonymousArguments)
		{
			Table = table,
			Name = name
		};
		AddOperation(migrationOperation);
	}

	protected internal void DropIndex(string table, string[] columns, object anonymousArguments = null)
	{
		Check.NotEmpty(table, "table");
		Check.NotNull(columns, "columns");
		if (!columns.Any())
		{
			throw new ArgumentException(Strings.CollectionEmpty("columns", "DropIndex"));
		}
		DropIndexOperation dropIndexOperation = new DropIndexOperation(anonymousArguments)
		{
			Table = table
		};
		columns.Each(delegate(string c)
		{
			dropIndexOperation.Columns.Add(c);
		});
		AddOperation(dropIndexOperation);
	}

	protected internal void RenameIndex(string table, string name, string newName, object anonymousArguments = null)
	{
		Check.NotEmpty(table, "table");
		Check.NotEmpty(name, "name");
		Check.NotEmpty(newName, "newName");
		AddOperation(new RenameIndexOperation(table, name, newName, anonymousArguments));
	}

	protected internal void Sql(string sql, bool suppressTransaction = false, object anonymousArguments = null)
	{
		Check.NotEmpty(sql, "sql");
		AddOperation(new SqlOperation(sql, anonymousArguments)
		{
			SuppressTransaction = suppressTransaction
		});
	}

	protected internal void SqlFile(string sqlFile, bool suppressTransaction = false, object anonymousArguments = null)
	{
		Check.NotEmpty(sqlFile, "sqlFile");
		if (!Path.IsPathRooted(sqlFile))
		{
			sqlFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, sqlFile);
		}
		AddOperation(new SqlOperation(File.ReadAllText(sqlFile), anonymousArguments)
		{
			SuppressTransaction = suppressTransaction
		});
	}

	protected internal void SqlResource(string sqlResource, Assembly resourceAssembly = null, bool suppressTransaction = false, object anonymousArguments = null)
	{
		Check.NotEmpty(sqlResource, "sqlResource");
		resourceAssembly = resourceAssembly ?? Assembly.GetCallingAssembly();
		if (!resourceAssembly.GetManifestResourceNames().Contains(sqlResource))
		{
			throw new ArgumentException(Strings.UnableToLoadEmbeddedResource(resourceAssembly.FullName, sqlResource));
		}
		using StreamReader streamReader = new StreamReader(resourceAssembly.GetManifestResourceStream(sqlResource));
		AddOperation(new SqlOperation(streamReader.ReadToEnd(), anonymousArguments)
		{
			SuppressTransaction = suppressTransaction
		});
	}

	void IDbMigration.AddOperation(MigrationOperation migrationOperation)
	{
		AddOperation(migrationOperation);
	}

	internal void AddOperation(MigrationOperation migrationOperation)
	{
		Check.NotNull(migrationOperation, "migrationOperation");
		_operations.Add(migrationOperation);
	}

	internal void Reset()
	{
		_operations.Clear();
	}

	internal VersionedModel GetSourceModel()
	{
		return GetModel((IMigrationMetadata mm) => mm.Source);
	}

	internal VersionedModel GetTargetModel()
	{
		return GetModel((IMigrationMetadata mm) => mm.Target);
	}

	private VersionedModel GetModel(Func<IMigrationMetadata, string> modelAccessor)
	{
		IMigrationMetadata arg = (IMigrationMetadata)this;
		string text = modelAccessor(arg);
		if (string.IsNullOrWhiteSpace(text))
		{
			return null;
		}
		GeneratedCodeAttribute generatedCodeAttribute = GetType().GetCustomAttributes<GeneratedCodeAttribute>(inherit: false).SingleOrDefault();
		string version = ((generatedCodeAttribute != null && !string.IsNullOrWhiteSpace(generatedCodeAttribute.Version)) ? generatedCodeAttribute.Version : typeof(DbMigration).Assembly().GetInformationalVersion());
		return new VersionedModel(new ModelCompressor().Decompress(Convert.FromBase64String(text)), version);
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
