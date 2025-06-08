using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.SqlServer.Resources;
using System.Globalization;

namespace System.Data.Entity.SqlServer;

internal static class SqlVersionUtils
{
	internal static SqlVersion GetSqlVersion(DbConnection connection)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		int num = int.Parse(DbInterception.Dispatch.Connection.GetServerVersion(connection, new DbInterceptionContext()).Substring(0, 2), CultureInfo.InvariantCulture);
		if (num >= 11)
		{
			return SqlVersion.Sql11;
		}
		return num switch
		{
			10 => SqlVersion.Sql10, 
			9 => SqlVersion.Sql9, 
			_ => SqlVersion.Sql8, 
		};
	}

	internal static ServerType GetServerType(DbConnection connection)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		using DbCommand dbCommand = connection.CreateCommand();
		dbCommand.CommandText = "select cast(serverproperty('EngineEdition') as int)";
		using DbDataReader dbDataReader = DbInterception.Dispatch.Command.Reader(dbCommand, new DbCommandInterceptionContext());
		dbDataReader.Read();
		return (dbDataReader.GetInt32(0) == 5) ? ServerType.Cloud : ServerType.OnPremises;
	}

	internal static string GetVersionHint(SqlVersion version, ServerType serverType)
	{
		if (serverType == ServerType.Cloud)
		{
			return "2012.Azure";
		}
		return version switch
		{
			SqlVersion.Sql8 => "2000", 
			SqlVersion.Sql9 => "2005", 
			SqlVersion.Sql10 => "2008", 
			SqlVersion.Sql11 => "2012", 
			_ => throw new ArgumentException(Strings.UnableToDetermineStoreVersion), 
		};
	}

	internal static SqlVersion GetSqlVersion(string versionHint)
	{
		if (!string.IsNullOrEmpty(versionHint))
		{
			switch (versionHint)
			{
			case "2000":
				return SqlVersion.Sql8;
			case "2005":
				return SqlVersion.Sql9;
			case "2008":
				return SqlVersion.Sql10;
			case "2012":
				return SqlVersion.Sql11;
			case "2012.Azure":
				return SqlVersion.Sql11;
			}
		}
		throw new ArgumentException(Strings.UnableToDetermineStoreVersion);
	}

	internal static bool IsPreKatmai(SqlVersion sqlVersion)
	{
		if (sqlVersion != SqlVersion.Sql8)
		{
			return sqlVersion == SqlVersion.Sql9;
		}
		return true;
	}
}
