using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Infrastructure;

public class DefaultExecutionStrategy : IDbExecutionStrategy
{
	public bool RetriesOnFailure => false;

	public void Execute(Action operation)
	{
		operation();
	}

	public TResult Execute<TResult>(Func<TResult> operation)
	{
		return operation();
	}

	public Task ExecuteAsync(Func<Task> operation, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		return operation();
	}

	public Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> operation, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		return operation();
	}
}
