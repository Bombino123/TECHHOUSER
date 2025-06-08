using System.ComponentModel;

namespace System.Data.Entity.Infrastructure.Interception;

public class DbDispatchers
{
	private readonly DbCommandTreeDispatcher _commandTreeDispatcher = new DbCommandTreeDispatcher();

	private readonly DbCommandDispatcher _commandDispatcher = new DbCommandDispatcher();

	private readonly DbTransactionDispatcher _transactionDispatcher = new DbTransactionDispatcher();

	private readonly DbConnectionDispatcher _dbConnectionDispatcher = new DbConnectionDispatcher();

	private readonly DbConfigurationDispatcher _configurationDispatcher = new DbConfigurationDispatcher();

	private readonly CancelableEntityConnectionDispatcher _cancelableEntityConnectionDispatcher = new CancelableEntityConnectionDispatcher();

	private readonly CancelableDbCommandDispatcher _cancelableCommandDispatcher = new CancelableDbCommandDispatcher();

	internal virtual DbCommandTreeDispatcher CommandTree => _commandTreeDispatcher;

	public virtual DbCommandDispatcher Command => _commandDispatcher;

	public virtual DbTransactionDispatcher Transaction => _transactionDispatcher;

	public virtual DbConnectionDispatcher Connection => _dbConnectionDispatcher;

	internal virtual DbConfigurationDispatcher Configuration => _configurationDispatcher;

	internal virtual CancelableEntityConnectionDispatcher CancelableEntityConnection => _cancelableEntityConnectionDispatcher;

	internal virtual CancelableDbCommandDispatcher CancelableCommand => _cancelableCommandDispatcher;

	internal DbDispatchers()
	{
	}

	internal virtual void AddInterceptor(IDbInterceptor interceptor)
	{
		_commandTreeDispatcher.InternalDispatcher.Add(interceptor);
		_commandDispatcher.InternalDispatcher.Add(interceptor);
		_transactionDispatcher.InternalDispatcher.Add(interceptor);
		_dbConnectionDispatcher.InternalDispatcher.Add(interceptor);
		_cancelableEntityConnectionDispatcher.InternalDispatcher.Add(interceptor);
		_cancelableCommandDispatcher.InternalDispatcher.Add(interceptor);
		_configurationDispatcher.InternalDispatcher.Add(interceptor);
	}

	internal virtual void RemoveInterceptor(IDbInterceptor interceptor)
	{
		_commandTreeDispatcher.InternalDispatcher.Remove(interceptor);
		_commandDispatcher.InternalDispatcher.Remove(interceptor);
		_transactionDispatcher.InternalDispatcher.Remove(interceptor);
		_dbConnectionDispatcher.InternalDispatcher.Remove(interceptor);
		_cancelableEntityConnectionDispatcher.InternalDispatcher.Remove(interceptor);
		_cancelableCommandDispatcher.InternalDispatcher.Remove(interceptor);
		_configurationDispatcher.InternalDispatcher.Remove(interceptor);
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
