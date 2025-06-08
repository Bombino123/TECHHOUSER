using System.Data.Common;
using System.Data.Entity.Utilities;
using System.Globalization;

namespace System.Data.Entity.Infrastructure;

public sealed class LocalDbConnectionFactory : IDbConnectionFactory
{
	private readonly string _baseConnectionString;

	private readonly string _localDbVersion;

	public string BaseConnectionString => _baseConnectionString;

	public LocalDbConnectionFactory()
		: this("mssqllocaldb")
	{
	}

	public LocalDbConnectionFactory(string localDbVersion)
	{
		Check.NotEmpty(localDbVersion, "localDbVersion");
		_localDbVersion = localDbVersion;
		_baseConnectionString = "Integrated Security=True; MultipleActiveResultSets=True;";
	}

	public LocalDbConnectionFactory(string localDbVersion, string baseConnectionString)
	{
		Check.NotEmpty(localDbVersion, "localDbVersion");
		Check.NotNull(baseConnectionString, "baseConnectionString");
		_localDbVersion = localDbVersion;
		_baseConnectionString = baseConnectionString;
	}

	public DbConnection CreateConnection(string nameOrConnectionString)
	{
		Check.NotEmpty(nameOrConnectionString, "nameOrConnectionString");
		string text = " ";
		if (!string.IsNullOrEmpty(AppDomain.CurrentDomain.GetData("DataDirectory") as string))
		{
			text = string.Format(CultureInfo.InvariantCulture, " AttachDbFilename=|DataDirectory|{0}.mdf; ", new object[1] { nameOrConnectionString });
		}
		return new SqlConnectionFactory(string.Format(CultureInfo.InvariantCulture, "Data Source=(localdb)\\{1};{0};{2}", new object[3] { _baseConnectionString, _localDbVersion, text })).CreateConnection(nameOrConnectionString);
	}
}
