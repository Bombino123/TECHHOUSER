using System.Data.Common;
using System.Data.Entity.SqlServer.Resources;
using System.Data.SqlClient;

namespace System.Data.Entity.SqlServer;

internal class SqlProviderUtilities
{
	internal static SqlConnection GetRequiredSqlConnection(DbConnection connection)
	{
		SqlConnection val = (SqlConnection)(object)((connection is SqlConnection) ? connection : null);
		if (val == null)
		{
			throw new ArgumentException(Strings.Mapping_Provider_WrongConnectionType(typeof(SqlConnection)));
		}
		return val;
	}
}
