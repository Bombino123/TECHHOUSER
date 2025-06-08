using System.Data.Common;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.Internal;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.IO;

namespace System.Data.Entity.Infrastructure;

public sealed class SqlCeConnectionFactory : IDbConnectionFactory
{
	private readonly string _databaseDirectory;

	private readonly string _baseConnectionString;

	private readonly string _providerInvariantName;

	public string DatabaseDirectory => _databaseDirectory;

	public string BaseConnectionString => _baseConnectionString;

	public string ProviderInvariantName => _providerInvariantName;

	public SqlCeConnectionFactory(string providerInvariantName)
	{
		Check.NotEmpty(providerInvariantName, "providerInvariantName");
		_providerInvariantName = providerInvariantName;
		_databaseDirectory = "|DataDirectory|";
		_baseConnectionString = "";
	}

	public SqlCeConnectionFactory(string providerInvariantName, string databaseDirectory, string baseConnectionString)
	{
		Check.NotEmpty(providerInvariantName, "providerInvariantName");
		Check.NotNull(databaseDirectory, "databaseDirectory");
		Check.NotNull(baseConnectionString, "baseConnectionString");
		_providerInvariantName = providerInvariantName;
		_databaseDirectory = databaseDirectory;
		_baseConnectionString = baseConnectionString;
	}

	public DbConnection CreateConnection(string nameOrConnectionString)
	{
		Check.NotEmpty(nameOrConnectionString, "nameOrConnectionString");
		DbConnection dbConnection = DbConfiguration.DependencyResolver.GetService<DbProviderFactory>(ProviderInvariantName).CreateConnection();
		if (dbConnection == null)
		{
			throw Error.DbContext_ProviderReturnedNullConnection();
		}
		string value;
		if (DbHelpers.TreatAsConnectionString(nameOrConnectionString))
		{
			value = nameOrConnectionString;
		}
		else
		{
			if (!nameOrConnectionString.EndsWith(".sdf", ignoreCase: true, null))
			{
				nameOrConnectionString += ".sdf";
			}
			string text = ((DatabaseDirectory.StartsWith("|", StringComparison.Ordinal) && DatabaseDirectory.EndsWith("|", StringComparison.Ordinal)) ? (DatabaseDirectory + nameOrConnectionString) : Path.Combine(DatabaseDirectory, nameOrConnectionString));
			value = string.Format(CultureInfo.InvariantCulture, "Data Source={0}; {1}", new object[2] { text, BaseConnectionString });
		}
		DbInterception.Dispatch.Connection.SetConnectionString(dbConnection, new DbConnectionPropertyInterceptionContext<string>().WithValue(value));
		return dbConnection;
	}
}
