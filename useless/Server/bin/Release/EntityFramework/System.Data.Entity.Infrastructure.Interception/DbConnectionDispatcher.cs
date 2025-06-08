using System.ComponentModel;
using System.Data.Common;
using System.Data.Entity.Utilities;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Infrastructure.Interception;

public class DbConnectionDispatcher
{
	private readonly InternalDispatcher<IDbConnectionInterceptor> _internalDispatcher = new InternalDispatcher<IDbConnectionInterceptor>();

	internal InternalDispatcher<IDbConnectionInterceptor> InternalDispatcher => _internalDispatcher;

	internal DbConnectionDispatcher()
	{
	}

	public virtual DbTransaction BeginTransaction(DbConnection connection, BeginTransactionInterceptionContext interceptionContext)
	{
		Check.NotNull(connection, "connection");
		Check.NotNull(interceptionContext, "interceptionContext");
		return InternalDispatcher.Dispatch(connection, (DbConnection t, BeginTransactionInterceptionContext c) => t.BeginTransaction(c.IsolationLevel), new BeginTransactionInterceptionContext(interceptionContext), delegate(IDbConnectionInterceptor i, DbConnection t, BeginTransactionInterceptionContext c)
		{
			i.BeginningTransaction(t, c);
		}, delegate(IDbConnectionInterceptor i, DbConnection t, BeginTransactionInterceptionContext c)
		{
			i.BeganTransaction(t, c);
		});
	}

	public virtual void Close(DbConnection connection, DbInterceptionContext interceptionContext)
	{
		Check.NotNull(connection, "connection");
		Check.NotNull(interceptionContext, "interceptionContext");
		InternalDispatcher.Dispatch(connection, delegate(DbConnection t, DbConnectionInterceptionContext c)
		{
			t.Close();
		}, new DbConnectionInterceptionContext(interceptionContext), delegate(IDbConnectionInterceptor i, DbConnection t, DbConnectionInterceptionContext c)
		{
			i.Closing(t, c);
		}, delegate(IDbConnectionInterceptor i, DbConnection t, DbConnectionInterceptionContext c)
		{
			i.Closed(t, c);
		});
	}

	public virtual void Dispose(DbConnection connection, DbInterceptionContext interceptionContext)
	{
		Check.NotNull(connection, "connection");
		Check.NotNull(interceptionContext, "interceptionContext");
		InternalDispatcher.Dispatch(connection, delegate(DbConnection t, DbConnectionInterceptionContext c)
		{
			using (t)
			{
			}
		}, new DbConnectionInterceptionContext(interceptionContext), delegate(IDbConnectionInterceptor i, DbConnection t, DbConnectionInterceptionContext c)
		{
			i.Disposing(t, c);
		}, delegate(IDbConnectionInterceptor i, DbConnection t, DbConnectionInterceptionContext c)
		{
			i.Disposed(t, c);
		});
	}

	public virtual string GetConnectionString(DbConnection connection, DbInterceptionContext interceptionContext)
	{
		Check.NotNull(connection, "connection");
		Check.NotNull(interceptionContext, "interceptionContext");
		return InternalDispatcher.Dispatch(connection, (DbConnection t, DbConnectionInterceptionContext<string> c) => t.ConnectionString, new DbConnectionInterceptionContext<string>(interceptionContext), delegate(IDbConnectionInterceptor i, DbConnection t, DbConnectionInterceptionContext<string> c)
		{
			i.ConnectionStringGetting(t, c);
		}, delegate(IDbConnectionInterceptor i, DbConnection t, DbConnectionInterceptionContext<string> c)
		{
			i.ConnectionStringGot(t, c);
		});
	}

	public virtual void SetConnectionString(DbConnection connection, DbConnectionPropertyInterceptionContext<string> interceptionContext)
	{
		Check.NotNull(connection, "connection");
		Check.NotNull(interceptionContext, "interceptionContext");
		InternalDispatcher.Dispatch(connection, delegate(DbConnection t, DbConnectionPropertyInterceptionContext<string> c)
		{
			t.ConnectionString = c.Value;
		}, new DbConnectionPropertyInterceptionContext<string>(interceptionContext), delegate(IDbConnectionInterceptor i, DbConnection t, DbConnectionPropertyInterceptionContext<string> c)
		{
			i.ConnectionStringSetting(t, c);
		}, delegate(IDbConnectionInterceptor i, DbConnection t, DbConnectionPropertyInterceptionContext<string> c)
		{
			i.ConnectionStringSet(t, c);
		});
	}

	public virtual int GetConnectionTimeout(DbConnection connection, DbInterceptionContext interceptionContext)
	{
		Check.NotNull(connection, "connection");
		Check.NotNull(interceptionContext, "interceptionContext");
		return InternalDispatcher.Dispatch(connection, (DbConnection t, DbConnectionInterceptionContext<int> c) => t.ConnectionTimeout, new DbConnectionInterceptionContext<int>(interceptionContext), delegate(IDbConnectionInterceptor i, DbConnection t, DbConnectionInterceptionContext<int> c)
		{
			i.ConnectionTimeoutGetting(t, c);
		}, delegate(IDbConnectionInterceptor i, DbConnection t, DbConnectionInterceptionContext<int> c)
		{
			i.ConnectionTimeoutGot(t, c);
		});
	}

	public virtual string GetDatabase(DbConnection connection, DbInterceptionContext interceptionContext)
	{
		Check.NotNull(connection, "connection");
		Check.NotNull(interceptionContext, "interceptionContext");
		return InternalDispatcher.Dispatch(connection, (DbConnection t, DbConnectionInterceptionContext<string> c) => t.Database, new DbConnectionInterceptionContext<string>(interceptionContext), delegate(IDbConnectionInterceptor i, DbConnection t, DbConnectionInterceptionContext<string> c)
		{
			i.DatabaseGetting(t, c);
		}, delegate(IDbConnectionInterceptor i, DbConnection t, DbConnectionInterceptionContext<string> c)
		{
			i.DatabaseGot(t, c);
		});
	}

	public virtual string GetDataSource(DbConnection connection, DbInterceptionContext interceptionContext)
	{
		Check.NotNull(connection, "connection");
		Check.NotNull(interceptionContext, "interceptionContext");
		return InternalDispatcher.Dispatch(connection, (DbConnection t, DbConnectionInterceptionContext<string> c) => t.DataSource, new DbConnectionInterceptionContext<string>(interceptionContext), delegate(IDbConnectionInterceptor i, DbConnection t, DbConnectionInterceptionContext<string> c)
		{
			i.DataSourceGetting(t, c);
		}, delegate(IDbConnectionInterceptor i, DbConnection t, DbConnectionInterceptionContext<string> c)
		{
			i.DataSourceGot(t, c);
		});
	}

	public virtual void EnlistTransaction(DbConnection connection, EnlistTransactionInterceptionContext interceptionContext)
	{
		Check.NotNull(connection, "connection");
		Check.NotNull(interceptionContext, "interceptionContext");
		InternalDispatcher.Dispatch(connection, delegate(DbConnection t, EnlistTransactionInterceptionContext c)
		{
			t.EnlistTransaction(c.Transaction);
		}, new EnlistTransactionInterceptionContext(interceptionContext), delegate(IDbConnectionInterceptor i, DbConnection t, EnlistTransactionInterceptionContext c)
		{
			i.EnlistingTransaction(t, c);
		}, delegate(IDbConnectionInterceptor i, DbConnection t, EnlistTransactionInterceptionContext c)
		{
			i.EnlistedTransaction(t, c);
		});
	}

	public virtual void Open(DbConnection connection, DbInterceptionContext interceptionContext)
	{
		Check.NotNull(connection, "connection");
		Check.NotNull(interceptionContext, "interceptionContext");
		InternalDispatcher.Dispatch(connection, delegate(DbConnection t, DbConnectionInterceptionContext c)
		{
			t.Open();
		}, new DbConnectionInterceptionContext(interceptionContext), delegate(IDbConnectionInterceptor i, DbConnection t, DbConnectionInterceptionContext c)
		{
			i.Opening(t, c);
		}, delegate(IDbConnectionInterceptor i, DbConnection t, DbConnectionInterceptionContext c)
		{
			i.Opened(t, c);
		});
	}

	public virtual Task OpenAsync(DbConnection connection, DbInterceptionContext interceptionContext, CancellationToken cancellationToken)
	{
		Check.NotNull(connection, "connection");
		Check.NotNull(interceptionContext, "interceptionContext");
		return InternalDispatcher.DispatchAsync(connection, (DbConnection t, DbConnectionInterceptionContext c, CancellationToken ct) => t.OpenAsync(ct), new DbConnectionInterceptionContext(interceptionContext).AsAsync(), delegate(IDbConnectionInterceptor i, DbConnection t, DbConnectionInterceptionContext c)
		{
			i.Opening(t, c);
		}, delegate(IDbConnectionInterceptor i, DbConnection t, DbConnectionInterceptionContext c)
		{
			i.Opened(t, c);
		}, cancellationToken);
	}

	public virtual string GetServerVersion(DbConnection connection, DbInterceptionContext interceptionContext)
	{
		Check.NotNull(connection, "connection");
		Check.NotNull(interceptionContext, "interceptionContext");
		return InternalDispatcher.Dispatch(connection, (DbConnection t, DbConnectionInterceptionContext<string> c) => t.ServerVersion, new DbConnectionInterceptionContext<string>(interceptionContext), delegate(IDbConnectionInterceptor i, DbConnection t, DbConnectionInterceptionContext<string> c)
		{
			i.ServerVersionGetting(t, c);
		}, delegate(IDbConnectionInterceptor i, DbConnection t, DbConnectionInterceptionContext<string> c)
		{
			i.ServerVersionGot(t, c);
		});
	}

	public virtual ConnectionState GetState(DbConnection connection, DbInterceptionContext interceptionContext)
	{
		Check.NotNull(connection, "connection");
		Check.NotNull(interceptionContext, "interceptionContext");
		return InternalDispatcher.Dispatch(connection, (DbConnection t, DbConnectionInterceptionContext<ConnectionState> c) => t.State, new DbConnectionInterceptionContext<ConnectionState>(interceptionContext), delegate(IDbConnectionInterceptor i, DbConnection t, DbConnectionInterceptionContext<ConnectionState> c)
		{
			i.StateGetting(t, c);
		}, delegate(IDbConnectionInterceptor i, DbConnection t, DbConnectionInterceptionContext<ConnectionState> c)
		{
			i.StateGot(t, c);
		});
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
