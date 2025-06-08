using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.EntityClient.Internal;
using System.Security;
using System.Security.Permissions;

namespace System.Data.Entity.Core.EntityClient;

public sealed class EntityProviderFactory : DbProviderFactory, IServiceProvider
{
	public static readonly EntityProviderFactory Instance = new EntityProviderFactory();

	private EntityProviderFactory()
	{
	}

	public override DbCommand CreateCommand()
	{
		return new EntityCommand();
	}

	public override DbCommandBuilder CreateCommandBuilder()
	{
		throw new NotSupportedException();
	}

	public override DbConnection CreateConnection()
	{
		return new EntityConnection();
	}

	public override DbConnectionStringBuilder CreateConnectionStringBuilder()
	{
		return new EntityConnectionStringBuilder();
	}

	public override DbDataAdapter CreateDataAdapter()
	{
		throw new NotSupportedException();
	}

	public override DbParameter CreateParameter()
	{
		return new EntityParameter();
	}

	public CodeAccessPermission CreatePermission(PermissionState state)
	{
		throw new NotSupportedException();
	}

	object IServiceProvider.GetService(Type serviceType)
	{
		if (!(serviceType == typeof(DbProviderServices)))
		{
			return null;
		}
		return EntityProviderServices.Instance;
	}
}
