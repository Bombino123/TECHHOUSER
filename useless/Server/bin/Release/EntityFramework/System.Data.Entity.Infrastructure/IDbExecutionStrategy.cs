using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Infrastructure;

public interface IDbExecutionStrategy
{
	bool RetriesOnFailure { get; }

	void Execute(Action operation);

	TResult Execute<TResult>(Func<TResult> operation);

	Task ExecuteAsync(Func<Task> operation, CancellationToken cancellationToken);

	Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> operation, CancellationToken cancellationToken);
}
