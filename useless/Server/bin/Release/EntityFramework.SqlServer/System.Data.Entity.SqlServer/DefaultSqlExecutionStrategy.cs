using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.SqlServer.Resources;
using System.Data.Entity.SqlServer.Utilities;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.SqlServer;

internal sealed class DefaultSqlExecutionStrategy : IDbExecutionStrategy
{
	public bool RetriesOnFailure => false;

	public void Execute(Action operation)
	{
		if (operation == null)
		{
			throw new ArgumentNullException("operation");
		}
		Execute(delegate
		{
			operation();
			return (object)null;
		});
	}

	public TResult Execute<TResult>(Func<TResult> operation)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		Check.NotNull(operation, "operation");
		try
		{
			return operation();
		}
		catch (Exception ex)
		{
			if (DbExecutionStrategy.UnwrapAndHandleException<bool>(ex, (Func<Exception, bool>)SqlAzureRetriableExceptionDetector.ShouldRetryOn))
			{
				throw new EntityException(Strings.TransientExceptionDetected, ex);
			}
			throw;
		}
	}

	public Task ExecuteAsync(Func<Task> operation, CancellationToken cancellationToken)
	{
		Check.NotNull(operation, "operation");
		cancellationToken.ThrowIfCancellationRequested();
		return ExecuteAsyncImplementation(async delegate
		{
			await operation().ConfigureAwait(continueOnCapturedContext: false);
			return true;
		});
	}

	public Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> operation, CancellationToken cancellationToken)
	{
		Check.NotNull(operation, "operation");
		cancellationToken.ThrowIfCancellationRequested();
		return ExecuteAsyncImplementation(operation);
	}

	private static async Task<TResult> ExecuteAsyncImplementation<TResult>(Func<Task<TResult>> func)
	{
		try
		{
			return await func().ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (Exception ex)
		{
			if (DbExecutionStrategy.UnwrapAndHandleException<bool>(ex, (Func<Exception, bool>)SqlAzureRetriableExceptionDetector.ShouldRetryOn))
			{
				throw new EntityException(Strings.TransientExceptionDetected, ex);
			}
			throw;
		}
	}
}
