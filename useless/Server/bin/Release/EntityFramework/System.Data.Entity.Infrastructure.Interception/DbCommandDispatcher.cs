using System.ComponentModel;
using System.Data.Common;
using System.Data.Entity.Utilities;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Infrastructure.Interception;

public class DbCommandDispatcher
{
	private readonly InternalDispatcher<IDbCommandInterceptor> _internalDispatcher = new InternalDispatcher<IDbCommandInterceptor>();

	internal InternalDispatcher<IDbCommandInterceptor> InternalDispatcher => _internalDispatcher;

	internal DbCommandDispatcher()
	{
	}

	public virtual int NonQuery(DbCommand command, DbCommandInterceptionContext interceptionContext)
	{
		Check.NotNull(command, "command");
		Check.NotNull(interceptionContext, "interceptionContext");
		return _internalDispatcher.Dispatch(command, (DbCommand t, DbCommandInterceptionContext<int> c) => t.ExecuteNonQuery(), new DbCommandInterceptionContext<int>(interceptionContext), delegate(IDbCommandInterceptor i, DbCommand t, DbCommandInterceptionContext<int> c)
		{
			i.NonQueryExecuting(t, c);
		}, delegate(IDbCommandInterceptor i, DbCommand t, DbCommandInterceptionContext<int> c)
		{
			i.NonQueryExecuted(t, c);
		});
	}

	public virtual object Scalar(DbCommand command, DbCommandInterceptionContext interceptionContext)
	{
		Check.NotNull(command, "command");
		Check.NotNull(interceptionContext, "interceptionContext");
		return _internalDispatcher.Dispatch(command, (DbCommand t, DbCommandInterceptionContext<object> c) => t.ExecuteScalar(), new DbCommandInterceptionContext<object>(interceptionContext), delegate(IDbCommandInterceptor i, DbCommand t, DbCommandInterceptionContext<object> c)
		{
			i.ScalarExecuting(t, c);
		}, delegate(IDbCommandInterceptor i, DbCommand t, DbCommandInterceptionContext<object> c)
		{
			i.ScalarExecuted(t, c);
		});
	}

	public virtual DbDataReader Reader(DbCommand command, DbCommandInterceptionContext interceptionContext)
	{
		Check.NotNull(command, "command");
		Check.NotNull(interceptionContext, "interceptionContext");
		return _internalDispatcher.Dispatch(command, (DbCommand t, DbCommandInterceptionContext<DbDataReader> c) => t.ExecuteReader(c.CommandBehavior), new DbCommandInterceptionContext<DbDataReader>(interceptionContext), delegate(IDbCommandInterceptor i, DbCommand t, DbCommandInterceptionContext<DbDataReader> c)
		{
			i.ReaderExecuting(t, c);
		}, delegate(IDbCommandInterceptor i, DbCommand t, DbCommandInterceptionContext<DbDataReader> c)
		{
			i.ReaderExecuted(t, c);
		});
	}

	public virtual Task<int> NonQueryAsync(DbCommand command, DbCommandInterceptionContext interceptionContext, CancellationToken cancellationToken)
	{
		Check.NotNull(command, "command");
		Check.NotNull(interceptionContext, "interceptionContext");
		return _internalDispatcher.DispatchAsync(command, (DbCommand t, DbCommandInterceptionContext<int> c, CancellationToken ct) => t.ExecuteNonQueryAsync(ct), new DbCommandInterceptionContext<int>(interceptionContext).AsAsync(), delegate(IDbCommandInterceptor i, DbCommand t, DbCommandInterceptionContext<int> c)
		{
			i.NonQueryExecuting(t, c);
		}, delegate(IDbCommandInterceptor i, DbCommand t, DbCommandInterceptionContext<int> c)
		{
			i.NonQueryExecuted(t, c);
		}, cancellationToken);
	}

	public virtual Task<object> ScalarAsync(DbCommand command, DbCommandInterceptionContext interceptionContext, CancellationToken cancellationToken)
	{
		Check.NotNull(command, "command");
		Check.NotNull(interceptionContext, "interceptionContext");
		return _internalDispatcher.DispatchAsync(command, (DbCommand t, DbCommandInterceptionContext<object> c, CancellationToken ct) => t.ExecuteScalarAsync(ct), new DbCommandInterceptionContext<object>(interceptionContext).AsAsync(), delegate(IDbCommandInterceptor i, DbCommand t, DbCommandInterceptionContext<object> c)
		{
			i.ScalarExecuting(t, c);
		}, delegate(IDbCommandInterceptor i, DbCommand t, DbCommandInterceptionContext<object> c)
		{
			i.ScalarExecuted(t, c);
		}, cancellationToken);
	}

	public virtual Task<DbDataReader> ReaderAsync(DbCommand command, DbCommandInterceptionContext interceptionContext, CancellationToken cancellationToken)
	{
		Check.NotNull(command, "command");
		Check.NotNull(interceptionContext, "interceptionContext");
		return _internalDispatcher.DispatchAsync(command, (DbCommand t, DbCommandInterceptionContext<DbDataReader> c, CancellationToken ct) => t.ExecuteReaderAsync(c.CommandBehavior, ct), new DbCommandInterceptionContext<DbDataReader>(interceptionContext).AsAsync(), delegate(IDbCommandInterceptor i, DbCommand t, DbCommandInterceptionContext<DbDataReader> c)
		{
			i.ReaderExecuting(t, c);
		}, delegate(IDbCommandInterceptor i, DbCommand t, DbCommandInterceptionContext<DbDataReader> c)
		{
			i.ReaderExecuted(t, c);
		}, cancellationToken);
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
