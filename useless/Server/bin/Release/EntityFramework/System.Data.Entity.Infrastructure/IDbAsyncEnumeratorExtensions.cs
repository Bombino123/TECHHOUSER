using System.Data.Entity.Utilities;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Infrastructure;

internal static class IDbAsyncEnumeratorExtensions
{
	private class CastDbAsyncEnumerator<TResult> : IDbAsyncEnumerator<TResult>, IDbAsyncEnumerator, IDisposable
	{
		private readonly IDbAsyncEnumerator _underlyingEnumerator;

		public TResult Current => (TResult)_underlyingEnumerator.Current;

		object IDbAsyncEnumerator.Current => _underlyingEnumerator.Current;

		public CastDbAsyncEnumerator(IDbAsyncEnumerator sourceEnumerator)
		{
			_underlyingEnumerator = sourceEnumerator;
		}

		public Task<bool> MoveNextAsync(CancellationToken cancellationToken)
		{
			return _underlyingEnumerator.MoveNextAsync(cancellationToken);
		}

		public void Dispose()
		{
			_underlyingEnumerator.Dispose();
		}
	}

	public static Task<bool> MoveNextAsync(this IDbAsyncEnumerator enumerator)
	{
		Check.NotNull(enumerator, "enumerator");
		return enumerator.MoveNextAsync(CancellationToken.None);
	}

	internal static IDbAsyncEnumerator<TResult> Cast<TResult>(this IDbAsyncEnumerator source)
	{
		return new CastDbAsyncEnumerator<TResult>(source);
	}
}
