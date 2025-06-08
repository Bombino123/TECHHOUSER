using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Data.Common.CommandTrees;
using System.Data.Metadata.Edm;
using System.Data.SQLite.Linq.Properties;
using System.Globalization;
using System.Text;

namespace System.Data.SQLite.Linq;

internal sealed class SQLiteProviderServices : DbProviderServices, ISQLiteSchemaExtensions
{
	internal static readonly SQLiteProviderServices Instance = new SQLiteProviderServices();

	protected override DbCommandDefinition CreateDbCommandDefinition(DbProviderManifest manifest, DbCommandTree commandTree)
	{
		DbCommand dbCommand = CreateCommand(manifest, commandTree);
		return ((DbProviderServices)this).CreateCommandDefinition(dbCommand);
	}

	private DbCommand CreateCommand(DbProviderManifest manifest, DbCommandTree commandTree)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		if (manifest == null)
		{
			throw new ArgumentNullException("manifest");
		}
		if (commandTree == null)
		{
			throw new ArgumentNullException("commandTree");
		}
		SQLiteCommand val = new SQLiteCommand();
		try
		{
			((DbCommand)(object)val).CommandText = SqlGenerator.GenerateSql((SQLiteProviderManifest)(object)manifest, commandTree, out var parameters, out var commandType);
			((DbCommand)(object)val).CommandType = commandType;
			EdmFunction val2 = null;
			if (commandTree is DbFunctionCommandTree)
			{
				val2 = ((DbFunctionCommandTree)commandTree).EdmFunction;
			}
			FunctionParameter val3 = default(FunctionParameter);
			foreach (KeyValuePair<string, TypeUsage> parameter in commandTree.Parameters)
			{
				SQLiteParameter val4 = ((val2 == null || !val2.Parameters.TryGetValue(parameter.Key, false, ref val3)) ? CreateSqlParameter((SQLiteProviderManifest)(object)manifest, parameter.Key, parameter.Value, (ParameterMode)0, DBNull.Value) : CreateSqlParameter((SQLiteProviderManifest)(object)manifest, val3.Name, val3.TypeUsage, val3.Mode, DBNull.Value));
				val.Parameters.Add(val4);
			}
			if (parameters != null && 0 < parameters.Count)
			{
				if (!(commandTree is DbInsertCommandTree) && !(commandTree is DbUpdateCommandTree) && !(commandTree is DbDeleteCommandTree))
				{
					throw new InvalidOperationException("SqlGenParametersNotPermitted");
				}
				foreach (DbParameter item in parameters)
				{
					((DbParameterCollection)(object)val.Parameters).Add((object)item);
				}
			}
			return (DbCommand)(object)val;
		}
		catch
		{
			((Component)(object)val).Dispose();
			throw;
		}
	}

	protected override string GetDbProviderManifestToken(DbConnection connection)
	{
		if (connection == null)
		{
			throw new ArgumentNullException("connection");
		}
		if (string.IsNullOrEmpty(connection.ConnectionString))
		{
			throw new ArgumentNullException("ConnectionString");
		}
		return connection.ConnectionString;
	}

	protected override DbProviderManifest GetDbProviderManifest(string versionHint)
	{
		return (DbProviderManifest)(object)new SQLiteProviderManifest(versionHint);
	}

	internal static SQLiteParameter CreateSqlParameter(SQLiteProviderManifest manifest, string name, TypeUsage type, ParameterMode mode, object value)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Invalid comparison between Unknown and I4
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		if (manifest != null && !manifest._binaryGuid && (int)MetadataHelpers.GetPrimitiveTypeKind(type) == 6)
		{
			type = TypeUsage.CreateStringTypeUsage(PrimitiveType.GetEdmPrimitiveType((PrimitiveTypeKind)12), false, true);
		}
		SQLiteParameter val = new SQLiteParameter(name, value);
		ParameterDirection parameterDirection = MetadataHelpers.ParameterModeToParameterDirection(mode);
		if (((DbParameter)(object)val).Direction != parameterDirection)
		{
			((DbParameter)(object)val).Direction = parameterDirection;
		}
		bool flag = (int)mode > 0;
		int? size;
		DbType sqlDbType = GetSqlDbType(type, flag, out size);
		if (((DbParameter)(object)val).DbType != sqlDbType)
		{
			((DbParameter)(object)val).DbType = sqlDbType;
		}
		if (size.HasValue && (flag || ((DbParameter)(object)val).Size != size.Value))
		{
			((DbParameter)(object)val).Size = size.Value;
		}
		bool flag2 = MetadataHelpers.IsNullable(type);
		if (flag || flag2 != ((DbParameter)(object)val).IsNullable)
		{
			((DbParameter)(object)val).IsNullable = flag2;
		}
		return val;
	}

	private static DbType GetSqlDbType(TypeUsage type, bool isOutParam, out int? size)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected I4, but got Unknown
		PrimitiveTypeKind primitiveTypeKind = MetadataHelpers.GetPrimitiveTypeKind(type);
		size = null;
		switch ((int)primitiveTypeKind)
		{
		case 0:
			size = GetParameterSize(type, isOutParam);
			return GetBinaryDbType(type);
		case 1:
			return DbType.Boolean;
		case 2:
			return DbType.Byte;
		case 13:
			return DbType.Time;
		case 14:
			return DbType.DateTimeOffset;
		case 3:
			return DbType.DateTime;
		case 4:
			return DbType.Decimal;
		case 5:
			return DbType.Double;
		case 6:
			return DbType.Guid;
		case 9:
			return DbType.Int16;
		case 10:
			return DbType.Int32;
		case 11:
			return DbType.Int64;
		case 8:
			return DbType.SByte;
		case 7:
			return DbType.Single;
		case 12:
			size = GetParameterSize(type, isOutParam);
			return GetStringDbType(type);
		default:
			return DbType.Object;
		}
	}

	private static int? GetParameterSize(TypeUsage type, bool isOutParam)
	{
		if (MetadataHelpers.TryGetMaxLength(type, out var maxLength))
		{
			return maxLength;
		}
		if (isOutParam)
		{
			return int.MaxValue;
		}
		return null;
	}

	private static DbType GetStringDbType(TypeUsage type)
	{
		if (!MetadataHelpers.TryGetIsFixedLength(type, out var isFixedLength))
		{
			isFixedLength = false;
		}
		if (!MetadataHelpers.TryGetIsUnicode(type, out var isUnicode))
		{
			isUnicode = true;
		}
		if (isFixedLength)
		{
			return isUnicode ? DbType.StringFixedLength : DbType.AnsiStringFixedLength;
		}
		return isUnicode ? DbType.String : DbType.AnsiString;
	}

	private static DbType GetBinaryDbType(TypeUsage type)
	{
		if (!MetadataHelpers.TryGetIsFixedLength(type, out var isFixedLength))
		{
			isFixedLength = false;
		}
		return DbType.Binary;
	}

	void ISQLiteSchemaExtensions.BuildTempSchema(SQLiteConnection cnn)
	{
		string[] array = new string[8] { "TABLES", "COLUMNS", "VIEWS", "VIEWCOLUMNS", "INDEXES", "INDEXCOLUMNS", "FOREIGNKEYS", "CATALOGS" };
		using (DataTable dataTable = ((DbConnection)(object)cnn).GetSchema("Tables", new string[3]
		{
			"temp",
			null,
			$"SCHEMA{array[0]}"
		}))
		{
			if (dataTable.Rows.Count > 0)
			{
				return;
			}
		}
		for (int i = 0; i < array.Length; i++)
		{
			using DataTable table = ((DbConnection)(object)cnn).GetSchema(array[i]);
			DataTableToTable(cnn, table, $"SCHEMA{array[i]}");
		}
		SQLiteCommand val = cnn.CreateCommand();
		try
		{
			((DbCommand)(object)val).CommandText = System.Data.SQLite.Linq.Properties.Resources.SQL_CONSTRAINTS;
			((DbCommand)(object)val).ExecuteNonQuery();
			((DbCommand)(object)val).CommandText = System.Data.SQLite.Linq.Properties.Resources.SQL_CONSTRAINTCOLUMNS;
			((DbCommand)(object)val).ExecuteNonQuery();
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void DataTableToTable(SQLiteConnection cnn, DataTable table, string dest)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Expected O, but got Unknown
		StringBuilder stringBuilder = new StringBuilder();
		SQLiteCommandBuilder val = new SQLiteCommandBuilder();
		SQLiteCommand val2 = cnn.CreateCommand();
		try
		{
			using DataTable dataTable = new DataTable();
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "CREATE TEMP TABLE {0} (", ((DbCommandBuilder)(object)val).QuoteIdentifier(dest));
			string arg = string.Empty;
			SQLiteConnectionFlags flags = cnn.Flags;
			foreach (DataColumn column in table.Columns)
			{
				DbType dbType = SQLiteConvert.TypeToDbType(column.DataType);
				string arg2 = SQLiteConvert.DbTypeToTypeName(cnn, dbType, flags);
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{2}{0} {1} COLLATE NOCASE", ((DbCommandBuilder)(object)val).QuoteIdentifier(column.ColumnName), arg2, arg);
				arg = ", ";
			}
			stringBuilder.Append(")");
			((DbCommand)(object)val2).CommandText = stringBuilder.ToString();
			((DbCommand)(object)val2).ExecuteNonQuery();
			((DbCommand)(object)val2).CommandText = $"SELECT * FROM TEMP.{((DbCommandBuilder)(object)val).QuoteIdentifier(dest)} WHERE 1=2";
			SQLiteDataAdapter val3 = new SQLiteDataAdapter(val2);
			try
			{
				val.DataAdapter = val3;
				((DbDataAdapter)(object)val3).Fill(dataTable);
				foreach (DataRow row in table.Rows)
				{
					object[] itemArray = row.ItemArray;
					dataTable.Rows.Add(itemArray);
				}
				((DbDataAdapter)(object)val3).Update(dataTable);
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}
}
