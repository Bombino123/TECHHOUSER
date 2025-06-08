using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Utilities;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Internal;

internal class LazyAsyncEnumerator<T> : IDbAsyncEnumerator<T>, IDbAsyncEnumerator, IDisposable
{
	private readonly Func<CancellationToken, Task<ObjectResult<T>>> _getObjectResultAsync;

	private IDbAsyncEnumerator<T> _objectResultAsyncEnumerator;

	public T Current
	{
		get
		{
			if (_objectResultAsyncEnumerator != null)
			{
				return _objectResultAsyncEnumerator.Current;
			}
			return default(T);
		}
	}

	object IDbAsyncEnumerator.Current => Current;

	public LazyAsyncEnumerator(Func<CancellationToken, Task<ObjectResult<T>>> getObjectResultAsync)
	{
		_getObjectResultAsync = getObjectResultAsync;
	}

	public void Dispose()
	{
		if (_objectResultAsyncEnumerator != null)
		{
			_objectResultAsyncEnumerator.Dispose();
		}
	}

	public Task<bool> MoveNextAsync(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (_objectResultAsyncEnumerator != null)
		{
			return _objectResultAsyncEnumerator.MoveNextAsync(cancellationToken);
		}
		return FirstMoveNextAsync(cancellationToken);
	}

	private async Task<bool> FirstMoveNextAsync(CancellationToken cancellationToken)
	{
		ObjectResult<T> objectResult = await _getObjectResultAsync(cancellationToken).WithCurrentCulture();
		try
		{
			_objectResultAsyncEnumerator = ((IDbAsyncEnumerable<T>)objectResult).GetAsyncEnumerator();
		}
		catch
		{
			objectResult.Dispose();
			throw;
		}
		return await _objectResultAsyncEnumerator.MoveNextAsync(cancellationToken).WithCurrentCulture();
	}
}
