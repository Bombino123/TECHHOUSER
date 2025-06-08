using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.SqlServer.Utilities;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace System.Data.Entity.SqlServer;

internal sealed class SqlDdlBuilder
{
	private readonly StringBuilder unencodedStringBuilder = new StringBuilder();

	private readonly HashSet<EntitySet> ignoredEntitySets = new HashSet<EntitySet>();

	internal static string CreateObjectsScript(StoreItemCollection itemCollection, bool createSchemas)
	{
		SqlDdlBuilder sqlDdlBuilder = new SqlDdlBuilder();
		foreach (EntityContainer item in ((ItemCollection)itemCollection).GetItems<EntityContainer>())
		{
			IOrderedEnumerable<EntitySet> source = from s in ((IEnumerable)item.BaseEntitySets).OfType<EntitySet>()
				orderby ((EntitySetBase)s).Name
				select s;
			if (createSchemas)
			{
				foreach (string item2 in new HashSet<string>(source.Select((EntitySet s) => GetSchemaName(s))).OrderBy((string s) => s))
				{
					if (item2 != "dbo")
					{
						sqlDdlBuilder.AppendCreateSchema(item2);
					}
				}
			}
			foreach (EntitySet item3 in from s in ((IEnumerable)item.BaseEntitySets).OfType<EntitySet>()
				orderby ((EntitySetBase)s).Name
				select s)
			{
				sqlDdlBuilder.AppendCreateTable(item3);
			}
			foreach (AssociationSet item4 in from s in ((IEnumerable)item.BaseEntitySets).OfType<AssociationSet>()
				orderby ((EntitySetBase)s).Name
				select s)
			{
				sqlDdlBuilder.AppendCreateForeignKeys(item4);
			}
		}
		return sqlDdlBuilder.GetCommandText();
	}

	internal static string CreateDatabaseScript(string databaseName, string dataFileName, string logFileName)
	{
		SqlDdlBuilder sqlDdlBuilder = new SqlDdlBuilder();
		sqlDdlBuilder.AppendSql("create database ");
		sqlDdlBuilder.AppendIdentifier(databaseName);
		if (dataFileName != null)
		{
			sqlDdlBuilder.AppendSql(" on primary ");
			sqlDdlBuilder.AppendFileName(dataFileName);
			sqlDdlBuilder.AppendSql(" log on ");
			sqlDdlBuilder.AppendFileName(logFileName);
		}
		return sqlDdlBuilder.unencodedStringBuilder.ToString();
	}

	internal static string SetDatabaseOptionsScript(SqlVersion sqlVersion, string databaseName)
	{
		if (sqlVersion < SqlVersion.Sql9)
		{
			return string.Empty;
		}
		SqlDdlBuilder sqlDdlBuilder = new SqlDdlBuilder();
		sqlDdlBuilder.AppendSql("if serverproperty('EngineEdition') <> 5 execute sp_executesql ");
		sqlDdlBuilder.AppendStringLiteral(SetReadCommittedSnapshotScript(databaseName));
		return sqlDdlBuilder.unencodedStringBuilder.ToString();
	}

	private static string SetReadCommittedSnapshotScript(string databaseName)
	{
		SqlDdlBuilder sqlDdlBuilder = new SqlDdlBuilder();
		sqlDdlBuilder.AppendSql("alter database ");
		sqlDdlBuilder.AppendIdentifier(databaseName);
		sqlDdlBuilder.AppendSql(" set read_committed_snapshot on");
		return sqlDdlBuilder.unencodedStringBuilder.ToString();
	}

	internal static string CreateDatabaseExistsScript(string databaseName)
	{
		SqlDdlBuilder sqlDdlBuilder = new SqlDdlBuilder();
		sqlDdlBuilder.AppendSql("IF db_id(");
		sqlDdlBuilder.AppendStringLiteral(databaseName);
		sqlDdlBuilder.AppendSql(") IS NOT NULL SELECT 1 ELSE SELECT Count(*) FROM sys.databases WHERE [name]=");
		sqlDdlBuilder.AppendStringLiteral(databaseName);
		return sqlDdlBuilder.unencodedStringBuilder.ToString();
	}

	private static void AppendSysDatabases(SqlDdlBuilder builder, bool useDeprecatedSystemTable)
	{
		if (useDeprecatedSystemTable)
		{
			builder.AppendSql("sysdatabases");
		}
		else
		{
			builder.AppendSql("sys.databases");
		}
	}

	internal static string CreateGetDatabaseNamesBasedOnFileNameScript(string databaseFileName, bool useDeprecatedSystemTable)
	{
		SqlDdlBuilder sqlDdlBuilder = new SqlDdlBuilder();
		sqlDdlBuilder.AppendSql("SELECT [d].[name] FROM ");
		AppendSysDatabases(sqlDdlBuilder, useDeprecatedSystemTable);
		sqlDdlBuilder.AppendSql(" AS [d] ");
		if (!useDeprecatedSystemTable)
		{
			sqlDdlBuilder.AppendSql("INNER JOIN sys.master_files AS [f] ON [f].[database_id] = [d].[database_id]");
		}
		sqlDdlBuilder.AppendSql(" WHERE [");
		if (useDeprecatedSystemTable)
		{
			sqlDdlBuilder.AppendSql("filename");
		}
		else
		{
			sqlDdlBuilder.AppendSql("f].[physical_name");
		}
		sqlDdlBuilder.AppendSql("]=");
		sqlDdlBuilder.AppendStringLiteral(databaseFileName);
		return sqlDdlBuilder.unencodedStringBuilder.ToString();
	}

	internal static string CreateCountDatabasesBasedOnFileNameScript(string databaseFileName, bool useDeprecatedSystemTable)
	{
		SqlDdlBuilder sqlDdlBuilder = new SqlDdlBuilder();
		sqlDdlBuilder.AppendSql("SELECT Count(*) FROM ");
		if (useDeprecatedSystemTable)
		{
			sqlDdlBuilder.AppendSql("sysdatabases");
		}
		if (!useDeprecatedSystemTable)
		{
			sqlDdlBuilder.AppendSql("sys.master_files");
		}
		sqlDdlBuilder.AppendSql(" WHERE [");
		if (useDeprecatedSystemTable)
		{
			sqlDdlBuilder.AppendSql("filename");
		}
		else
		{
			sqlDdlBuilder.AppendSql("physical_name");
		}
		sqlDdlBuilder.AppendSql("]=");
		sqlDdlBuilder.AppendStringLiteral(databaseFileName);
		return sqlDdlBuilder.unencodedStringBuilder.ToString();
	}

	internal static string DropDatabaseScript(string databaseName)
	{
		SqlDdlBuilder sqlDdlBuilder = new SqlDdlBuilder();
		sqlDdlBuilder.AppendSql("drop database ");
		sqlDdlBuilder.AppendIdentifier(databaseName);
		return sqlDdlBuilder.unencodedStringBuilder.ToString();
	}

	internal string GetCommandText()
	{
		return unencodedStringBuilder.ToString();
	}

	internal static string GetSchemaName(EntitySet entitySet)
	{
		return ((MetadataItem)(object)entitySet).GetMetadataPropertyValue<string>("Schema") ?? ((EntitySetBase)entitySet).EntityContainer.Name;
	}

	internal static string GetTableName(EntitySet entitySet)
	{
		return ((MetadataItem)(object)entitySet).GetMetadataPropertyValue<string>("Table") ?? ((EntitySetBase)entitySet).Name;
	}

	private void AppendCreateForeignKeys(AssociationSet associationSet)
	{
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Invalid comparison between Unknown and I4
		ReferentialConstraint val = ((IEnumerable<ReferentialConstraint>)associationSet.ElementType.ReferentialConstraints).Single();
		AssociationSetEnd val2 = associationSet.AssociationSetEnds[((EdmMember)val.FromRole).Name];
		AssociationSetEnd val3 = associationSet.AssociationSetEnds[((EdmMember)val.ToRole).Name];
		if (ignoredEntitySets.Contains(val2.EntitySet) || ignoredEntitySets.Contains(val3.EntitySet))
		{
			AppendSql("-- Ignoring association set with participating entity set with defining query: ");
			AppendIdentifierEscapeNewLine(((EntitySetBase)associationSet).Name);
		}
		else
		{
			AppendSql("alter table ");
			AppendIdentifier(val3.EntitySet);
			AppendSql(" add constraint ");
			AppendIdentifier(((EntitySetBase)associationSet).Name);
			AppendSql(" foreign key (");
			AppendIdentifiers((IEnumerable<EdmProperty>)val.ToProperties);
			AppendSql(") references ");
			AppendIdentifier(val2.EntitySet);
			AppendSql("(");
			AppendIdentifiers((IEnumerable<EdmProperty>)val.FromProperties);
			AppendSql(")");
			if ((int)((RelationshipEndMember)val2.CorrespondingAssociationEndMember).DeleteBehavior == 1)
			{
				AppendSql(" on delete cascade");
			}
			AppendSql(";");
		}
		AppendNewLine();
	}

	private void AppendCreateTable(EntitySet entitySet)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		if (((MetadataItem)(object)entitySet).GetMetadataPropertyValue<string>("DefiningQuery") != null)
		{
			AppendSql("-- Ignoring entity set with defining query: ");
			AppendIdentifier(entitySet, AppendIdentifierEscapeNewLine);
			ignoredEntitySets.Add(entitySet);
		}
		else
		{
			AppendSql("create table ");
			AppendIdentifier(entitySet);
			AppendSql(" (");
			AppendNewLine();
			Enumerator<EdmProperty> enumerator = entitySet.ElementType.Properties.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					EdmProperty current = enumerator.Current;
					AppendSql("    ");
					AppendIdentifier(((EdmMember)current).Name);
					AppendSql(" ");
					AppendType(current);
					AppendSql(",");
					AppendNewLine();
				}
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
			AppendSql("    primary key (");
			AppendJoin((IEnumerable<EdmMember>)((EntityTypeBase)entitySet.ElementType).KeyMembers, delegate(EdmMember k)
			{
				AppendIdentifier(k.Name);
			}, ", ");
			AppendSql(")");
			AppendNewLine();
			AppendSql(");");
		}
		AppendNewLine();
	}

	private void AppendCreateSchema(string schema)
	{
		AppendSql("if (schema_id(");
		AppendStringLiteral(schema);
		AppendSql(") is null) exec(");
		SqlDdlBuilder sqlDdlBuilder = new SqlDdlBuilder();
		sqlDdlBuilder.AppendSql("create schema ");
		sqlDdlBuilder.AppendIdentifier(schema);
		AppendStringLiteral(sqlDdlBuilder.unencodedStringBuilder.ToString());
		AppendSql(");");
		AppendNewLine();
	}

	private void AppendIdentifier(EntitySet table)
	{
		AppendIdentifier(table, AppendIdentifier);
	}

	private void AppendIdentifier(EntitySet table, Action<string> AppendIdentifierEscape)
	{
		string schemaName = GetSchemaName(table);
		string tableName = GetTableName(table);
		if (schemaName != null)
		{
			AppendIdentifierEscape(schemaName);
			AppendSql(".");
		}
		AppendIdentifierEscape(tableName);
	}

	private void AppendStringLiteral(string literalValue)
	{
		AppendSql("N'" + literalValue.Replace("'", "''") + "'");
	}

	private void AppendIdentifiers(IEnumerable<EdmProperty> properties)
	{
		AppendJoin(properties, delegate(EdmProperty p)
		{
			AppendIdentifier(((EdmMember)p).Name);
		}, ", ");
	}

	private void AppendIdentifier(string identifier)
	{
		AppendSql("[" + identifier.Replace("]", "]]") + "]");
	}

	private void AppendIdentifierEscapeNewLine(string identifier)
	{
		AppendIdentifier(identifier.Replace("\r", "\r--").Replace("\n", "\n--"));
	}

	private void AppendFileName(string path)
	{
		AppendSql("(name=");
		AppendStringLiteral(Path.GetFileName(path));
		AppendSql(", filename=");
		AppendStringLiteral(path);
		AppendSql(")");
	}

	private void AppendJoin<T>(IEnumerable<T> elements, Action<T> appendElement, string unencodedSeparator)
	{
		bool flag = true;
		foreach (T element in elements)
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				AppendSql(unencodedSeparator);
			}
			appendElement(element);
		}
	}

	private void AppendType(EdmProperty column)
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Invalid comparison between Unknown and I4
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Invalid comparison between I4 and Unknown
		//IL_032a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0330: Invalid comparison between Unknown and I4
		TypeUsage typeUsage = ((EdmMember)column).TypeUsage;
		bool flag = false;
		Facet val = default(Facet);
		if (typeUsage.EdmType.Name == "binary" && 8 == typeUsage.GetMaxLength() && ((EdmMember)column).TypeUsage.Facets.TryGetValue("StoreGeneratedPattern", false, ref val) && val.Value != null && 2 == (int)(StoreGeneratedPattern)val.Value)
		{
			flag = true;
			AppendIdentifier("rowversion");
		}
		else
		{
			string name = typeUsage.EdmType.Name;
			if ((int)((MetadataItem)typeUsage.EdmType).BuiltInTypeKind == 26 && name.EndsWith("(max)", StringComparison.Ordinal))
			{
				AppendIdentifier(name.Substring(0, name.Length - "(max)".Length));
				AppendSql("(max)");
			}
			else
			{
				AppendIdentifier(name);
			}
			switch (typeUsage.EdmType.Name)
			{
			case "decimal":
			case "numeric":
				AppendSqlInvariantFormat("({0}, {1})", typeUsage.GetPrecision(), typeUsage.GetScale());
				break;
			case "datetime2":
			case "datetimeoffset":
			case "time":
				AppendSqlInvariantFormat("({0})", typeUsage.GetPrecision());
				break;
			case "binary":
			case "varbinary":
			case "nvarchar":
			case "varchar":
			case "char":
			case "nchar":
				AppendSqlInvariantFormat("({0})", typeUsage.GetMaxLength());
				break;
			}
		}
		AppendSql(column.Nullable ? " null" : " not null");
		if (!flag && ((EdmMember)column).TypeUsage.Facets.TryGetValue("StoreGeneratedPattern", false, ref val) && val.Value != null && (int)(StoreGeneratedPattern)val.Value == 1)
		{
			if (typeUsage.EdmType.Name == "uniqueidentifier")
			{
				AppendSql(" default newid()");
			}
			else
			{
				AppendSql(" identity");
			}
		}
	}

	private void AppendSql(string text)
	{
		unencodedStringBuilder.Append(text);
	}

	private void AppendNewLine()
	{
		unencodedStringBuilder.Append("\r\n");
	}

	private void AppendSqlInvariantFormat(string format, params object[] args)
	{
		unencodedStringBuilder.AppendFormat(CultureInfo.InvariantCulture, format, args);
	}
}
