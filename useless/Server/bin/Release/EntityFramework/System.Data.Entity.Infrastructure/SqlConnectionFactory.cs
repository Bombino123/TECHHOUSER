using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.Internal;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Data.SqlClient;

namespace System.Data.Entity.Infrastructure;

public sealed class SqlConnectionFactory : IDbConnectionFactory
{
	private readonly string _baseConnectionString;

	private Func<string, DbProviderFactory> _providerFactoryCreator;

	internal Func<string, DbProviderFactory> ProviderFactory
	{
		get
		{
			return _providerFactoryCreator ?? new Func<string, DbProviderFactory>(DbConfiguration.DependencyResolver.GetService<DbProviderFactory>);
		}
		set
		{
			_providerFactoryCreator = value;
		}
	}

	public string BaseConnectionString => _baseConnectionString;

	public SqlConnectionFactory()
	{
		_baseConnectionString = "Data Source=.\\SQLEXPRESS; Integrated Security=True; MultipleActiveResultSets=True;";
	}

	public SqlConnectionFactory(string baseConnectionString)
	{
		Check.NotNull(baseConnectionString, "baseConnectionString");
		_baseConnectionString = baseConnectionString;
	}

	public DbConnection CreateConnection(string nameOrConnectionString)
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		Check.NotEmpty(nameOrConnectionString, "nameOrConnectionString");
		string value = nameOrConnectionString;
		if (!DbHelpers.TreatAsConnectionString(nameOrConnectionString))
		{
			if (nameOrConnectionString.EndsWith(".mdf", ignoreCase: true, null))
			{
				throw Error.SqlConnectionFactory_MdfNotSupported(nameOrConnectionString);
			}
			value = ((DbConnectionStringBuilder)new SqlConnectionStringBuilder(BaseConnectionString)
			{
				InitialCatalog = nameOrConnectionString
			}).ConnectionString;
		}
		DbConnection dbConnection = null;
		try
		{
			dbConnection = ProviderFactory("System.Data.SqlClient").CreateConnection();
			DbInterception.Dispatch.Connection.SetConnectionString(dbConnection, new DbConnectionPropertyInterceptionContext<string>().WithValue(value));
		}
		catch
		{
			dbConnection = (DbConnection)new SqlConnection();
			DbInterception.Dispatch.Connection.SetConnectionString(dbConnection, new DbConnectionPropertyInterceptionContext<string>().WithValue(value));
		}
		return dbConnection;
	}
}
