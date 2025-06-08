using System.ComponentModel;
using System.Data.Common;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Infrastructure.Interception;

public class DbTransactionDispatcher
{
	private readonly InternalDispatcher<IDbTransactionInterceptor> _internalDispatcher = new InternalDispatcher<IDbTransactionInterceptor>();

	internal InternalDispatcher<IDbTransactionInterceptor> InternalDispatcher => _internalDispatcher;

	internal DbTransactionDispatcher()
	{
	}

	public virtual DbConnection GetConnection(DbTransaction transaction, DbInterceptionContext interceptionContext)
	{
		Check.NotNull(transaction, "transaction");
		Check.NotNull(interceptionContext, "interceptionContext");
		return InternalDispatcher.Dispatch(transaction, (DbTransaction t, DbTransactionInterceptionContext<DbConnection> c) => t.Connection, new DbTransactionInterceptionContext<DbConnection>(interceptionContext), delegate(IDbTransactionInterceptor i, DbTransaction t, DbTransactionInterceptionContext<DbConnection> c)
		{
			i.ConnectionGetting(t, c);
		}, delegate(IDbTransactionInterceptor i, DbTransaction t, DbTransactionInterceptionContext<DbConnection> c)
		{
			i.ConnectionGot(t, c);
		});
	}

	public virtual IsolationLevel GetIsolationLevel(DbTransaction transaction, DbInterceptionContext interceptionContext)
	{
		Check.NotNull(transaction, "transaction");
		Check.NotNull(interceptionContext, "interceptionContext");
		return InternalDispatcher.Dispatch(transaction, (DbTransaction t, DbTransactionInterceptionContext<IsolationLevel> c) => t.IsolationLevel, new DbTransactionInterceptionContext<IsolationLevel>(interceptionContext), delegate(IDbTransactionInterceptor i, DbTransaction t, DbTransactionInterceptionContext<IsolationLevel> c)
		{
			i.IsolationLevelGetting(t, c);
		}, delegate(IDbTransactionInterceptor i, DbTransaction t, DbTransactionInterceptionContext<IsolationLevel> c)
		{
			i.IsolationLevelGot(t, c);
		});
	}

	public virtual void Commit(DbTransaction transaction, DbInterceptionContext interceptionContext)
	{
		Check.NotNull(transaction, "transaction");
		Check.NotNull(interceptionContext, "interceptionContext");
		InternalDispatcher.Dispatch(transaction, delegate(DbTransaction t, DbTransactionInterceptionContext c)
		{
			t.Commit();
		}, new DbTransactionInterceptionContext(interceptionContext).WithConnection(transaction.Connection), delegate(IDbTransactionInterceptor i, DbTransaction t, DbTransactionInterceptionContext c)
		{
			i.Committing(t, c);
		}, delegate(IDbTransactionInterceptor i, DbTransaction t, DbTransactionInterceptionContext c)
		{
			i.Committed(t, c);
		});
	}

	public virtual void Dispose(DbTransaction transaction, DbInterceptionContext interceptionContext)
	{
		Check.NotNull(transaction, "transaction");
		Check.NotNull(interceptionContext, "interceptionContext");
		DbTransactionInterceptionContext dbTransactionInterceptionContext = new DbTransactionInterceptionContext(interceptionContext);
		if (transaction.Connection != null)
		{
			dbTransactionInterceptionContext = dbTransactionInterceptionContext.WithConnection(transaction.Connection);
		}
		InternalDispatcher.Dispatch(transaction, delegate(DbTransaction t, DbTransactionInterceptionContext c)
		{
			t.Dispose();
		}, dbTransactionInterceptionContext, delegate(IDbTransactionInterceptor i, DbTransaction t, DbTransactionInterceptionContext c)
		{
			i.Disposing(t, c);
		}, delegate(IDbTransactionInterceptor i, DbTransaction t, DbTransactionInterceptionContext c)
		{
			i.Disposed(t, c);
		});
	}

	public virtual void Rollback(DbTransaction transaction, DbInterceptionContext interceptionContext)
	{
		Check.NotNull(transaction, "transaction");
		Check.NotNull(interceptionContext, "interceptionContext");
		InternalDispatcher.Dispatch(transaction, delegate(DbTransaction t, DbTransactionInterceptionContext c)
		{
			t.Rollback();
		}, new DbTransactionInterceptionContext(interceptionContext).WithConnection(transaction.Connection), delegate(IDbTransactionInterceptor i, DbTransaction t, DbTransactionInterceptionContext c)
		{
			i.RollingBack(t, c);
		}, delegate(IDbTransactionInterceptor i, DbTransaction t, DbTransactionInterceptionContext c)
		{
			i.RolledBack(t, c);
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
