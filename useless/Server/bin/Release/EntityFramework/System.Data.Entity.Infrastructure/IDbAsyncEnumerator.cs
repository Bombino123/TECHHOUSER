using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Infrastructure;

public interface IDbAsyncEnumerator : IDisposable
{
	object Current { get; }

	Task<bool> MoveNextAsync(CancellationToken cancellationToken);
}
public interface IDbAsyncEnumerator<out T> : IDbAsyncEnumerator, IDisposable
{
	new T Current { get; }
}
