using System.ComponentModel;
using System.Configuration;
using System.Data.Entity.Internal;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Infrastructure;

[Serializable]
public class DbConnectionInfo
{
	private readonly string _connectionName;

	private readonly string _connectionString;

	private readonly string _providerInvariantName;

	public DbConnectionInfo(string connectionName)
	{
		Check.NotEmpty(connectionName, "connectionName");
		_connectionName = connectionName;
	}

	public DbConnectionInfo(string connectionString, string providerInvariantName)
	{
		Check.NotEmpty(connectionString, "connectionString");
		Check.NotEmpty(providerInvariantName, "providerInvariantName");
		_connectionString = connectionString;
		_providerInvariantName = providerInvariantName;
	}

	internal ConnectionStringSettings GetConnectionString(AppConfig config)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		if (_connectionName != null)
		{
			return config.GetConnectionString(_connectionName) ?? throw Error.DbConnectionInfo_ConnectionStringNotFound(_connectionName);
		}
		return new ConnectionStringSettings((string)null, _connectionString, _providerInvariantName);
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
}
