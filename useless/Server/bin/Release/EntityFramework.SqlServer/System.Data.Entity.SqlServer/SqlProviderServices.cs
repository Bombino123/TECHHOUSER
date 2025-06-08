using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Hierarchy;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.Migrations.Sql;
using System.Data.Entity.Spatial;
using System.Data.Entity.SqlServer.Resources;
using System.Data.Entity.SqlServer.SqlGen;
using System.Data.Entity.SqlServer.Utilities;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;

namespace System.Data.Entity.SqlServer;

public sealed class SqlProviderServices : DbProviderServices
{
	public const string ProviderInvariantName = "System.Data.SqlClient";

	private ConcurrentDictionary<string, SqlProviderManifest> _providerManifests = new ConcurrentDictionary<string, SqlProviderManifest>();

	private static readonly SqlProviderServices _providerInstance = new SqlProviderServices();

	private static bool _truncateDecimalsToScale = true;

	private static bool _useScopeIdentity = true;

	private static bool _useRowNumberOrderingInOffsetQueries = true;

	public static SqlProviderServices Instance => _providerInstance;

	public static string SqlServerTypesAssemblyName { get; set; }

	public static bool TruncateDecimalsToScale
	{
		get
		{
			return _truncateDecimalsToScale;
		}
		set
		{
			_truncateDecimalsToScale = value;
		}
	}

	public static bool UseScopeIdentity
	{
		get
		{
			return _useScopeIdentity;
		}
		set
		{
			_useScopeIdentity = value;
		}
	}

	public static bool UseRowNumberOrderingInOffsetQueries
	{
		get
		{
			return _useRowNumberOrderingInOffsetQueries;
		}
		set
		{
			_useRowNumberOrderingInOffsetQueries = value;
		}
	}

	private SqlProviderServices()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		((DbProviderServices)this).AddDependencyResolver((IDbDependencyResolver)(object)new SingletonDependencyResolver<IDbConnectionFactory>((IDbConnectionFactory)new LocalDbConnectionFactory()));
		((DbProviderServices)this).AddDependencyResolver((IDbDependencyResolver)(object)new ExecutionStrategyResolver<DefaultSqlExecutionStrategy>("System.Data.SqlClient", (string)null, (Func<DefaultSqlExecutionStrategy>)(() => new DefaultSqlExecutionStrategy())));
		((DbProviderServices)this).AddDependencyResolver((IDbDependencyResolver)(object)new SingletonDependencyResolver<Func<MigrationSqlGenerator>>((Func<MigrationSqlGenerator>)(() => (MigrationSqlGenerator)(object)new SqlServerMigrationSqlGenerator()), (object)"System.Data.SqlClient"));
		((DbProviderServices)this).AddDependencyResolver((IDbDependencyResolver)(object)new SingletonDependencyResolver<TableExistenceChecker>((TableExistenceChecker)(object)new SqlTableExistenceChecker(), (object)"System.Data.SqlClient"));
		((DbProviderServices)this).AddDependencyResolver((IDbDependencyResolver)(object)new SingletonDependencyResolver<DbSpatialServices>((DbSpatialServices)(object)SqlSpatialServices.Instance, (Func<object, bool>)delegate(object k)
		{
			if (k == null)
			{
				return true;
			}
			DbProviderInfo val = (DbProviderInfo)((k is DbProviderInfo) ? k : null);
			return val != null && val.ProviderInvariantName == "System.Data.SqlClient" && SupportsSpatial(val.ProviderManifestToken);
		}));
	}

	public override void RegisterInfoMessageHandler(DbConnection connection, Action<string> handler)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		Check.NotNull(connection, "connection");
		Check.NotNull(handler, "handler");
		((SqlConnection)(((connection is SqlConnection) ? connection : null) ?? throw new ArgumentException(Strings.Mapping_Provider_WrongConnectionType(typeof(SqlConnection))))).InfoMessage += (SqlInfoMessageEventHandler)delegate(object _, SqlInfoMessageEventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(e.Message))
			{
				handler(e.Message);
			}
		};
	}

	protected override DbCommandDefinition CreateDbCommandDefinition(DbProviderManifest providerManifest, DbCommandTree commandTree)
	{
		Check.NotNull<DbProviderManifest>(providerManifest, "providerManifest");
		Check.NotNull<DbCommandTree>(commandTree, "commandTree");
		DbCommand dbCommand = CreateCommand(providerManifest, commandTree);
		return ((DbProviderServices)this).CreateCommandDefinition(dbCommand);
	}

	protected override DbCommand CloneDbCommand(DbCommand fromDbCommand)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		Check.NotNull(fromDbCommand, "fromDbCommand");
		SqlCommand val = (SqlCommand)(object)((fromDbCommand is SqlCommand) ? fromDbCommand : null);
		if (val == null)
		{
			return ((DbProviderServices)this).CloneDbCommand(fromDbCommand);
		}
		SqlCommand val2 = new SqlCommand();
		((DbCommand)(object)val2).CommandText = ((DbCommand)(object)val).CommandText;
		((DbCommand)(object)val2).CommandTimeout = ((DbCommand)(object)val).CommandTimeout;
		((DbCommand)(object)val2).CommandType = ((DbCommand)(object)val).CommandType;
		val2.Connection = val.Connection;
		val2.Transaction = val.Transaction;
		((DbCommand)(object)val2).UpdatedRowSource = ((DbCommand)(object)val).UpdatedRowSource;
		foreach (object item in (DbParameterCollection)(object)val.Parameters)
		{
			ICloneable cloneable = item as ICloneable;
			((DbParameterCollection)(object)val2.Parameters).Add((cloneable == null) ? item : cloneable.Clone());
		}
		return (DbCommand)(object)val2;
	}

	private static DbCommand CreateCommand(DbProviderManifest providerManifest, DbCommandTree commandTree)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Invalid comparison between Unknown and I4
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Invalid comparison between Unknown and I4
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Invalid comparison between Unknown and I4
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Invalid comparison between Unknown and I4
		SqlVersion sqlVersion = ((providerManifest as SqlProviderManifest) ?? throw new ArgumentException(Strings.Mapping_Provider_WrongManifestType(typeof(SqlProviderManifest)))).SqlVersion;
		SqlCommand val = new SqlCommand();
		((DbCommand)(object)val).CommandText = SqlGenerator.GenerateSql(commandTree, sqlVersion, out var parameters, out var commandType, out var paramsToForceNonUnicode);
		((DbCommand)(object)val).CommandType = commandType;
		EdmFunction val2 = null;
		if ((int)commandTree.CommandTreeKind == 4)
		{
			val2 = ((DbFunctionCommandTree)commandTree).EdmFunction;
		}
		FunctionParameter val3 = default(FunctionParameter);
		foreach (KeyValuePair<string, TypeUsage> parameter in commandTree.Parameters)
		{
			SqlParameter val4;
			if (val2 != null && val2.Parameters.TryGetValue(parameter.Key, false, ref val3))
			{
				val4 = CreateSqlParameter(val3.Name, val3.TypeUsage, val3.Mode, DBNull.Value, preventTruncation: false, sqlVersion);
			}
			else
			{
				TypeUsage type = ((paramsToForceNonUnicode != null && paramsToForceNonUnicode.Contains(parameter.Key)) ? parameter.Value.ForceNonUnicode() : parameter.Value);
				val4 = CreateSqlParameter(parameter.Key, type, (ParameterMode)0, DBNull.Value, preventTruncation: false, sqlVersion);
			}
			val.Parameters.Add(val4);
		}
		if (parameters != null && 0 < parameters.Count)
		{
			if ((int)commandTree.CommandTreeKind != 3 && (int)commandTree.CommandTreeKind != 2 && (int)commandTree.CommandTreeKind != 1)
			{
				throw new InvalidOperationException(Strings.ADP_InternalProviderError(1017));
			}
			foreach (SqlParameter item in parameters)
			{
				val.Parameters.Add(item);
			}
		}
		return (DbCommand)(object)val;
	}

	protected override void SetDbParameterValue(DbParameter parameter, TypeUsage parameterType, object value)
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		Check.NotNull(parameter, "parameter");
		Check.NotNull<TypeUsage>(parameterType, "parameterType");
		value = EnsureSqlParameterValue(value);
		if (parameterType.IsPrimitiveType((PrimitiveTypeKind)12) || parameterType.IsPrimitiveType((PrimitiveTypeKind)0))
		{
			if (!GetParameterSize(parameterType, (parameter.Direction & ParameterDirection.Output) == ParameterDirection.Output).HasValue)
			{
				int size = parameter.Size;
				parameter.Size = 0;
				parameter.Value = value;
				if (size > -1)
				{
					if (parameter.Size < size)
					{
						parameter.Size = size;
					}
					return;
				}
				int nonMaxLength = GetNonMaxLength(((SqlParameter)parameter).SqlDbType);
				if (parameter.Size < nonMaxLength)
				{
					parameter.Size = nonMaxLength;
				}
				else if (parameter.Size > nonMaxLength)
				{
					parameter.Size = -1;
				}
			}
			else
			{
				parameter.Value = value;
			}
		}
		else
		{
			parameter.Value = value;
		}
	}

	protected override string GetDbProviderManifestToken(DbConnection connection)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		Check.NotNull(connection, "connection");
		if (string.IsNullOrEmpty(DbInterception.Dispatch.Connection.GetConnectionString(connection, new DbInterceptionContext())))
		{
			throw new ArgumentException(Strings.UnableToDetermineStoreVersion);
		}
		string providerManifestToken = null;
		try
		{
			UsingConnection(connection, delegate(DbConnection conn)
			{
				providerManifestToken = QueryForManifestToken(conn);
			});
			return providerManifestToken;
		}
		catch
		{
		}
		try
		{
			UsingMasterConnection(connection, delegate(DbConnection conn)
			{
				providerManifestToken = QueryForManifestToken(conn);
			});
			return providerManifestToken;
		}
		catch
		{
		}
		return "2008";
	}

	private static string QueryForManifestToken(DbConnection conn)
	{
		SqlVersion sqlVersion = SqlVersionUtils.GetSqlVersion(conn);
		ServerType serverType = ((sqlVersion >= SqlVersion.Sql11) ? SqlVersionUtils.GetServerType(conn) : ServerType.OnPremises);
		return SqlVersionUtils.GetVersionHint(sqlVersion, serverType);
	}

	protected override DbProviderManifest GetDbProviderManifest(string versionHint)
	{
		if (string.IsNullOrEmpty(versionHint))
		{
			throw new ArgumentException(Strings.UnableToDetermineStoreVersion);
		}
		return (DbProviderManifest)(object)_providerManifests.GetOrAdd(versionHint, (string s) => new SqlProviderManifest(s));
	}

	protected override DbSpatialDataReader GetDbSpatialDataReader(DbDataReader fromReader, string versionHint)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		SqlDataReader val = (SqlDataReader)(object)((fromReader is SqlDataReader) ? fromReader : null);
		if (val == null)
		{
			throw new ProviderIncompatibleException(Strings.SqlProvider_NeedSqlDataReader(fromReader.GetType()));
		}
		if (!SupportsSpatial(versionHint))
		{
			return null;
		}
		return (DbSpatialDataReader)(object)new SqlSpatialDataReader(((DbProviderServices)this).GetSpatialServices(new DbProviderInfo("System.Data.SqlClient", versionHint)), new SqlDataReaderWrapper(val));
	}

	[Obsolete("Return DbSpatialServices from the GetService method. See http://go.microsoft.com/fwlink/?LinkId=260882 for more information.")]
	protected override DbSpatialServices DbGetSpatialServices(string versionHint)
	{
		if (!SupportsSpatial(versionHint))
		{
			return null;
		}
		return (DbSpatialServices)(object)SqlSpatialServices.Instance;
	}

	private static bool SupportsSpatial(string versionHint)
	{
		if (string.IsNullOrEmpty(versionHint))
		{
			throw new ArgumentException(Strings.UnableToDetermineStoreVersion);
		}
		return SqlVersionUtils.GetSqlVersion(versionHint) >= SqlVersion.Sql10;
	}

	internal static SqlParameter CreateSqlParameter(string name, TypeUsage type, ParameterMode mode, object value, bool preventTruncation, SqlVersion version)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Invalid comparison between Unknown and I4
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Invalid comparison between Unknown and I4
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		value = EnsureSqlParameterValue(value);
		SqlParameter val = new SqlParameter(name, value);
		ParameterDirection parameterDirection = ParameterModeToParameterDirection(mode);
		if (((DbParameter)(object)val).Direction != parameterDirection)
		{
			((DbParameter)(object)val).Direction = parameterDirection;
		}
		bool flag = (int)mode > 0;
		int? size;
		byte? precision;
		byte? scale;
		string udtName;
		SqlDbType sqlDbType = GetSqlDbType(type, flag, version, out size, out precision, out scale, out udtName);
		if (val.SqlDbType != sqlDbType)
		{
			val.SqlDbType = sqlDbType;
		}
		if (sqlDbType == SqlDbType.Udt)
		{
			val.UdtTypeName = udtName;
		}
		if (size.HasValue)
		{
			if (flag || ((DbParameter)(object)val).Size != size.Value)
			{
				if (preventTruncation && size.Value != -1)
				{
					((DbParameter)(object)val).Size = Math.Max(((DbParameter)(object)val).Size, size.Value);
				}
				else
				{
					((DbParameter)(object)val).Size = size.Value;
				}
			}
		}
		else
		{
			PrimitiveTypeKind primitiveTypeKind = ((PrimitiveType)type.EdmType).PrimitiveTypeKind;
			if ((int)primitiveTypeKind == 12)
			{
				((DbParameter)(object)val).Size = GetDefaultStringMaxLength(version, sqlDbType);
			}
			else if ((int)primitiveTypeKind == 0)
			{
				((DbParameter)(object)val).Size = GetDefaultBinaryMaxLength(version);
			}
		}
		if (precision.HasValue && (flag || (val.Precision != precision.Value && _truncateDecimalsToScale)))
		{
			val.Precision = precision.Value;
		}
		if (scale.HasValue && (flag || (val.Scale != scale.Value && _truncateDecimalsToScale)))
		{
			val.Scale = scale.Value;
		}
		bool flag2 = type.IsNullable();
		if (flag || flag2 != ((DbParameter)(object)val).IsNullable)
		{
			((DbParameter)(object)val).IsNullable = flag2;
		}
		return val;
	}

	private static ParameterDirection ParameterModeToParameterDirection(ParameterMode mode)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected I4, but got Unknown
		return (int)mode switch
		{
			0 => ParameterDirection.Input, 
			2 => ParameterDirection.InputOutput, 
			1 => ParameterDirection.Output, 
			3 => ParameterDirection.ReturnValue, 
			_ => (ParameterDirection)0, 
		};
	}

	internal static object EnsureSqlParameterValue(object value)
	{
		if (value != null && value != DBNull.Value && value.GetType().IsClass())
		{
			DbGeography val = (DbGeography)((value is DbGeography) ? value : null);
			if (val != null)
			{
				value = SqlTypesAssemblyLoader.DefaultInstance.GetSqlTypesAssembly().ConvertToSqlTypesGeography(val);
			}
			else
			{
				DbGeometry val2 = (DbGeometry)((value is DbGeometry) ? value : null);
				if (val2 != null)
				{
					value = SqlTypesAssemblyLoader.DefaultInstance.GetSqlTypesAssembly().ConvertToSqlTypesGeometry(val2);
				}
				else
				{
					HierarchyId val3 = (HierarchyId)((value is HierarchyId) ? value : null);
					if (val3 != (HierarchyId)null)
					{
						value = SqlTypesAssemblyLoader.DefaultInstance.GetSqlTypesAssembly().ConvertToSqlTypesHierarchyId(val3);
					}
				}
			}
		}
		return value;
	}

	private static SqlDbType GetSqlDbType(TypeUsage type, bool isOutParam, SqlVersion version, out int? size, out byte? precision, out byte? scale, out string udtName)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Expected I4, but got Unknown
		PrimitiveTypeKind primitiveTypeKind = ((PrimitiveType)type.EdmType).PrimitiveTypeKind;
		size = null;
		precision = null;
		scale = null;
		udtName = null;
		switch ((int)primitiveTypeKind)
		{
		case 0:
			size = GetParameterSize(type, isOutParam);
			return GetBinaryDbType(type);
		case 1:
			return SqlDbType.Bit;
		case 2:
			return SqlDbType.TinyInt;
		case 13:
			if (!SqlVersionUtils.IsPreKatmai(version))
			{
				precision = GetKatmaiDateTimePrecision(type, isOutParam);
			}
			return SqlDbType.Time;
		case 14:
			if (!SqlVersionUtils.IsPreKatmai(version))
			{
				precision = GetKatmaiDateTimePrecision(type, isOutParam);
			}
			return SqlDbType.DateTimeOffset;
		case 3:
			if (!SqlVersionUtils.IsPreKatmai(version))
			{
				precision = GetKatmaiDateTimePrecision(type, isOutParam);
				return SqlDbType.DateTime2;
			}
			return SqlDbType.DateTime;
		case 4:
			precision = GetParameterPrecision(type, null);
			scale = GetScale(type);
			return SqlDbType.Decimal;
		case 5:
			return SqlDbType.Float;
		case 16:
			udtName = "geography";
			return SqlDbType.Udt;
		case 15:
			udtName = "geometry";
			return SqlDbType.Udt;
		case 6:
			return SqlDbType.UniqueIdentifier;
		case 31:
			udtName = "hierarchyid";
			return SqlDbType.Udt;
		case 9:
			return SqlDbType.SmallInt;
		case 10:
			return SqlDbType.Int;
		case 11:
			return SqlDbType.BigInt;
		case 8:
			return SqlDbType.SmallInt;
		case 7:
			return SqlDbType.Real;
		case 12:
			size = GetParameterSize(type, isOutParam);
			return GetStringDbType(type);
		default:
			return SqlDbType.Variant;
		}
	}

	private static int? GetParameterSize(TypeUsage type, bool isOutParam)
	{
		Facet val = default(Facet);
		if (type.Facets.TryGetValue("MaxLength", false, ref val) && val.Value != null)
		{
			if (val.IsUnbounded)
			{
				return -1;
			}
			return (int?)val.Value;
		}
		if (isOutParam)
		{
			return -1;
		}
		return null;
	}

	private static int GetNonMaxLength(SqlDbType type)
	{
		int result = -1;
		switch (type)
		{
		case SqlDbType.NChar:
		case SqlDbType.NVarChar:
			result = 4000;
			break;
		case SqlDbType.Binary:
		case SqlDbType.Char:
		case SqlDbType.VarBinary:
		case SqlDbType.VarChar:
			result = 8000;
			break;
		}
		return result;
	}

	private static int GetDefaultStringMaxLength(SqlVersion version, SqlDbType type)
	{
		if (version < SqlVersion.Sql9)
		{
			if (type == SqlDbType.NChar || type == SqlDbType.NVarChar)
			{
				return 4000;
			}
			return 8000;
		}
		return -1;
	}

	private static int GetDefaultBinaryMaxLength(SqlVersion version)
	{
		if (version < SqlVersion.Sql9)
		{
			return 8000;
		}
		return -1;
	}

	private static byte? GetKatmaiDateTimePrecision(TypeUsage type, bool isOutParam)
	{
		byte? defaultIfUndefined = (isOutParam ? new byte?((byte)7) : null);
		return GetParameterPrecision(type, defaultIfUndefined);
	}

	private static byte? GetParameterPrecision(TypeUsage type, byte? defaultIfUndefined)
	{
		if (type.TryGetPrecision(out var precision))
		{
			return precision;
		}
		return defaultIfUndefined;
	}

	private static byte? GetScale(TypeUsage type)
	{
		if (type.TryGetScale(out var scale))
		{
			return scale;
		}
		return null;
	}

	private static SqlDbType GetStringDbType(TypeUsage type)
	{
		if (type.EdmType.Name.ToLowerInvariant() == "xml")
		{
			return SqlDbType.Xml;
		}
		if (!type.TryGetIsUnicode(out var isUnicode))
		{
			isUnicode = true;
		}
		if (type.IsFixedLength())
		{
			return isUnicode ? SqlDbType.NChar : SqlDbType.Char;
		}
		return isUnicode ? SqlDbType.NVarChar : SqlDbType.VarChar;
	}

	private static SqlDbType GetBinaryDbType(TypeUsage type)
	{
		if (!type.IsFixedLength())
		{
			return SqlDbType.VarBinary;
		}
		return SqlDbType.Binary;
	}

	protected override string DbCreateDatabaseScript(string providerManifestToken, StoreItemCollection storeItemCollection)
	{
		Check.NotNull(providerManifestToken, "providerManifestToken");
		Check.NotNull<StoreItemCollection>(storeItemCollection, "storeItemCollection");
		return CreateObjectsScript(SqlVersionUtils.GetSqlVersion(providerManifestToken), storeItemCollection);
	}

	protected override void DbCreateDatabase(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
	{
		Check.NotNull(connection, "connection");
		Check.NotNull<StoreItemCollection>(storeItemCollection, "storeItemCollection");
		SqlConnection requiredSqlConnection = SqlProviderUtilities.GetRequiredSqlConnection(connection);
		GetOrGenerateDatabaseNameAndGetFileNames(requiredSqlConnection, out var databaseName, out var dataFileName, out var logFileName);
		string createDatabaseScript = SqlDdlBuilder.CreateDatabaseScript(databaseName, dataFileName, logFileName);
		SqlVersion sqlVersion = CreateDatabaseFromScript(commandTimeout, (DbConnection)(object)requiredSqlConnection, createDatabaseScript);
		try
		{
			SqlConnection.ClearPool(requiredSqlConnection);
			string setDatabaseOptionsScript = SqlDdlBuilder.SetDatabaseOptionsScript(sqlVersion, databaseName);
			if (!string.IsNullOrEmpty(setDatabaseOptionsScript))
			{
				UsingMasterConnection((DbConnection)(object)requiredSqlConnection, delegate(DbConnection conn)
				{
					//IL_001e: Unknown result type (might be due to invalid IL or missing references)
					//IL_0028: Expected O, but got Unknown
					using DbCommand dbCommand2 = CreateCommand(conn, setDatabaseOptionsScript, commandTimeout);
					DbInterception.Dispatch.Command.NonQuery(dbCommand2, new DbCommandInterceptionContext());
				});
			}
			string createObjectsScript = CreateObjectsScript(sqlVersion, storeItemCollection);
			if (string.IsNullOrWhiteSpace(createObjectsScript))
			{
				return;
			}
			UsingConnection((DbConnection)(object)requiredSqlConnection, delegate(DbConnection conn)
			{
				//IL_001e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0028: Expected O, but got Unknown
				using DbCommand dbCommand = CreateCommand(conn, createObjectsScript, commandTimeout);
				DbInterception.Dispatch.Command.NonQuery(dbCommand, new DbCommandInterceptionContext());
			});
		}
		catch (Exception ex)
		{
			try
			{
				DropDatabase(requiredSqlConnection, commandTimeout, databaseName);
			}
			catch (Exception ex2)
			{
				throw new InvalidOperationException(Strings.SqlProvider_IncompleteCreateDatabase, new AggregateException(Strings.SqlProvider_IncompleteCreateDatabaseAggregate, ex, ex2));
			}
			throw;
		}
	}

	private static void GetOrGenerateDatabaseNameAndGetFileNames(SqlConnection sqlConnection, out string databaseName, out string dataFileName, out string logFileName)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		SqlConnectionStringBuilder val = new SqlConnectionStringBuilder(DbInterception.Dispatch.Connection.GetConnectionString((DbConnection)(object)sqlConnection, new DbInterceptionContext()));
		string attachDBFilename = val.AttachDBFilename;
		if (string.IsNullOrEmpty(attachDBFilename))
		{
			dataFileName = null;
			logFileName = null;
		}
		else
		{
			dataFileName = GetMdfFileName(attachDBFilename);
			logFileName = GetLdfFileName(dataFileName);
		}
		if (!string.IsNullOrEmpty(val.InitialCatalog))
		{
			databaseName = val.InitialCatalog;
			return;
		}
		if (dataFileName != null)
		{
			databaseName = GenerateDatabaseName(dataFileName);
			return;
		}
		throw new InvalidOperationException(Strings.SqlProvider_DdlGeneration_MissingInitialCatalog);
	}

	private static string GetLdfFileName(string dataFileName)
	{
		return Path.Combine(new FileInfo(dataFileName).Directory.FullName, Path.GetFileNameWithoutExtension(dataFileName) + "_log.ldf");
	}

	private static string GenerateDatabaseName(string mdfFileName)
	{
		char[] array = Path.GetFileNameWithoutExtension(mdfFileName.ToUpper(CultureInfo.InvariantCulture)).ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (!char.IsLetterOrDigit(array[i]))
			{
				array[i] = '_';
			}
		}
		string text = new string(array);
		text = ((text.Length > 30) ? text.Substring(0, 30) : text);
		return string.Format(CultureInfo.InvariantCulture, "{0}_{1}", new object[2]
		{
			text,
			Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture)
		});
	}

	private static string GetMdfFileName(string attachDBFile)
	{
		return DbProviderServices.ExpandDataDirectory(attachDBFile);
	}

	internal SqlVersion CreateDatabaseFromScript(int? commandTimeout, DbConnection sqlConnection, string createDatabaseScript)
	{
		SqlVersion sqlVersion = (SqlVersion)0;
		UsingMasterConnection(sqlConnection, delegate(DbConnection conn)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Expected O, but got Unknown
			using (DbCommand dbCommand = CreateCommand(conn, createDatabaseScript, commandTimeout))
			{
				DbInterception.Dispatch.Command.NonQuery(dbCommand, new DbCommandInterceptionContext());
			}
			sqlVersion = SqlVersionUtils.GetSqlVersion(conn);
		});
		return sqlVersion;
	}

	protected override bool DbDatabaseExists(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
	{
		return ((DbProviderServices)this).DbDatabaseExists(connection, commandTimeout, new Lazy<StoreItemCollection>((Func<StoreItemCollection>)(() => storeItemCollection)));
	}

	protected override bool DbDatabaseExists(DbConnection connection, int? commandTimeout, Lazy<StoreItemCollection> storeItemCollection)
	{
		//IL_00d6: Expected O, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		Check.NotNull(connection, "connection");
		Check.NotNull(storeItemCollection, "storeItemCollection");
		if (connection.State == ConnectionState.Open)
		{
			return true;
		}
		SqlConnection requiredSqlConnection = SqlProviderUtilities.GetRequiredSqlConnection(connection);
		SqlConnectionStringBuilder val = new SqlConnectionStringBuilder(DbInterception.Dispatch.Connection.GetConnectionString((DbConnection)(object)requiredSqlConnection, new DbInterceptionContext()));
		if (string.IsNullOrEmpty(val.InitialCatalog) && string.IsNullOrEmpty(val.AttachDBFilename))
		{
			throw new InvalidOperationException(Strings.SqlProvider_DdlGeneration_MissingInitialCatalog);
		}
		if (!string.IsNullOrEmpty(val.InitialCatalog) && CheckDatabaseExists(requiredSqlConnection, commandTimeout, val.InitialCatalog))
		{
			return true;
		}
		if (!string.IsNullOrEmpty(val.AttachDBFilename))
		{
			try
			{
				UsingConnection((DbConnection)(object)requiredSqlConnection, delegate
				{
				});
				return true;
			}
			catch (SqlException val2)
			{
				SqlException innerException = val2;
				if (!string.IsNullOrEmpty(val.InitialCatalog))
				{
					return CheckDatabaseExists(requiredSqlConnection, commandTimeout, val.InitialCatalog);
				}
				string fileName = GetMdfFileName(val.AttachDBFilename);
				bool databaseDoesNotExistInSysTables = false;
				UsingMasterConnection((DbConnection)(object)requiredSqlConnection, delegate(DbConnection conn)
				{
					//IL_0031: Unknown result type (might be due to invalid IL or missing references)
					//IL_003b: Expected O, but got Unknown
					SqlVersion sqlVersion = SqlVersionUtils.GetSqlVersion(conn);
					string commandText = SqlDdlBuilder.CreateCountDatabasesBasedOnFileNameScript(fileName, sqlVersion == SqlVersion.Sql8);
					using DbCommand dbCommand = CreateCommand(conn, commandText, commandTimeout);
					int num = (int)DbInterception.Dispatch.Command.Scalar(dbCommand, new DbCommandInterceptionContext());
					databaseDoesNotExistInSysTables = num == 0;
				});
				if (!databaseDoesNotExistInSysTables)
				{
					throw new InvalidOperationException(Strings.SqlProvider_DdlGeneration_CannotTellIfDatabaseExists, (Exception?)(object)innerException);
				}
				return false;
			}
		}
		return false;
	}

	private bool CheckDatabaseExists(SqlConnection sqlConnection, int? commandTimeout, string databaseName)
	{
		bool databaseExists = false;
		UsingMasterConnection((DbConnection)(object)sqlConnection, delegate(DbConnection conn)
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Expected O, but got Unknown
			string commandText = SqlDdlBuilder.CreateDatabaseExistsScript(databaseName);
			using DbCommand dbCommand = CreateCommand(conn, commandText, commandTimeout);
			databaseExists = (int)DbInterception.Dispatch.Command.Scalar(dbCommand, new DbCommandInterceptionContext()) >= 1;
		});
		return databaseExists;
	}

	protected override void DbDeleteDatabase(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		Check.NotNull(connection, "connection");
		Check.NotNull<StoreItemCollection>(storeItemCollection, "storeItemCollection");
		SqlConnection requiredSqlConnection = SqlProviderUtilities.GetRequiredSqlConnection(connection);
		SqlConnectionStringBuilder val = new SqlConnectionStringBuilder(DbInterception.Dispatch.Connection.GetConnectionString((DbConnection)(object)requiredSqlConnection, new DbInterceptionContext()));
		string initialCatalog = val.InitialCatalog;
		string attachDBFilename = val.AttachDBFilename;
		if (!string.IsNullOrEmpty(initialCatalog))
		{
			DropDatabase(requiredSqlConnection, commandTimeout, initialCatalog);
			return;
		}
		if (!string.IsNullOrEmpty(attachDBFilename))
		{
			string fullFileName = GetMdfFileName(attachDBFilename);
			List<string> databaseNames = new List<string>();
			UsingMasterConnection((DbConnection)(object)requiredSqlConnection, delegate(DbConnection conn)
			{
				//IL_0031: Unknown result type (might be due to invalid IL or missing references)
				//IL_003b: Expected O, but got Unknown
				SqlVersion sqlVersion = SqlVersionUtils.GetSqlVersion(conn);
				string commandText = SqlDdlBuilder.CreateGetDatabaseNamesBasedOnFileNameScript(fullFileName, sqlVersion == SqlVersion.Sql8);
				DbCommand dbCommand = CreateCommand(conn, commandText, commandTimeout);
				using DbDataReader dbDataReader = DbInterception.Dispatch.Command.Reader(dbCommand, new DbCommandInterceptionContext());
				while (dbDataReader.Read())
				{
					databaseNames.Add(dbDataReader.GetString(0));
				}
			});
			if (databaseNames.Count > 0)
			{
				foreach (string item in databaseNames)
				{
					DropDatabase(requiredSqlConnection, commandTimeout, item);
				}
				return;
			}
			throw new InvalidOperationException(Strings.SqlProvider_DdlGeneration_CannotDeleteDatabaseNoInitialCatalog);
		}
		throw new InvalidOperationException(Strings.SqlProvider_DdlGeneration_MissingInitialCatalog);
	}

	private void DropDatabase(SqlConnection sqlConnection, int? commandTimeout, string databaseName)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		SqlConnection.ClearAllPools();
		string dropDatabaseScript = SqlDdlBuilder.DropDatabaseScript(databaseName);
		try
		{
			UsingMasterConnection((DbConnection)(object)sqlConnection, delegate(DbConnection conn)
			{
				//IL_001e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0028: Expected O, but got Unknown
				using DbCommand dbCommand = CreateCommand(conn, dropDatabaseScript, commandTimeout);
				DbInterception.Dispatch.Command.NonQuery(dbCommand, new DbCommandInterceptionContext());
			});
		}
		catch (SqlException val)
		{
			foreach (SqlError error in val.Errors)
			{
				if (error.Number == 5120)
				{
					return;
				}
			}
			throw;
		}
	}

	private static string CreateObjectsScript(SqlVersion version, StoreItemCollection storeItemCollection)
	{
		return SqlDdlBuilder.CreateObjectsScript(storeItemCollection, version != SqlVersion.Sql8);
	}

	private static DbCommand CreateCommand(DbConnection sqlConnection, string commandText, int? commandTimeout)
	{
		if (string.IsNullOrEmpty(commandText))
		{
			commandText = Environment.NewLine;
		}
		DbCommand dbCommand = sqlConnection.CreateCommand();
		dbCommand.CommandText = commandText;
		if (commandTimeout.HasValue)
		{
			dbCommand.CommandTimeout = commandTimeout.Value;
		}
		return dbCommand;
	}

	private static void UsingConnection(DbConnection sqlConnection, Action<DbConnection> act)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		DbInterceptionContext interceptionContext = new DbInterceptionContext();
		string holdConnectionString = DbInterception.Dispatch.Connection.GetConnectionString(sqlConnection, interceptionContext);
		DbProviderServices.GetExecutionStrategy(sqlConnection, "System.Data.SqlClient").Execute((Action)delegate
		{
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Expected O, but got Unknown
			bool flag = DbInterception.Dispatch.Connection.GetState(sqlConnection, interceptionContext) == ConnectionState.Closed;
			if (flag)
			{
				if (DbInterception.Dispatch.Connection.GetState(sqlConnection, new DbInterceptionContext()) == ConnectionState.Closed && !DbInterception.Dispatch.Connection.GetConnectionString(sqlConnection, interceptionContext).Equals(holdConnectionString, StringComparison.Ordinal))
				{
					DbInterception.Dispatch.Connection.SetConnectionString(sqlConnection, new DbConnectionPropertyInterceptionContext<string>().WithValue(holdConnectionString));
				}
				DbInterception.Dispatch.Connection.Open(sqlConnection, interceptionContext);
			}
			try
			{
				act(sqlConnection);
			}
			finally
			{
				if (flag && DbInterception.Dispatch.Connection.GetState(sqlConnection, interceptionContext) == ConnectionState.Open)
				{
					DbInterception.Dispatch.Connection.Close(sqlConnection, interceptionContext);
					if (!DbInterception.Dispatch.Connection.GetConnectionString(sqlConnection, interceptionContext).Equals(holdConnectionString, StringComparison.Ordinal))
					{
						DbInterception.Dispatch.Connection.SetConnectionString(sqlConnection, new DbConnectionPropertyInterceptionContext<string>().WithValue(holdConnectionString));
					}
				}
			}
		});
	}

	private void UsingMasterConnection(DbConnection sqlConnection, Action<DbConnection> act)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		//IL_006f: Expected O, but got Unknown
		SqlConnectionStringBuilder val = new SqlConnectionStringBuilder(DbInterception.Dispatch.Connection.GetConnectionString(sqlConnection, new DbInterceptionContext()))
		{
			InitialCatalog = "master",
			AttachDBFilename = string.Empty
		};
		try
		{
			using DbConnection dbConnection = ((DbProviderServices)this).CloneDbConnection(sqlConnection);
			DbInterception.Dispatch.Connection.SetConnectionString(dbConnection, new DbConnectionPropertyInterceptionContext<string>().WithValue(((DbConnectionStringBuilder)(object)val).ConnectionString));
			UsingConnection(dbConnection, act);
		}
		catch (SqlException val2)
		{
			SqlException innerException = val2;
			if (!val.IntegratedSecurity && (string.IsNullOrEmpty(val.UserID) || string.IsNullOrEmpty(val.Password)))
			{
				throw new InvalidOperationException(Strings.SqlProvider_CredentialsMissingForMasterConnection, (Exception?)(object)innerException);
			}
			throw;
		}
	}

	public override DbConnection CloneDbConnection(DbConnection connection, DbProviderFactory factory)
	{
		if (connection is ICloneable cloneable)
		{
			return (DbConnection)cloneable.Clone();
		}
		return ((DbProviderServices)this).CloneDbConnection(connection, factory);
	}
}
