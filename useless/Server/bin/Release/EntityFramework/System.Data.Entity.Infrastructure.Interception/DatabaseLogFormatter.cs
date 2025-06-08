using System.ComponentModel;
using System.Data.Common;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Data.Entity.Infrastructure.Interception;

public class DatabaseLogFormatter : IDbCommandInterceptor, IDbInterceptor, IDbConnectionInterceptor, IDbTransactionInterceptor
{
	private const string StopwatchStateKey = "__LoggingStopwatch__";

	private readonly WeakReference _context;

	private readonly Action<string> _writeAction;

	private readonly Stopwatch _stopwatch = new Stopwatch();

	protected internal DbContext Context
	{
		get
		{
			if (_context == null || !_context.IsAlive)
			{
				return null;
			}
			return (DbContext)_context.Target;
		}
	}

	internal Action<string> WriteAction => _writeAction;

	[Obsolete("This stopwatch can give incorrect times. Use 'GetStopwatch' instead.")]
	protected internal Stopwatch Stopwatch => _stopwatch;

	public DatabaseLogFormatter(Action<string> writeAction)
	{
		Check.NotNull(writeAction, "writeAction");
		_writeAction = writeAction;
	}

	public DatabaseLogFormatter(DbContext context, Action<string> writeAction)
	{
		Check.NotNull(writeAction, "writeAction");
		_context = new WeakReference(context);
		_writeAction = writeAction;
	}

	protected virtual void Write(string output)
	{
		_writeAction(output);
	}

	protected internal Stopwatch GetStopwatch(DbCommandInterceptionContext interceptionContext)
	{
		if (_context != null)
		{
			return _stopwatch;
		}
		IDbMutableInterceptionContext dbMutableInterceptionContext = (IDbMutableInterceptionContext)interceptionContext;
		Stopwatch stopwatch = (Stopwatch)dbMutableInterceptionContext.MutableData.FindUserState("__LoggingStopwatch__");
		if (stopwatch == null)
		{
			stopwatch = new Stopwatch();
			dbMutableInterceptionContext.MutableData.SetUserState("__LoggingStopwatch__", stopwatch);
		}
		return stopwatch;
	}

	private void RestartStopwatch(DbCommandInterceptionContext interceptionContext)
	{
		Stopwatch stopwatch = GetStopwatch(interceptionContext);
		stopwatch.Restart();
		if (stopwatch != _stopwatch)
		{
			_stopwatch.Restart();
		}
	}

	private void StopStopwatch(DbCommandInterceptionContext interceptionContext)
	{
		Stopwatch stopwatch = GetStopwatch(interceptionContext);
		stopwatch.Stop();
		if (stopwatch != _stopwatch)
		{
			_stopwatch.Stop();
		}
	}

	public virtual void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
	{
		Check.NotNull(command, "command");
		Check.NotNull(interceptionContext, "interceptionContext");
		Executing(command, interceptionContext);
		RestartStopwatch(interceptionContext);
	}

	public virtual void NonQueryExecuted(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
	{
		Check.NotNull(command, "command");
		Check.NotNull(interceptionContext, "interceptionContext");
		StopStopwatch(interceptionContext);
		Executed(command, interceptionContext);
	}

	public virtual void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
	{
		Check.NotNull(command, "command");
		Check.NotNull(interceptionContext, "interceptionContext");
		Executing(command, interceptionContext);
		RestartStopwatch(interceptionContext);
	}

	public virtual void ReaderExecuted(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
	{
		Check.NotNull(command, "command");
		Check.NotNull(interceptionContext, "interceptionContext");
		StopStopwatch(interceptionContext);
		Executed(command, interceptionContext);
	}

	public virtual void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
	{
		Check.NotNull(command, "command");
		Check.NotNull(interceptionContext, "interceptionContext");
		Executing(command, interceptionContext);
		RestartStopwatch(interceptionContext);
	}

	public virtual void ScalarExecuted(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
	{
		Check.NotNull(command, "command");
		Check.NotNull(interceptionContext, "interceptionContext");
		StopStopwatch(interceptionContext);
		Executed(command, interceptionContext);
	}

	public virtual void Executing<TResult>(DbCommand command, DbCommandInterceptionContext<TResult> interceptionContext)
	{
		Check.NotNull(command, "command");
		Check.NotNull(interceptionContext, "interceptionContext");
		if (Context == null || interceptionContext.DbContexts.Contains(Context, object.ReferenceEquals))
		{
			LogCommand(command, interceptionContext);
		}
	}

	public virtual void Executed<TResult>(DbCommand command, DbCommandInterceptionContext<TResult> interceptionContext)
	{
		Check.NotNull(command, "command");
		Check.NotNull(interceptionContext, "interceptionContext");
		if (Context == null || interceptionContext.DbContexts.Contains(Context, object.ReferenceEquals))
		{
			LogResult(command, interceptionContext);
		}
	}

	public virtual void LogCommand<TResult>(DbCommand command, DbCommandInterceptionContext<TResult> interceptionContext)
	{
		Check.NotNull(command, "command");
		Check.NotNull(interceptionContext, "interceptionContext");
		string text = command.CommandText ?? "<null>";
		if (text.EndsWith(Environment.NewLine, StringComparison.Ordinal))
		{
			Write(text);
		}
		else
		{
			Write(text);
			Write(Environment.NewLine);
		}
		if (command.Parameters != null)
		{
			foreach (DbParameter item in command.Parameters.OfType<DbParameter>())
			{
				LogParameter(command, interceptionContext, item);
			}
		}
		Write(interceptionContext.IsAsync ? Strings.CommandLogAsync(DateTimeOffset.Now, Environment.NewLine) : Strings.CommandLogNonAsync(DateTimeOffset.Now, Environment.NewLine));
	}

	public virtual void LogParameter<TResult>(DbCommand command, DbCommandInterceptionContext<TResult> interceptionContext, DbParameter parameter)
	{
		Check.NotNull(command, "command");
		Check.NotNull(interceptionContext, "interceptionContext");
		Check.NotNull(parameter, "parameter");
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("-- ").Append(parameter.ParameterName).Append(": '")
			.Append((parameter.Value == null || parameter.Value == DBNull.Value) ? "null" : parameter.Value)
			.Append("' (Type = ")
			.Append(parameter.DbType);
		if (parameter.Direction != ParameterDirection.Input)
		{
			stringBuilder.Append(", Direction = ").Append(parameter.Direction);
		}
		if (!parameter.IsNullable)
		{
			stringBuilder.Append(", IsNullable = false");
		}
		if (parameter.Size != 0)
		{
			stringBuilder.Append(", Size = ").Append(parameter.Size);
		}
		if (((IDbDataParameter)parameter).Precision != 0)
		{
			stringBuilder.Append(", Precision = ").Append(((IDbDataParameter)parameter).Precision);
		}
		if (((IDbDataParameter)parameter).Scale != 0)
		{
			stringBuilder.Append(", Scale = ").Append(((IDbDataParameter)parameter).Scale);
		}
		stringBuilder.Append(")").Append(Environment.NewLine);
		Write(stringBuilder.ToString());
	}

	public virtual void LogResult<TResult>(DbCommand command, DbCommandInterceptionContext<TResult> interceptionContext)
	{
		Check.NotNull(command, "command");
		Check.NotNull(interceptionContext, "interceptionContext");
		Stopwatch stopwatch = _stopwatch;
		if (_context == null)
		{
			Stopwatch stopwatch2 = (Stopwatch)((IDbMutableInterceptionContext)interceptionContext).MutableData.FindUserState("__LoggingStopwatch__");
			if (stopwatch2 != null)
			{
				stopwatch = stopwatch2;
			}
		}
		if (interceptionContext.Exception != null)
		{
			Write(Strings.CommandLogFailed(stopwatch.ElapsedMilliseconds, interceptionContext.Exception.Message, Environment.NewLine));
		}
		else if (interceptionContext.TaskStatus.HasFlag(TaskStatus.Canceled))
		{
			Write(Strings.CommandLogCanceled(stopwatch.ElapsedMilliseconds, Environment.NewLine));
		}
		else
		{
			TResult result = interceptionContext.Result;
			string p = ((result == null) ? "null" : ((result is DbDataReader) ? result.GetType().Name : result.ToString()));
			Write(Strings.CommandLogComplete(stopwatch.ElapsedMilliseconds, p, Environment.NewLine));
		}
		Write(Environment.NewLine);
	}

	public virtual void BeginningTransaction(DbConnection connection, BeginTransactionInterceptionContext interceptionContext)
	{
	}

	public virtual void BeganTransaction(DbConnection connection, BeginTransactionInterceptionContext interceptionContext)
	{
		Check.NotNull(connection, "connection");
		Check.NotNull(interceptionContext, "interceptionContext");
		if (Context == null || interceptionContext.DbContexts.Contains(Context, object.ReferenceEquals))
		{
			if (interceptionContext.Exception != null)
			{
				Write(Strings.TransactionStartErrorLog(DateTimeOffset.Now, interceptionContext.Exception.Message, Environment.NewLine));
			}
			else
			{
				Write(Strings.TransactionStartedLog(DateTimeOffset.Now, Environment.NewLine));
			}
		}
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
		Check.NotNull(connection, "connection");
		Check.NotNull(interceptionContext, "interceptionContext");
		if (Context == null || interceptionContext.DbContexts.Contains(Context, object.ReferenceEquals))
		{
			if (interceptionContext.Exception != null)
			{
				Write(interceptionContext.IsAsync ? Strings.ConnectionOpenErrorLogAsync(DateTimeOffset.Now, interceptionContext.Exception.Message, Environment.NewLine) : Strings.ConnectionOpenErrorLog(DateTimeOffset.Now, interceptionContext.Exception.Message, Environment.NewLine));
			}
			else if (interceptionContext.TaskStatus.HasFlag(TaskStatus.Canceled))
			{
				Write(Strings.ConnectionOpenCanceledLog(DateTimeOffset.Now, Environment.NewLine));
			}
			else
			{
				Write(interceptionContext.IsAsync ? Strings.ConnectionOpenedLogAsync(DateTimeOffset.Now, Environment.NewLine) : Strings.ConnectionOpenedLog(DateTimeOffset.Now, Environment.NewLine));
			}
		}
	}

	public virtual void Closing(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
	{
	}

	public virtual void Closed(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
	{
		Check.NotNull(connection, "connection");
		Check.NotNull(interceptionContext, "interceptionContext");
		if (Context == null || interceptionContext.DbContexts.Contains(Context, object.ReferenceEquals))
		{
			if (interceptionContext.Exception != null)
			{
				Write(Strings.ConnectionCloseErrorLog(DateTimeOffset.Now, interceptionContext.Exception.Message, Environment.NewLine));
			}
			else
			{
				Write(Strings.ConnectionClosedLog(DateTimeOffset.Now, Environment.NewLine));
			}
		}
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
		Check.NotNull(connection, "connection");
		Check.NotNull(interceptionContext, "interceptionContext");
		if ((Context == null || interceptionContext.DbContexts.Contains(Context, object.ReferenceEquals)) && connection.State == ConnectionState.Open)
		{
			Write(Strings.ConnectionDisposedLog(DateTimeOffset.Now, Environment.NewLine));
		}
	}

	public virtual void Disposed(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
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
		Check.NotNull(transaction, "transaction");
		Check.NotNull(interceptionContext, "interceptionContext");
		if (Context == null || interceptionContext.DbContexts.Contains(Context, object.ReferenceEquals))
		{
			if (interceptionContext.Exception != null)
			{
				Write(Strings.TransactionCommitErrorLog(DateTimeOffset.Now, interceptionContext.Exception.Message, Environment.NewLine));
			}
			else
			{
				Write(Strings.TransactionCommittedLog(DateTimeOffset.Now, Environment.NewLine));
			}
		}
	}

	public virtual void Disposing(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext)
	{
		Check.NotNull(transaction, "transaction");
		Check.NotNull(interceptionContext, "interceptionContext");
		if ((Context == null || interceptionContext.DbContexts.Contains(Context, object.ReferenceEquals)) && transaction.Connection != null)
		{
			Write(Strings.TransactionDisposedLog(DateTimeOffset.Now, Environment.NewLine));
		}
	}

	public virtual void Disposed(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext)
	{
	}

	public virtual void RollingBack(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext)
	{
	}

	public virtual void RolledBack(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext)
	{
		Check.NotNull(transaction, "transaction");
		Check.NotNull(interceptionContext, "interceptionContext");
		if (Context == null || interceptionContext.DbContexts.Contains(Context, object.ReferenceEquals))
		{
			if (interceptionContext.Exception != null)
			{
				Write(Strings.TransactionRollbackErrorLog(DateTimeOffset.Now, interceptionContext.Exception.Message, Environment.NewLine));
			}
			else
			{
				Write(Strings.TransactionRolledBackLog(DateTimeOffset.Now, Environment.NewLine));
			}
		}
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
