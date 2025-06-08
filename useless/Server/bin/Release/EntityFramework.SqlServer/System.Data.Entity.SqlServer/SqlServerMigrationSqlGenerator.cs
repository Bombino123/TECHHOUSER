using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Hierarchy;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.Migrations.Sql;
using System.Data.Entity.Migrations.Utilities;
using System.Data.Entity.Spatial;
using System.Data.Entity.SqlServer.Resources;
using System.Data.Entity.SqlServer.SqlGen;
using System.Data.Entity.SqlServer.Utilities;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace System.Data.Entity.SqlServer;

public class SqlServerMigrationSqlGenerator : MigrationSqlGenerator
{
	private class HistoryRebuildOperationSequence : MigrationOperation
	{
		public readonly AddColumnOperation AddColumnOperation;

		public readonly DropPrimaryKeyOperation DropPrimaryKeyOperation;

		public override bool IsDestructiveChange => false;

		private HistoryRebuildOperationSequence(AddColumnOperation addColumnOperation, DropPrimaryKeyOperation dropPrimaryKeyOperation)
			: base((object)null)
		{
			AddColumnOperation = addColumnOperation;
			DropPrimaryKeyOperation = dropPrimaryKeyOperation;
		}

		public static HistoryRebuildOperationSequence Detect(IEnumerator<MigrationOperation> enumerator)
		{
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Expected O, but got Unknown
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			MigrationOperation current = enumerator.Current;
			AddColumnOperation val = (AddColumnOperation)(object)((current is AddColumnOperation) ? current : null);
			if (val == null || val.Table != "dbo.__MigrationHistory" || ((PropertyModel)val.Column).Name != "ContextKey")
			{
				return null;
			}
			enumerator.MoveNext();
			DropPrimaryKeyOperation dropPrimaryKeyOperation = (DropPrimaryKeyOperation)enumerator.Current;
			enumerator.MoveNext();
			_ = (AlterColumnOperation)enumerator.Current;
			enumerator.MoveNext();
			_ = (AddPrimaryKeyOperation)enumerator.Current;
			return new HistoryRebuildOperationSequence(val, dropPrimaryKeyOperation);
		}
	}

	private const string BatchTerminator = "GO";

	internal const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffK";

	internal const string DateTimeOffsetFormat = "yyyy-MM-ddTHH:mm:ss.fffzzz";

	private SqlGenerator _sqlGenerator;

	private List<MigrationStatement> _statements;

	private HashSet<string> _generatedSchemas;

	private string _providerManifestToken;

	private int _variableCounter;

	protected virtual string GuidColumnDefault
	{
		get
		{
			if (!(_providerManifestToken != "2012.Azure") || !(_providerManifestToken != "2000"))
			{
				return "newid()";
			}
			return "newsequentialid()";
		}
	}

	public override bool IsPermissionDeniedError(Exception exception)
	{
		SqlException val = (SqlException)(object)((exception is SqlException) ? exception : null);
		if (val != null)
		{
			return val.Number == 229;
		}
		return false;
	}

	public override IEnumerable<MigrationStatement> Generate(IEnumerable<MigrationOperation> migrationOperations, string providerManifestToken)
	{
		Check.NotNull(migrationOperations, "migrationOperations");
		Check.NotNull(providerManifestToken, "providerManifestToken");
		_statements = new List<MigrationStatement>();
		_generatedSchemas = new HashSet<string>();
		InitializeProviderServices(providerManifestToken);
		GenerateStatements(migrationOperations);
		return _statements;
	}

	private void GenerateStatements(IEnumerable<MigrationOperation> migrationOperations)
	{
		Check.NotNull(migrationOperations, "migrationOperations");
		DetectHistoryRebuild(migrationOperations).Each(delegate(dynamic o)
		{
			Generate(o);
		});
	}

	public override string GenerateProcedureBody(ICollection<DbModificationCommandTree> commandTrees, string rowsAffectedParameter, string providerManifestToken)
	{
		Check.NotNull(commandTrees, "commandTrees");
		Check.NotEmpty(providerManifestToken, "providerManifestToken");
		if (!commandTrees.Any())
		{
			return "RETURN";
		}
		InitializeProviderServices(providerManifestToken);
		return GenerateFunctionSql(commandTrees, rowsAffectedParameter);
	}

	private void InitializeProviderServices(string providerManifestToken)
	{
		Check.NotEmpty(providerManifestToken, "providerManifestToken");
		_providerManifestToken = providerManifestToken;
		using DbConnection dbConnection = CreateConnection();
		((MigrationSqlGenerator)this).ProviderManifest = DbProviderServices.GetProviderServices(dbConnection).GetProviderManifest(providerManifestToken);
		_sqlGenerator = new SqlGenerator(SqlVersionUtils.GetSqlVersion(providerManifestToken));
	}

	private string GenerateFunctionSql(ICollection<DbModificationCommandTree> commandTrees, string rowsAffectedParameter)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected I4, but got Unknown
		DmlFunctionSqlGenerator dmlFunctionSqlGenerator = new DmlFunctionSqlGenerator(_sqlGenerator);
		DbCommandTreeKind commandTreeKind = ((DbCommandTree)commandTrees.First()).CommandTreeKind;
		return (commandTreeKind - 1) switch
		{
			1 => dmlFunctionSqlGenerator.GenerateInsert(commandTrees.Cast<DbInsertCommandTree>().ToList()), 
			0 => dmlFunctionSqlGenerator.GenerateUpdate(commandTrees.Cast<DbUpdateCommandTree>().ToList(), rowsAffectedParameter), 
			2 => dmlFunctionSqlGenerator.GenerateDelete(commandTrees.Cast<DbDeleteCommandTree>().ToList(), rowsAffectedParameter), 
			_ => null, 
		};
	}

	protected virtual void Generate(UpdateDatabaseOperation updateDatabaseOperation)
	{
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0341: Unknown result type (might be due to invalid IL or missing references)
		//IL_0358: Expected O, but got Unknown
		Check.NotNull<UpdateDatabaseOperation>(updateDatabaseOperation, "updateDatabaseOperation");
		if (!updateDatabaseOperation.Migrations.Any())
		{
			return;
		}
		IndentedTextWriter val = Writer();
		try
		{
			((TextWriter)(object)val).WriteLine("DECLARE @CurrentMigration [nvarchar](max)");
			((TextWriter)(object)val).WriteLine();
			int indent;
			foreach (DbQueryCommandTree historyQueryTree in updateDatabaseOperation.HistoryQueryTrees)
			{
				HashSet<string> paramsToForceNonUnicode;
				string s = _sqlGenerator.GenerateSql(historyQueryTree, out paramsToForceNonUnicode);
				((TextWriter)(object)val).Write("IF object_id('");
				((TextWriter)(object)val).Write(Escape(_sqlGenerator.Targets.Single()));
				((TextWriter)(object)val).WriteLine("') IS NOT NULL");
				indent = val.Indent;
				val.Indent = indent + 1;
				((TextWriter)(object)val).WriteLine("SELECT @CurrentMigration =");
				indent = val.Indent;
				val.Indent = indent + 1;
				((TextWriter)(object)val).Write("(");
				((TextWriter)(object)val).Write(Indent(s, val.CurrentIndentation()));
				((TextWriter)(object)val).WriteLine(")");
				val.Indent -= 2;
				((TextWriter)(object)val).WriteLine();
			}
			((TextWriter)(object)val).WriteLine("IF @CurrentMigration IS NULL");
			indent = val.Indent;
			val.Indent = indent + 1;
			((TextWriter)(object)val).WriteLine("SET @CurrentMigration = '0'");
			Statement(val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		List<MigrationStatement> statements = _statements;
		foreach (Migration migration in updateDatabaseOperation.Migrations)
		{
			IndentedTextWriter val2 = Writer();
			try
			{
				_statements = new List<MigrationStatement>();
				GenerateStatements(migration.Operations);
				if (_statements.Count <= 0)
				{
					continue;
				}
				((TextWriter)(object)val2).Write("IF @CurrentMigration < '");
				((TextWriter)(object)val2).Write(Escape(migration.MigrationId));
				((TextWriter)(object)val2).WriteLine("'");
				((TextWriter)(object)val2).Write("BEGIN");
				IndentedTextWriter blockWriter = Writer();
				try
				{
					((TextWriter)(object)blockWriter).WriteLine();
					IndentedTextWriter obj = blockWriter;
					int indent = obj.Indent;
					obj.Indent = indent + 1;
					foreach (MigrationStatement statement in _statements)
					{
						if (string.IsNullOrWhiteSpace(statement.BatchTerminator))
						{
							statement.Sql.EachLine(((TextWriter)(object)blockWriter).WriteLine);
							continue;
						}
						((TextWriter)(object)blockWriter).WriteLine("EXECUTE('");
						IndentedTextWriter obj2 = blockWriter;
						indent = obj2.Indent;
						obj2.Indent = indent + 1;
						statement.Sql.EachLine(delegate(string l)
						{
							((TextWriter)(object)blockWriter).WriteLine(Escape(l));
						});
						IndentedTextWriter obj3 = blockWriter;
						indent = obj3.Indent;
						obj3.Indent = indent - 1;
						((TextWriter)(object)blockWriter).WriteLine("')");
					}
					((TextWriter)(object)val2).WriteLine(blockWriter.InnerWriter.ToString().TrimEnd(new char[0]));
				}
				finally
				{
					if (blockWriter != null)
					{
						((IDisposable)blockWriter).Dispose();
					}
				}
				((TextWriter)(object)val2).WriteLine("END");
				statements.Add(new MigrationStatement
				{
					Sql = val2.InnerWriter.ToString()
				});
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		_statements = statements;
	}

	protected virtual void Generate(MigrationOperation migrationOperation)
	{
		Check.NotNull<MigrationOperation>(migrationOperation, "migrationOperation");
		throw Error.SqlServerMigrationSqlGenerator_UnknownOperation(((object)this).GetType().Name, ((object)migrationOperation).GetType().FullName);
	}

	protected virtual DbConnection CreateConnection()
	{
		return DbDependencyResolverExtensions.GetService<DbProviderFactory>(DbConfiguration.DependencyResolver, (object)"System.Data.SqlClient").CreateConnection();
	}

	protected virtual void Generate(CreateProcedureOperation createProcedureOperation)
	{
		Check.NotNull<CreateProcedureOperation>(createProcedureOperation, "createProcedureOperation");
		Generate((ProcedureOperation)(object)createProcedureOperation, "CREATE");
	}

	protected virtual void Generate(AlterProcedureOperation alterProcedureOperation)
	{
		Check.NotNull<AlterProcedureOperation>(alterProcedureOperation, "alterProcedureOperation");
		Generate((ProcedureOperation)(object)alterProcedureOperation, "ALTER");
	}

	private void Generate(ProcedureOperation procedureOperation, string modifier)
	{
		IndentedTextWriter writer = Writer();
		try
		{
			((TextWriter)(object)writer).Write(modifier);
			((TextWriter)(object)writer).WriteLine(" PROCEDURE " + Name(procedureOperation.Name));
			IndentedTextWriter obj = writer;
			int indent = obj.Indent;
			obj.Indent = indent + 1;
			procedureOperation.Parameters.Each(delegate(ParameterModel p, int i)
			{
				Generate(p, writer);
				((TextWriter)(object)writer).WriteLine((i < procedureOperation.Parameters.Count - 1) ? "," : string.Empty);
			});
			IndentedTextWriter obj2 = writer;
			indent = obj2.Indent;
			obj2.Indent = indent - 1;
			((TextWriter)(object)writer).WriteLine("AS");
			((TextWriter)(object)writer).WriteLine("BEGIN");
			IndentedTextWriter obj3 = writer;
			indent = obj3.Indent;
			obj3.Indent = indent + 1;
			((TextWriter)(object)writer).WriteLine((!string.IsNullOrWhiteSpace(procedureOperation.BodySql)) ? Indent(procedureOperation.BodySql, writer.CurrentIndentation()) : "RETURN");
			IndentedTextWriter obj4 = writer;
			indent = obj4.Indent;
			obj4.Indent = indent - 1;
			((TextWriter)(object)writer).Write("END");
			Statement(writer, "GO");
		}
		finally
		{
			if (writer != null)
			{
				((IDisposable)writer).Dispose();
			}
		}
	}

	private void Generate(ParameterModel parameterModel, IndentedTextWriter writer)
	{
		((TextWriter)(object)writer).Write("@");
		((TextWriter)(object)writer).Write(((PropertyModel)parameterModel).Name);
		((TextWriter)(object)writer).Write(" ");
		((TextWriter)(object)writer).Write(BuildPropertyType((PropertyModel)(object)parameterModel));
		if (parameterModel.IsOutParameter)
		{
			((TextWriter)(object)writer).Write(" OUT");
		}
		if (((PropertyModel)parameterModel).DefaultValue != null)
		{
			((TextWriter)(object)writer).Write(" = ");
			writer.Write(Generate((dynamic)((PropertyModel)parameterModel).DefaultValue));
		}
		else if (!string.IsNullOrWhiteSpace(((PropertyModel)parameterModel).DefaultValueSql))
		{
			((TextWriter)(object)writer).Write(" = ");
			((TextWriter)(object)writer).Write(((PropertyModel)parameterModel).DefaultValueSql);
		}
	}

	protected virtual void Generate(DropProcedureOperation dropProcedureOperation)
	{
		Check.NotNull<DropProcedureOperation>(dropProcedureOperation, "dropProcedureOperation");
		IndentedTextWriter val = Writer();
		try
		{
			((TextWriter)(object)val).Write("DROP PROCEDURE ");
			((TextWriter)(object)val).Write(Name(dropProcedureOperation.Name));
			Statement(val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	protected virtual void Generate(CreateTableOperation createTableOperation)
	{
		Check.NotNull<CreateTableOperation>(createTableOperation, "createTableOperation");
		DatabaseName databaseName = DatabaseName.Parse(createTableOperation.Name);
		if (!string.IsNullOrWhiteSpace(databaseName.Schema) && !databaseName.Schema.EqualsIgnoreCase("dbo") && !_generatedSchemas.Contains(databaseName.Schema))
		{
			GenerateCreateSchema(databaseName.Schema);
			_generatedSchemas.Add(databaseName.Schema);
		}
		WriteCreateTable(createTableOperation);
	}

	protected virtual void WriteCreateTable(CreateTableOperation createTableOperation)
	{
		Check.NotNull<CreateTableOperation>(createTableOperation, "createTableOperation");
		IndentedTextWriter val = Writer();
		try
		{
			WriteCreateTable(createTableOperation, val);
			Statement(val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	protected virtual void WriteCreateTable(CreateTableOperation createTableOperation, IndentedTextWriter writer)
	{
		Check.NotNull<CreateTableOperation>(createTableOperation, "createTableOperation");
		Check.NotNull<IndentedTextWriter>(writer, "writer");
		((TextWriter)(object)writer).WriteLine("CREATE TABLE " + Name(createTableOperation.Name) + " (");
		IndentedTextWriter obj = writer;
		int indent = obj.Indent;
		obj.Indent = indent + 1;
		createTableOperation.Columns.Each(delegate(ColumnModel c, int i)
		{
			Generate(c, writer);
			if (i < createTableOperation.Columns.Count - 1)
			{
				((TextWriter)(object)writer).WriteLine(",");
			}
		});
		if (createTableOperation.PrimaryKey != null)
		{
			((TextWriter)(object)writer).WriteLine(",");
			((TextWriter)(object)writer).Write("CONSTRAINT ");
			((TextWriter)(object)writer).Write(Quote(((PrimaryKeyOperation)createTableOperation.PrimaryKey).Name));
			((TextWriter)(object)writer).Write(" PRIMARY KEY ");
			if (!((PrimaryKeyOperation)createTableOperation.PrimaryKey).IsClustered)
			{
				((TextWriter)(object)writer).Write("NONCLUSTERED ");
			}
			((TextWriter)(object)writer).Write("(");
			((TextWriter)(object)writer).Write(((PrimaryKeyOperation)createTableOperation.PrimaryKey).Columns.Join(Quote));
			((TextWriter)(object)writer).WriteLine(")");
		}
		else
		{
			((TextWriter)(object)writer).WriteLine();
		}
		IndentedTextWriter obj2 = writer;
		indent = obj2.Indent;
		obj2.Indent = indent - 1;
		((TextWriter)(object)writer).Write(")");
	}

	protected internal virtual void Generate(AlterTableOperation alterTableOperation)
	{
		Check.NotNull<AlterTableOperation>(alterTableOperation, "alterTableOperation");
	}

	protected virtual void GenerateMakeSystemTable(CreateTableOperation createTableOperation, IndentedTextWriter writer)
	{
		Check.NotNull<CreateTableOperation>(createTableOperation, "createTableOperation");
		Check.NotNull<IndentedTextWriter>(writer, "writer");
		((TextWriter)(object)writer).WriteLine("BEGIN TRY");
		int indent = writer.Indent;
		writer.Indent = indent + 1;
		((TextWriter)(object)writer).WriteLine("EXECUTE sp_MS_marksystemobject '" + Escape(createTableOperation.Name) + "'");
		indent = writer.Indent;
		writer.Indent = indent - 1;
		((TextWriter)(object)writer).WriteLine("END TRY");
		((TextWriter)(object)writer).WriteLine("BEGIN CATCH");
		((TextWriter)(object)writer).Write("END CATCH");
	}

	protected virtual void GenerateCreateSchema(string schema)
	{
		Check.NotEmpty(schema, "schema");
		IndentedTextWriter val = Writer();
		try
		{
			((TextWriter)(object)val).Write("IF schema_id('");
			((TextWriter)(object)val).Write(Escape(schema));
			((TextWriter)(object)val).WriteLine("') IS NULL");
			int indent = val.Indent;
			val.Indent = indent + 1;
			((TextWriter)(object)val).Write("EXECUTE('CREATE SCHEMA ");
			((TextWriter)(object)val).Write(Escape(Quote(schema)));
			((TextWriter)(object)val).Write("')");
			Statement(val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	protected virtual void Generate(AddForeignKeyOperation addForeignKeyOperation)
	{
		Check.NotNull<AddForeignKeyOperation>(addForeignKeyOperation, "addForeignKeyOperation");
		IndentedTextWriter val = Writer();
		try
		{
			((TextWriter)(object)val).Write("ALTER TABLE ");
			((TextWriter)(object)val).Write(Name(((ForeignKeyOperation)addForeignKeyOperation).DependentTable));
			((TextWriter)(object)val).Write(" ADD CONSTRAINT ");
			((TextWriter)(object)val).Write(Quote(((ForeignKeyOperation)addForeignKeyOperation).Name));
			((TextWriter)(object)val).Write(" FOREIGN KEY (");
			((TextWriter)(object)val).Write(((ForeignKeyOperation)addForeignKeyOperation).DependentColumns.Select(Quote).Join());
			((TextWriter)(object)val).Write(") REFERENCES ");
			((TextWriter)(object)val).Write(Name(((ForeignKeyOperation)addForeignKeyOperation).PrincipalTable));
			((TextWriter)(object)val).Write(" (");
			((TextWriter)(object)val).Write(addForeignKeyOperation.PrincipalColumns.Select(Quote).Join());
			((TextWriter)(object)val).Write(")");
			if (addForeignKeyOperation.CascadeDelete)
			{
				((TextWriter)(object)val).Write(" ON DELETE CASCADE");
			}
			Statement(val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	protected virtual void Generate(DropForeignKeyOperation dropForeignKeyOperation)
	{
		Check.NotNull<DropForeignKeyOperation>(dropForeignKeyOperation, "dropForeignKeyOperation");
		IndentedTextWriter val = Writer();
		try
		{
			((TextWriter)(object)val).Write("IF object_id(N'");
			string schema = DatabaseName.Parse(((ForeignKeyOperation)dropForeignKeyOperation).DependentTable).Schema;
			if (schema != null)
			{
				((TextWriter)(object)val).Write(Escape(Quote(schema)));
				((TextWriter)(object)val).Write(".");
			}
			((TextWriter)(object)val).Write(Escape(Quote(((ForeignKeyOperation)dropForeignKeyOperation).Name)));
			((TextWriter)(object)val).WriteLine("', N'F') IS NOT NULL");
			int indent = val.Indent;
			val.Indent = indent + 1;
			((TextWriter)(object)val).Write("ALTER TABLE ");
			((TextWriter)(object)val).Write(Name(((ForeignKeyOperation)dropForeignKeyOperation).DependentTable));
			((TextWriter)(object)val).Write(" DROP CONSTRAINT ");
			((TextWriter)(object)val).Write(Quote(((ForeignKeyOperation)dropForeignKeyOperation).Name));
			indent = val.Indent;
			val.Indent = indent - 1;
			Statement(val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	protected virtual void Generate(CreateIndexOperation createIndexOperation)
	{
		Check.NotNull<CreateIndexOperation>(createIndexOperation, "createIndexOperation");
		IndentedTextWriter val = Writer();
		try
		{
			((TextWriter)(object)val).Write("CREATE ");
			if (createIndexOperation.IsUnique)
			{
				((TextWriter)(object)val).Write("UNIQUE ");
			}
			if (createIndexOperation.IsClustered)
			{
				((TextWriter)(object)val).Write("CLUSTERED ");
			}
			((TextWriter)(object)val).Write("INDEX ");
			((TextWriter)(object)val).Write(Quote(((IndexOperation)createIndexOperation).Name));
			((TextWriter)(object)val).Write(" ON ");
			((TextWriter)(object)val).Write(Name(((IndexOperation)createIndexOperation).Table));
			((TextWriter)(object)val).Write("(");
			((TextWriter)(object)val).Write(((IndexOperation)createIndexOperation).Columns.Join(Quote));
			((TextWriter)(object)val).Write(")");
			Statement(val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	protected virtual void Generate(DropIndexOperation dropIndexOperation)
	{
		Check.NotNull<DropIndexOperation>(dropIndexOperation, "dropIndexOperation");
		IndentedTextWriter val = Writer();
		try
		{
			((TextWriter)(object)val).Write("IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'");
			((TextWriter)(object)val).Write(Escape(((IndexOperation)dropIndexOperation).Name));
			((TextWriter)(object)val).Write("' AND object_id = object_id(N'");
			((TextWriter)(object)val).Write(Escape(Name(((IndexOperation)dropIndexOperation).Table)));
			((TextWriter)(object)val).WriteLine("', N'U'))");
			int indent = val.Indent;
			val.Indent = indent + 1;
			((TextWriter)(object)val).Write("DROP INDEX ");
			((TextWriter)(object)val).Write(Quote(((IndexOperation)dropIndexOperation).Name));
			((TextWriter)(object)val).Write(" ON ");
			((TextWriter)(object)val).Write(Name(((IndexOperation)dropIndexOperation).Table));
			indent = val.Indent;
			val.Indent = indent - 1;
			Statement(val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	protected virtual void Generate(AddPrimaryKeyOperation addPrimaryKeyOperation)
	{
		Check.NotNull<AddPrimaryKeyOperation>(addPrimaryKeyOperation, "addPrimaryKeyOperation");
		IndentedTextWriter val = Writer();
		try
		{
			((TextWriter)(object)val).Write("ALTER TABLE ");
			((TextWriter)(object)val).Write(Name(((PrimaryKeyOperation)addPrimaryKeyOperation).Table));
			((TextWriter)(object)val).Write(" ADD CONSTRAINT ");
			((TextWriter)(object)val).Write(Quote(((PrimaryKeyOperation)addPrimaryKeyOperation).Name));
			((TextWriter)(object)val).Write(" PRIMARY KEY ");
			if (!((PrimaryKeyOperation)addPrimaryKeyOperation).IsClustered)
			{
				((TextWriter)(object)val).Write("NONCLUSTERED ");
			}
			((TextWriter)(object)val).Write("(");
			((TextWriter)(object)val).Write(((PrimaryKeyOperation)addPrimaryKeyOperation).Columns.Select(Quote).Join());
			((TextWriter)(object)val).Write(")");
			Statement(val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	protected virtual void Generate(DropPrimaryKeyOperation dropPrimaryKeyOperation)
	{
		Check.NotNull<DropPrimaryKeyOperation>(dropPrimaryKeyOperation, "dropPrimaryKeyOperation");
		IndentedTextWriter val = Writer();
		try
		{
			((TextWriter)(object)val).Write("ALTER TABLE ");
			((TextWriter)(object)val).Write(Name(((PrimaryKeyOperation)dropPrimaryKeyOperation).Table));
			((TextWriter)(object)val).Write(" DROP CONSTRAINT ");
			((TextWriter)(object)val).Write(Quote(((PrimaryKeyOperation)dropPrimaryKeyOperation).Name));
			Statement(val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	protected virtual void Generate(AddColumnOperation addColumnOperation)
	{
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Invalid comparison between Unknown and I4
		Check.NotNull<AddColumnOperation>(addColumnOperation, "addColumnOperation");
		IndentedTextWriter val = Writer();
		try
		{
			((TextWriter)(object)val).Write("ALTER TABLE ");
			((TextWriter)(object)val).Write(Name(addColumnOperation.Table));
			((TextWriter)(object)val).Write(" ADD ");
			ColumnModel column = addColumnOperation.Column;
			Generate(column, val);
			if (column.IsNullable.HasValue && !column.IsNullable.Value && ((PropertyModel)column).DefaultValue == null && string.IsNullOrWhiteSpace(((PropertyModel)column).DefaultValueSql) && !column.IsIdentity && !column.IsTimestamp && !((PropertyModel)column).StoreType.EqualsIgnoreCase("rowversion") && !((PropertyModel)column).StoreType.EqualsIgnoreCase("timestamp"))
			{
				((TextWriter)(object)val).Write(" DEFAULT ");
				if ((int)((PropertyModel)column).Type != 3)
				{
					val.Write(Generate((dynamic)column.ClrDefaultValue));
				}
				else
				{
					((TextWriter)(object)val).Write(Generate(DateTime.Parse("1900-01-01 00:00:00", CultureInfo.InvariantCulture)));
				}
			}
			Statement(val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	protected virtual void Generate(DropColumnOperation dropColumnOperation)
	{
		Check.NotNull<DropColumnOperation>(dropColumnOperation, "dropColumnOperation");
		IndentedTextWriter val = Writer();
		try
		{
			DropDefaultConstraint(dropColumnOperation.Table, dropColumnOperation.Name, val);
			((TextWriter)(object)val).Write("ALTER TABLE ");
			((TextWriter)(object)val).Write(Name(dropColumnOperation.Table));
			((TextWriter)(object)val).Write(" DROP COLUMN ");
			((TextWriter)(object)val).Write(Quote(dropColumnOperation.Name));
			Statement(val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	protected virtual void Generate(AlterColumnOperation alterColumnOperation)
	{
		Check.NotNull<AlterColumnOperation>(alterColumnOperation, "alterColumnOperation");
		ColumnModel column = alterColumnOperation.Column;
		IndentedTextWriter val = Writer();
		try
		{
			DropDefaultConstraint(alterColumnOperation.Table, ((PropertyModel)column).Name, val);
			((TextWriter)(object)val).Write("ALTER TABLE ");
			((TextWriter)(object)val).Write(Name(alterColumnOperation.Table));
			((TextWriter)(object)val).Write(" ALTER COLUMN ");
			((TextWriter)(object)val).Write(Quote(((PropertyModel)column).Name));
			((TextWriter)(object)val).Write(" ");
			((TextWriter)(object)val).Write(BuildColumnType(column));
			if (column.IsNullable.HasValue && !column.IsNullable.Value)
			{
				((TextWriter)(object)val).Write(" NOT");
			}
			((TextWriter)(object)val).Write(" NULL");
			if (((PropertyModel)column).DefaultValue != null || !string.IsNullOrWhiteSpace(((PropertyModel)column).DefaultValueSql))
			{
				((TextWriter)(object)val).WriteLine();
				((TextWriter)(object)val).Write("ALTER TABLE ");
				((TextWriter)(object)val).Write(Name(alterColumnOperation.Table));
				((TextWriter)(object)val).Write(" ADD CONSTRAINT ");
				((TextWriter)(object)val).Write(Quote("DF_" + alterColumnOperation.Table + "_" + ((PropertyModel)column).Name));
				((TextWriter)(object)val).Write(" DEFAULT ");
				val.Write((((PropertyModel)column).DefaultValue != null) ? Generate((dynamic)((PropertyModel)column).DefaultValue) : ((PropertyModel)column).DefaultValueSql);
				((TextWriter)(object)val).Write(" FOR ");
				((TextWriter)(object)val).Write(Quote(((PropertyModel)column).Name));
			}
			Statement(val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	protected internal virtual void DropDefaultConstraint(string table, string column, IndentedTextWriter writer)
	{
		Check.NotEmpty(table, "table");
		Check.NotEmpty(column, "column");
		Check.NotNull<IndentedTextWriter>(writer, "writer");
		string value = "@var" + _variableCounter++;
		((TextWriter)(object)writer).Write("DECLARE ");
		((TextWriter)(object)writer).Write(value);
		((TextWriter)(object)writer).WriteLine(" nvarchar(128)");
		((TextWriter)(object)writer).Write("SELECT ");
		((TextWriter)(object)writer).Write(value);
		((TextWriter)(object)writer).WriteLine(" = name");
		((TextWriter)(object)writer).WriteLine("FROM sys.default_constraints");
		((TextWriter)(object)writer).Write("WHERE parent_object_id = object_id(N'");
		((TextWriter)(object)writer).Write(table);
		((TextWriter)(object)writer).WriteLine("')");
		((TextWriter)(object)writer).Write("AND col_name(parent_object_id, parent_column_id) = '");
		((TextWriter)(object)writer).Write(column);
		((TextWriter)(object)writer).WriteLine("';");
		((TextWriter)(object)writer).Write("IF ");
		((TextWriter)(object)writer).Write(value);
		((TextWriter)(object)writer).WriteLine(" IS NOT NULL");
		int indent = writer.Indent;
		writer.Indent = indent + 1;
		((TextWriter)(object)writer).Write("EXECUTE('ALTER TABLE ");
		((TextWriter)(object)writer).Write(Escape(Name(table)));
		((TextWriter)(object)writer).Write(" DROP CONSTRAINT [' + ");
		((TextWriter)(object)writer).Write(value);
		((TextWriter)(object)writer).WriteLine(" + ']')");
		indent = writer.Indent;
		writer.Indent = indent - 1;
	}

	protected virtual void Generate(DropTableOperation dropTableOperation)
	{
		Check.NotNull<DropTableOperation>(dropTableOperation, "dropTableOperation");
		IndentedTextWriter val = Writer();
		try
		{
			((TextWriter)(object)val).Write("DROP TABLE ");
			((TextWriter)(object)val).Write(Name(dropTableOperation.Name));
			Statement(val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	protected virtual void Generate(SqlOperation sqlOperation)
	{
		Check.NotNull<SqlOperation>(sqlOperation, "sqlOperation");
		StatementBatch(sqlOperation.Sql, sqlOperation.SuppressTransaction);
	}

	protected virtual void Generate(RenameColumnOperation renameColumnOperation)
	{
		Check.NotNull<RenameColumnOperation>(renameColumnOperation, "renameColumnOperation");
		IndentedTextWriter val = Writer();
		try
		{
			((TextWriter)(object)val).Write("EXECUTE sp_rename @objname = N'");
			((TextWriter)(object)val).Write(Escape(renameColumnOperation.Table));
			((TextWriter)(object)val).Write(".");
			((TextWriter)(object)val).Write(Escape(renameColumnOperation.Name));
			((TextWriter)(object)val).Write("', @newname = N'");
			((TextWriter)(object)val).Write(Escape(renameColumnOperation.NewName));
			((TextWriter)(object)val).Write("', @objtype = N'COLUMN'");
			Statement(val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	protected virtual void Generate(RenameIndexOperation renameIndexOperation)
	{
		Check.NotNull<RenameIndexOperation>(renameIndexOperation, "renameIndexOperation");
		IndentedTextWriter val = Writer();
		try
		{
			((TextWriter)(object)val).Write("EXECUTE sp_rename @objname = N'");
			((TextWriter)(object)val).Write(Escape(renameIndexOperation.Table));
			((TextWriter)(object)val).Write(".");
			((TextWriter)(object)val).Write(Escape(renameIndexOperation.Name));
			((TextWriter)(object)val).Write("', @newname = N'");
			((TextWriter)(object)val).Write(Escape(renameIndexOperation.NewName));
			((TextWriter)(object)val).Write("', @objtype = N'INDEX'");
			Statement(val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	protected virtual void Generate(RenameTableOperation renameTableOperation)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		Check.NotNull<RenameTableOperation>(renameTableOperation, "renameTableOperation");
		IndentedTextWriter val = Writer();
		try
		{
			WriteRenameTable(renameTableOperation, val);
			string identifier = PrimaryKeyOperation.BuildDefaultName(renameTableOperation.Name);
			string s = PrimaryKeyOperation.BuildDefaultName(((RenameTableOperation)((MigrationOperation)renameTableOperation).Inverse).Name);
			((TextWriter)(object)val).WriteLine();
			((TextWriter)(object)val).Write("IF object_id('");
			((TextWriter)(object)val).Write(Escape(Quote(identifier)));
			((TextWriter)(object)val).WriteLine("') IS NOT NULL BEGIN");
			int indent = val.Indent;
			val.Indent = indent + 1;
			((TextWriter)(object)val).Write("EXECUTE sp_rename @objname = N'");
			((TextWriter)(object)val).Write(Escape(Quote(identifier)));
			((TextWriter)(object)val).Write("', @newname = N'");
			((TextWriter)(object)val).Write(Escape(s));
			((TextWriter)(object)val).WriteLine("', @objtype = N'OBJECT'");
			indent = val.Indent;
			val.Indent = indent - 1;
			((TextWriter)(object)val).Write("END");
			Statement(val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private static void WriteRenameTable(RenameTableOperation renameTableOperation, IndentedTextWriter writer)
	{
		((TextWriter)(object)writer).Write("EXECUTE sp_rename @objname = N'");
		((TextWriter)(object)writer).Write(Escape(renameTableOperation.Name));
		((TextWriter)(object)writer).Write("', @newname = N'");
		((TextWriter)(object)writer).Write(Escape(renameTableOperation.NewName));
		((TextWriter)(object)writer).Write("', @objtype = N'OBJECT'");
	}

	protected virtual void Generate(RenameProcedureOperation renameProcedureOperation)
	{
		Check.NotNull<RenameProcedureOperation>(renameProcedureOperation, "renameProcedureOperation");
		IndentedTextWriter val = Writer();
		try
		{
			((TextWriter)(object)val).Write("EXECUTE sp_rename @objname = N'");
			((TextWriter)(object)val).Write(Escape(renameProcedureOperation.Name));
			((TextWriter)(object)val).Write("', @newname = N'");
			((TextWriter)(object)val).Write(Escape(renameProcedureOperation.NewName));
			((TextWriter)(object)val).Write("', @objtype = N'OBJECT'");
			Statement(val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	protected virtual void Generate(MoveProcedureOperation moveProcedureOperation)
	{
		Check.NotNull<MoveProcedureOperation>(moveProcedureOperation, "moveProcedureOperation");
		string text = moveProcedureOperation.NewSchema ?? "dbo";
		if (!text.EqualsIgnoreCase("dbo") && !_generatedSchemas.Contains(text))
		{
			GenerateCreateSchema(text);
			_generatedSchemas.Add(text);
		}
		IndentedTextWriter val = Writer();
		try
		{
			((TextWriter)(object)val).Write("ALTER SCHEMA ");
			((TextWriter)(object)val).Write(Quote(text));
			((TextWriter)(object)val).Write(" TRANSFER ");
			((TextWriter)(object)val).Write(Name(moveProcedureOperation.Name));
			Statement(val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	protected virtual void Generate(MoveTableOperation moveTableOperation)
	{
		Check.NotNull<MoveTableOperation>(moveTableOperation, "moveTableOperation");
		string text = moveTableOperation.NewSchema ?? "dbo";
		if (!text.EqualsIgnoreCase("dbo") && !_generatedSchemas.Contains(text))
		{
			GenerateCreateSchema(text);
			_generatedSchemas.Add(text);
		}
		if (!moveTableOperation.IsSystem)
		{
			IndentedTextWriter val = Writer();
			try
			{
				((TextWriter)(object)val).Write("ALTER SCHEMA ");
				((TextWriter)(object)val).Write(Quote(text));
				((TextWriter)(object)val).Write(" TRANSFER ");
				((TextWriter)(object)val).Write(Name(moveTableOperation.Name));
				Statement(val);
				return;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		IndentedTextWriter val2 = Writer();
		try
		{
			((TextWriter)(object)val2).Write("IF object_id('");
			((TextWriter)(object)val2).Write(moveTableOperation.CreateTableOperation.Name);
			((TextWriter)(object)val2).WriteLine("') IS NULL BEGIN");
			int indent = val2.Indent;
			val2.Indent = indent + 1;
			WriteCreateTable(moveTableOperation.CreateTableOperation, val2);
			((TextWriter)(object)val2).WriteLine();
			indent = val2.Indent;
			val2.Indent = indent - 1;
			((TextWriter)(object)val2).WriteLine("END");
			((TextWriter)(object)val2).Write("INSERT INTO ");
			((TextWriter)(object)val2).WriteLine(Name(moveTableOperation.CreateTableOperation.Name));
			((TextWriter)(object)val2).Write("SELECT * FROM ");
			((TextWriter)(object)val2).WriteLine(Name(moveTableOperation.Name));
			((TextWriter)(object)val2).Write("WHERE [ContextKey] = ");
			((TextWriter)(object)val2).WriteLine(Generate(moveTableOperation.ContextKey));
			((TextWriter)(object)val2).Write("DELETE ");
			((TextWriter)(object)val2).WriteLine(Name(moveTableOperation.Name));
			((TextWriter)(object)val2).Write("WHERE [ContextKey] = ");
			((TextWriter)(object)val2).WriteLine(Generate(moveTableOperation.ContextKey));
			((TextWriter)(object)val2).Write("IF NOT EXISTS(SELECT * FROM ");
			((TextWriter)(object)val2).Write(Name(moveTableOperation.Name));
			((TextWriter)(object)val2).WriteLine(")");
			indent = val2.Indent;
			val2.Indent = indent + 1;
			((TextWriter)(object)val2).Write("DROP TABLE ");
			((TextWriter)(object)val2).Write(Name(moveTableOperation.Name));
			indent = val2.Indent;
			val2.Indent = indent - 1;
			Statement(val2);
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	protected internal virtual void Generate(ColumnModel column, IndentedTextWriter writer)
	{
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Invalid comparison between Unknown and I4
		Check.NotNull<ColumnModel>(column, "column");
		Check.NotNull<IndentedTextWriter>(writer, "writer");
		((TextWriter)(object)writer).Write(Quote(((PropertyModel)column).Name));
		((TextWriter)(object)writer).Write(" ");
		((TextWriter)(object)writer).Write(BuildColumnType(column));
		if (column.IsNullable.HasValue && !column.IsNullable.Value)
		{
			((TextWriter)(object)writer).Write(" NOT NULL");
		}
		if (((PropertyModel)column).DefaultValue != null)
		{
			((TextWriter)(object)writer).Write(" DEFAULT ");
			writer.Write(Generate((dynamic)((PropertyModel)column).DefaultValue));
		}
		else if (!string.IsNullOrWhiteSpace(((PropertyModel)column).DefaultValueSql))
		{
			((TextWriter)(object)writer).Write(" DEFAULT ");
			((TextWriter)(object)writer).Write(((PropertyModel)column).DefaultValueSql);
		}
		else if (column.IsIdentity)
		{
			if ((int)((PropertyModel)column).Type == 6 && ((PropertyModel)column).DefaultValue == null)
			{
				((TextWriter)(object)writer).Write(" DEFAULT " + GuidColumnDefault);
			}
			else
			{
				((TextWriter)(object)writer).Write(" IDENTITY");
			}
		}
	}

	protected virtual void Generate(HistoryOperation historyOperation)
	{
		Check.NotNull<HistoryOperation>(historyOperation, "historyOperation");
		IndentedTextWriter writer = Writer();
		try
		{
			historyOperation.CommandTrees.Each(delegate(DbModificationCommandTree commandTree)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_0009: Invalid comparison between Unknown and I4
				//IL_0017: Unknown result type (might be due to invalid IL or missing references)
				//IL_0031: Expected O, but got Unknown
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_000d: Invalid comparison between Unknown and I4
				//IL_003e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0057: Expected O, but got Unknown
				DbCommandTreeKind commandTreeKind = ((DbCommandTree)commandTree).CommandTreeKind;
				List<SqlParameter> parameters;
				if ((int)commandTreeKind != 2)
				{
					if ((int)commandTreeKind == 3)
					{
						((TextWriter)(object)writer).Write(DmlSqlGenerator.GenerateDeleteSql((DbDeleteCommandTree)commandTree, _sqlGenerator, out parameters, upperCaseKeywords: true, createParameters: false));
					}
				}
				else
				{
					((TextWriter)(object)writer).Write(DmlSqlGenerator.GenerateInsertSql((DbInsertCommandTree)commandTree, _sqlGenerator, out parameters, generateReturningSql: false, upperCaseKeywords: true, createParameters: false));
				}
			});
			Statement(writer);
		}
		finally
		{
			if (writer != null)
			{
				((IDisposable)writer).Dispose();
			}
		}
	}

	protected virtual string Generate(byte[] defaultValue)
	{
		Check.NotNull(defaultValue, "defaultValue");
		return "0x" + defaultValue.ToHexString();
	}

	protected virtual string Generate(bool defaultValue)
	{
		if (!defaultValue)
		{
			return "0";
		}
		return "1";
	}

	protected virtual string Generate(DateTime defaultValue)
	{
		return "'" + defaultValue.ToString("yyyy-MM-ddTHH:mm:ss.fffK", CultureInfo.InvariantCulture) + "'";
	}

	protected virtual string Generate(DateTimeOffset defaultValue)
	{
		return "'" + defaultValue.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz", CultureInfo.InvariantCulture) + "'";
	}

	protected virtual string Generate(Guid defaultValue)
	{
		Guid guid = defaultValue;
		return "'" + guid.ToString() + "'";
	}

	protected virtual string Generate(string defaultValue)
	{
		Check.NotNull(defaultValue, "defaultValue");
		return "'" + defaultValue + "'";
	}

	protected virtual string Generate(TimeSpan defaultValue)
	{
		TimeSpan timeSpan = defaultValue;
		return "'" + timeSpan.ToString() + "'";
	}

	protected virtual string Generate(HierarchyId defaultValue)
	{
		return "cast('" + ((object)defaultValue)?.ToString() + "' as hierarchyid)";
	}

	protected virtual string Generate(DbGeography defaultValue)
	{
		return "'" + ((object)defaultValue)?.ToString() + "'";
	}

	protected virtual string Generate(DbGeometry defaultValue)
	{
		return "'" + ((object)defaultValue)?.ToString() + "'";
	}

	protected virtual string Generate(object defaultValue)
	{
		Check.NotNull(defaultValue, "defaultValue");
		return string.Format(CultureInfo.InvariantCulture, "{0}", new object[1] { defaultValue });
	}

	protected virtual string BuildColumnType(ColumnModel columnModel)
	{
		Check.NotNull<ColumnModel>(columnModel, "columnModel");
		if (columnModel.IsTimestamp)
		{
			return "rowversion";
		}
		return BuildPropertyType((PropertyModel)(object)columnModel);
	}

	private string BuildPropertyType(PropertyModel propertyModel)
	{
		string text = propertyModel.StoreType;
		TypeUsage val = ((MigrationSqlGenerator)this).ProviderManifest.GetStoreType(propertyModel.TypeUsage);
		if (string.IsNullOrWhiteSpace(text))
		{
			text = val.EdmType.Name;
		}
		else
		{
			val = ((MigrationSqlGenerator)this).BuildStoreTypeUsage(text, propertyModel) ?? val;
		}
		string text2 = text;
		text2 = ((!text2.EndsWith("(max)", StringComparison.Ordinal)) ? Quote(text2) : (Quote(text2.Substring(0, text2.Length - "(max)".Length)) + "(max)"));
		switch (text)
		{
		case "decimal":
		case "numeric":
			text2 = text2 + "(" + (propertyModel.Precision ?? val.GetPrecision()) + ", " + (propertyModel.Scale ?? val.GetScale()) + ")";
			break;
		case "datetime2":
		case "datetimeoffset":
		case "time":
			text2 = text2 + "(" + (propertyModel.Precision ?? val.GetPrecision()) + ")";
			break;
		case "binary":
		case "varbinary":
		case "nvarchar":
		case "varchar":
		case "char":
		case "nchar":
			text2 = text2 + "(" + (propertyModel.MaxLength ?? val.GetMaxLength()) + ")";
			break;
		}
		return text2;
	}

	protected virtual string Name(string name)
	{
		Check.NotEmpty(name, "name");
		DatabaseName databaseName = DatabaseName.Parse(name);
		return new string[2] { databaseName.Schema, databaseName.Name }.Join(Quote, ".");
	}

	protected virtual string Quote(string identifier)
	{
		Check.NotEmpty(identifier, "identifier");
		return SqlGenerator.QuoteIdentifier(identifier);
	}

	private static string Escape(string s)
	{
		return s.Replace("'", "''");
	}

	private static string Indent(string s, string indentation)
	{
		return new Regex("\\r?\\n *").Replace(s, Environment.NewLine + indentation);
	}

	protected void Statement(string sql, bool suppressTransaction = false, string batchTerminator = null)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		Check.NotEmpty(sql, "sql");
		_statements.Add(new MigrationStatement
		{
			Sql = sql,
			SuppressTransaction = suppressTransaction,
			BatchTerminator = batchTerminator
		});
	}

	protected static IndentedTextWriter Writer()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		return new IndentedTextWriter((TextWriter)new StringWriter(CultureInfo.InvariantCulture));
	}

	protected void Statement(IndentedTextWriter writer, string batchTerminator = null)
	{
		Check.NotNull<IndentedTextWriter>(writer, "writer");
		Statement(writer.InnerWriter.ToString(), suppressTransaction: false, batchTerminator);
	}

	protected void StatementBatch(string sqlBatch, bool suppressTransaction = false)
	{
		Check.NotNull(sqlBatch, "sqlBatch");
		sqlBatch = Regex.Replace(sqlBatch, "\\\\(\\r\\n|\\r|\\n)", "");
		string[] array = Regex.Split(sqlBatch, string.Format(CultureInfo.InvariantCulture, "^\\s*({0}[ \\t]+[0-9]+|{0})(?:\\s+|$)", new object[1] { "GO" }), RegexOptions.IgnoreCase | RegexOptions.Multiline);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].StartsWith("GO", StringComparison.OrdinalIgnoreCase) || (i == array.Length - 1 && string.IsNullOrWhiteSpace(array[i])))
			{
				continue;
			}
			if (array.Length > i + 1 && array[i + 1].StartsWith("GO", StringComparison.OrdinalIgnoreCase))
			{
				int num = 1;
				if (!array[i + 1].EqualsIgnoreCase("GO"))
				{
					num = int.Parse(Regex.Match(array[i + 1], "([0-9]+)").Value, CultureInfo.InvariantCulture);
				}
				for (int j = 0; j < num; j++)
				{
					Statement(array[i], suppressTransaction, "GO");
				}
			}
			else
			{
				Statement(array[i], suppressTransaction);
			}
		}
	}

	private static IEnumerable<MigrationOperation> DetectHistoryRebuild(IEnumerable<MigrationOperation> operations)
	{
		IEnumerator<MigrationOperation> enumerator = operations.GetEnumerator();
		while (enumerator.MoveNext())
		{
			HistoryRebuildOperationSequence historyRebuildOperationSequence = HistoryRebuildOperationSequence.Detect(enumerator);
			yield return (MigrationOperation)(((object)historyRebuildOperationSequence) ?? ((object)enumerator.Current));
		}
	}

	private void Generate(HistoryRebuildOperationSequence sequence)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Invalid comparison between Unknown and I4
		CreateTableOperation createTableOperation = sequence.DropPrimaryKeyOperation.CreateTableOperation;
		CreateTableOperation val = ResolveNameConflicts(createTableOperation);
		RenameTableOperation renameTableOperation = new RenameTableOperation(val.Name, "__MigrationHistory", (object)null);
		IndentedTextWriter val2 = Writer();
		try
		{
			WriteCreateTable(val, val2);
			((TextWriter)(object)val2).WriteLine();
			((TextWriter)(object)val2).Write("INSERT INTO ");
			((TextWriter)(object)val2).WriteLine(Name(val.Name));
			((TextWriter)(object)val2).Write("SELECT ");
			bool flag = true;
			foreach (ColumnModel column in createTableOperation.Columns)
			{
				if (flag)
				{
					flag = false;
				}
				else
				{
					((TextWriter)(object)val2).Write(", ");
				}
				((TextWriter)(object)val2).Write((((PropertyModel)column).Name == ((PropertyModel)sequence.AddColumnOperation.Column).Name) ? Generate((string)((PropertyModel)sequence.AddColumnOperation.Column).DefaultValue) : (((int)((PropertyModel)column).Type == 12) ? ("LEFT(" + Name(((PropertyModel)column).Name) + ", " + ((PropertyModel)column).MaxLength + ")") : Name(((PropertyModel)column).Name)));
			}
			((TextWriter)(object)val2).Write(" FROM ");
			((TextWriter)(object)val2).WriteLine(Name(createTableOperation.Name));
			((TextWriter)(object)val2).Write("DROP TABLE ");
			((TextWriter)(object)val2).WriteLine(Name(createTableOperation.Name));
			WriteRenameTable(renameTableOperation, val2);
			Statement(val2);
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	private static CreateTableOperation ResolveNameConflicts(CreateTableOperation source)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_003f: Expected O, but got Unknown
		CreateTableOperation target = new CreateTableOperation(source.Name + "2", (object)null)
		{
			PrimaryKey = new AddPrimaryKeyOperation((object)null)
			{
				IsClustered = ((PrimaryKeyOperation)source.PrimaryKey).IsClustered
			}
		};
		source.Columns.Each(delegate(ColumnModel c)
		{
			target.Columns.Add(c);
		});
		((PrimaryKeyOperation)source.PrimaryKey).Columns.Each(delegate(string c)
		{
			((PrimaryKeyOperation)target.PrimaryKey).Columns.Add(c);
		});
		return target;
	}
}
