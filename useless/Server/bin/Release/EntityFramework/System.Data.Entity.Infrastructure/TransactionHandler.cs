using System.Data.Common;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Infrastructure;

public abstract class TransactionHandler : IDbTransactionInterceptor, IDbInterceptor, IDbConnectionInterceptor, IDisposable
{
	private WeakReference _objectContext;

	private WeakReference _dbContext;

	private WeakReference _connection;

	public ObjectContext ObjectContext
	{
		get
		{
			if (_objectContext == null || !_objectContext.IsAlive)
			{
				return null;
			}
			return (ObjectContext)_objectContext.Target;
		}
		private set
		{
			_objectContext = new WeakReference(value);
		}
	}

	public DbContext DbContext
	{
		get
		{
			if (_dbContext == null || !_dbContext.IsAlive)
			{
				return null;
			}
			return (DbContext)_dbContext.Target;
		}
		private set
		{
			_dbContext = new WeakReference(value);
		}
	}

	public DbConnection Connection
	{
		get
		{
			if (_connection == null || !_connection.IsAlive)
			{
				return null;
			}
			return (DbConnection)_connection.Target;
		}
		private set
		{
			_connection = new WeakReference(value);
		}
	}

	protected bool IsDisposed { get; set; }

	protected TransactionHandler()
	{
		DbInterception.Add(this);
	}

	public virtual void Initialize(ObjectContext context)
	{
		Check.NotNull(context, "context");
		if (ObjectContext != null || DbContext != null || Connection != null)
		{
			throw new InvalidOperationException(Strings.TransactionHandler_AlreadyInitialized);
		}
		ObjectContext = context;
		DbContext = context.InterceptionContext.DbContexts.FirstOrDefault();
		Connection = ((EntityConnection)ObjectContext.Connection).StoreConnection;
	}

	public virtual void Initialize(DbContext context, DbConnection connection)
	{
		Check.NotNull(context, "context");
		Check.NotNull(connection, "connection");
		if (ObjectContext != null || DbContext != null || Connection != null)
		{
			throw new InvalidOperationException(Strings.TransactionHandler_AlreadyInitialized);
		}
		DbContext = context;
		Connection = connection;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!IsDisposed)
		{
			DbInterception.Remove(this);
			IsDisposed = true;
		}
	}

	protected internal virtual bool MatchesParentContext(DbConnection connection, DbInterceptionContext interceptionContext)
	{
		Check.NotNull(connection, "connection");
		Check.NotNull(interceptionContext, "interceptionContext");
		if (DbContext != null && interceptionContext.DbContexts.Contains(DbContext, object.ReferenceEquals))
		{
			return true;
		}
		if (ObjectContext != null && interceptionContext.ObjectContexts.Contains(ObjectContext, object.ReferenceEquals))
		{
			return true;
		}
		if (Connection != null && !interceptionContext.ObjectContexts.Any() && !interceptionContext.DbContexts.Any())
		{
			return connection == Connection;
		}
		return false;
	}

	public abstract string BuildDatabaseInitializationScript();

	public virtual void BeginningTransaction(DbConnection connection, BeginTransactionInterceptionContext interceptionContext)
	{
	}

	public virtual void BeganTransaction(DbConnection connection, BeginTransactionInterceptionContext interceptionContext)
	{
	}

	public virtual void Closing(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
	{
	}

	public virtual void Closed(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
	{
	}

	public virtual void ConnectionStringGetting(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
	{
	}

	public virtual void ConnectionStringGot(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
	{
	}

	public virtual void ConnectionStringSetting(DbConnection connection, DbConnectionPropertyInterceptionContext<string> interceptionContext)
	{
	}

	public virtual void ConnectionStringSet(DbConnection connection, DbConnectionPropertyInterceptionContext<string> interceptionContext)
	{
	}

	public virtual void ConnectionTimeoutGetting(DbConnection connection, DbConnectionInterceptionContext<int> interceptionContext)
	{
	}

	public virtual void ConnectionTimeoutGot(DbConnection connection, DbConnectionInterceptionContext<int> interceptionContext)
	{
	}

	public virtual void DatabaseGetting(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
	{
	}

	public virtual void DatabaseGot(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
	{
	}

	public virtual void DataSourceGetting(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
	{
	}

	public virtual void DataSourceGot(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
	{
	}

	public virtual void Disposing(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
	{
	}

	public virtual void Disposed(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
	{
	}

	public virtual void EnlistingTransaction(DbConnection connection, EnlistTransactionInterceptionContext interceptionContext)
	{
	}

	public virtual void EnlistedTransaction(DbConnection connection, EnlistTransactionInterceptionContext interceptionContext)
	{
	}

	public virtual void Opening(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
	{
	}

	public virtual void Opened(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
	{
	}

	public virtual void ServerVersionGetting(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
	{
	}

	public virtual void ServerVersionGot(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
	{
	}

	public virtual void StateGetting(DbConnection connection, DbConnectionInterceptionContext<ConnectionState> interceptionContext)
	{
	}

	public virtual void StateGot(DbConnection connection, DbConnectionInterceptionContext<ConnectionState> interceptionContext)
	{
	}

	public virtual void ConnectionGetting(DbTransaction transaction, DbTransactionInterceptionContext<DbConnection> interceptionContext)
	{
	}

	public virtual void ConnectionGot(DbTransaction transaction, DbTransactionInterceptionContext<DbConnection> interceptionContext)
	{
	}

	public virtual void IsolationLevelGetting(DbTransaction transaction, DbTransactionInterceptionContext<IsolationLevel> interceptionContext)
	{
	}

	public virtual void IsolationLevelGot(DbTransaction transaction, DbTransactionInterceptionContext<IsolationLevel> interceptionContext)
	{
	}

	public virtual void Committing(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext)
	{
	}

	public virtual void Committed(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext)
	{
	}

	public virtual void Disposing(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext)
	{
	}

	public virtual void Disposed(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext)
	{
	}

	public virtual void RollingBack(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext)
	{
	}

	public virtual void RolledBack(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext)
	{
	}
}
