using System.Data.Common;
using System.Data.Entity.Infrastructure;

namespace System.Data.Entity.Migrations.Utilities;

internal class DatabaseCreator
{
	private readonly int? _commandTimeout;

	public DatabaseCreator(int? commandTimeout)
	{
		_commandTimeout = commandTimeout;
	}

	public virtual bool Exists(DbConnection connection)
	{
		using EmptyContext emptyContext = new EmptyContext(connection);
		emptyContext.Database.CommandTimeout = _commandTimeout;
		return ((IObjectContextAdapter)emptyContext).ObjectContext.DatabaseExists();
	}

	public virtual void Create(DbConnection connection)
	{
		using EmptyContext emptyContext = new EmptyContext(connection);
		emptyContext.Database.CommandTimeout = _commandTimeout;
		((IObjectContextAdapter)emptyContext).ObjectContext.CreateDatabase();
	}

	public virtual void Delete(DbConnection connection)
	{
		using EmptyContext emptyContext = new EmptyContext(connection);
		emptyContext.Database.CommandTimeout = _commandTimeout;
		((IObjectContextAdapter)emptyContext).ObjectContext.DeleteDatabase();
	}
}
