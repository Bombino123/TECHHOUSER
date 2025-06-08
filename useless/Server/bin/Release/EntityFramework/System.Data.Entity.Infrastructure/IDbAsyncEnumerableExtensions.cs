using System.Collections.Generic;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Infrastructure;

internal static class IDbAsyncEnumerableExtensions
{
	private class CastDbAsyncEnumerable<TResult> : IDbAsyncEnumerable<TResult>, IDbAsyncEnumerable
	{
		private readonly IDbAsyncEnumerable _underlyingEnumerable;

		public CastDbAsyncEnumerable(IDbAsyncEnumerable sourceEnumerable)
		{
			_underlyingEnumerable = sourceEnumerable;
		}

		public IDbAsyncEnumerator<TResult> GetAsyncEnumerator()
		{
			return _underlyingEnumerable.GetAsyncEnumerator().Cast<TResult>();
		}

		IDbAsyncEnumerator IDbAsyncEnumerable.GetAsyncEnumerator()
		{
			return _underlyingEnumerable.GetAsyncEnumerator();
		}
	}

	private static class IdentityFunction<TElement>
	{
		internal static Func<TElement, TElement> Instance => (TElement x) => x;
	}

	internal static async Task ForEachAsync(this IDbAsyncEnumerable source, Action<object> action, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		using IDbAsyncEnumerator enumerator = source.GetAsyncEnumerator();
		if (await enumerator.MoveNextAsync(cancellationToken).WithCurrentCulture())
		{
			Task<bool> task;
			do
			{
				cancellationToken.ThrowIfCancellationRequested();
				object current = enumerator.Current;
				task = enumerator.MoveNextAsync(cancellationToken);
				action(current);
			}
			while (await task.WithCurrentCulture());
		}
	}

	internal static Task ForEachAsync<T>(this IDbAsyncEnumerable<T> source, Action<T> action, CancellationToken cancellationToken)
	{
		return ForEachAsync(source.GetAsyncEnumerator(), action, cancellationToken);
	}

	private static async Task ForEachAsync<T>(IDbAsyncEnumerator<T> enumerator, Action<T> action, CancellationToken cancellationToken)
	{
		using (enumerator)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (await enumerator.MoveNextAsync(cancellationToken).WithCurrentCulture())
			{
				Task<bool> task;
				do
				{
					cancellationToken.ThrowIfCancellationRequested();
					T current = enumerator.Current;
					task = enumerator.MoveNextAsync(cancellationToken);
					action(current);
				}
				while (await task.WithCurrentCulture());
			}
		}
	}

	internal static Task<List<T>> ToListAsync<T>(this IDbAsyncEnumerable source)
	{
		return source.ToListAsync<T>(CancellationToken.None);
	}

	internal static async Task<List<T>> ToListAsync<T>(this IDbAsyncEnumerable source, CancellationToken cancellationToken)
	{
		List<T> list = new List<T>();
		await source.ForEachAsync(delegate(object e)
		{
			list.Add((T)e);
		}, cancellationToken).WithCurrentCulture();
		return list;
	}

	internal static Task<List<T>> ToListAsync<T>(this IDbAsyncEnumerable<T> source)
	{
		return source.ToListAsync(CancellationToken.None);
	}

	internal static Task<List<T>> ToListAsync<T>(this IDbAsyncEnumerable<T> source, CancellationToken cancellationToken)
	{
		TaskCompletionSource<List<T>> tcs = new TaskCompletionSource<List<T>>();
		List<T> list = new List<T>();
		source.ForEachAsync(list.Add, cancellationToken).ContinueWith(delegate(Task t)
		{
			if (t.IsFaulted)
			{
				tcs.TrySetException(t.Exception.InnerExceptions);
			}
			else if (t.IsCanceled)
			{
				tcs.TrySetCanceled();
			}
			else
			{
				tcs.TrySetResult(list);
			}
		}, TaskContinuationOptions.ExecuteSynchronously);
		return tcs.Task;
	}

	internal static Task<T[]> ToArrayAsync<T>(this IDbAsyncEnumerable<T> source)
	{
		return source.ToArrayAsync(CancellationToken.None);
	}

	internal static async Task<T[]> ToArrayAsync<T>(this IDbAsyncEnumerable<T> source, CancellationToken cancellationToken)
	{
		return (await source.ToListAsync(cancellationToken).WithCurrentCulture()).ToArray();
	}

	internal static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(this IDbAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector)
	{
		return source.ToDictionaryAsync(keySelector, IdentityFunction<TSource>.Instance, null, CancellationToken.None);
	}

	internal static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(this IDbAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, CancellationToken cancellationToken)
	{
		return source.ToDictionaryAsync(keySelector, IdentityFunction<TSource>.Instance, null, cancellationToken);
	}

	internal static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(this IDbAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
	{
		return source.ToDictionaryAsync(keySelector, IdentityFunction<TSource>.Instance, comparer, CancellationToken.None);
	}

	internal static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(this IDbAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer, CancellationToken cancellationToken)
	{
		return source.ToDictionaryAsync(keySelector, IdentityFunction<TSource>.Instance, comparer, cancellationToken);
	}

	internal static Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(this IDbAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
	{
		return source.ToDictionaryAsync(keySelector, elementSelector, null, CancellationToken.None);
	}

	internal static Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(this IDbAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, CancellationToken cancellationToken)
	{
		return source.ToDictionaryAsync(keySelector, elementSelector, null, cancellationToken);
	}

	internal static Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(this IDbAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
	{
		return source.ToDictionaryAsync(keySelector, elementSelector, comparer, CancellationToken.None);
	}

	internal static async Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(this IDbAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer, CancellationToken cancellationToken)
	{
		Dictionary<TKey, TElement> d = new Dictionary<TKey, TElement>(comparer);
		await source.ForEachAsync(delegate(TSource element)
		{
			d.Add(keySelector(element), elementSelector(element));
		}, cancellationToken).WithCurrentCulture();
		return d;
	}

	internal static IDbAsyncEnumerable<TResult> Cast<TResult>(this IDbAsyncEnumerable source)
	{
		return new CastDbAsyncEnumerable<TResult>(source);
	}

	internal static Task<TSource> FirstAsync<TSource>(this IDbAsyncEnumerable<TSource> source)
	{
		return source.FirstAsync(CancellationToken.None);
	}

	internal static Task<TSource> FirstAsync<TSource>(this IDbAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		return source.FirstAsync(predicate, CancellationToken.None);
	}

	internal static async Task<TSource> FirstAsync<TSource>(this IDbAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		using (IDbAsyncEnumerator<TSource> e = source.GetAsyncEnumerator())
		{
			if (await e.MoveNextAsync(cancellationToken).WithCurrentCulture())
			{
				return e.Current;
			}
		}
		throw Error.EmptySequence();
	}

	internal static async Task<TSource> FirstAsync<TSource>(this IDbAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		using (IDbAsyncEnumerator<TSource> e = source.GetAsyncEnumerator())
		{
			if (await e.MoveNextAsync(cancellationToken).WithCurrentCulture() && predicate(e.Current))
			{
				return e.Current;
			}
		}
		throw Error.NoMatch();
	}

	internal static Task<TSource> FirstOrDefaultAsync<TSource>(this IDbAsyncEnumerable<TSource> source)
	{
		return source.FirstOrDefaultAsync(CancellationToken.None);
	}

	internal static Task<TSource> FirstOrDefaultAsync<TSource>(this IDbAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		return source.FirstOrDefaultAsync(predicate, CancellationToken.None);
	}

	internal static async Task<TSource> FirstOrDefaultAsync<TSource>(this IDbAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		using (IDbAsyncEnumerator<TSource> e = source.GetAsyncEnumerator())
		{
			if (await e.MoveNextAsync(cancellationToken).WithCurrentCulture())
			{
				return e.Current;
			}
		}
		return default(TSource);
	}

	internal static async Task<TSource> FirstOrDefaultAsync<TSource>(this IDbAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		using (IDbAsyncEnumerator<TSource> e = source.GetAsyncEnumerator())
		{
			if (await e.MoveNextAsync(cancellationToken).WithCurrentCulture() && predicate(e.Current))
			{
				return e.Current;
			}
		}
		return default(TSource);
	}

	internal static Task<TSource> SingleAsync<TSource>(this IDbAsyncEnumerable<TSource> source)
	{
		return source.SingleAsync(CancellationToken.None);
	}

	internal static async Task<TSource> SingleAsync<TSource>(this IDbAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		using (IDbAsyncEnumerator<TSource> e = source.GetAsyncEnumerator())
		{
			if (!(await e.MoveNextAsync(cancellationToken).WithCurrentCulture()))
			{
				throw Error.EmptySequence();
			}
			cancellationToken.ThrowIfCancellationRequested();
			TSource result = e.Current;
			if (!(await e.MoveNextAsync(cancellationToken).WithCurrentCulture()))
			{
				return result;
			}
		}
		throw Error.MoreThanOneElement();
	}

	internal static Task<TSource> SingleAsync<TSource>(this IDbAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		return source.SingleAsync(predicate, CancellationToken.None);
	}

	internal static async Task<TSource> SingleAsync<TSource>(this IDbAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		TSource result = default(TSource);
		long count = 0L;
		using (IDbAsyncEnumerator<TSource> e = source.GetAsyncEnumerator())
		{
			while (await e.MoveNextAsync(cancellationToken).WithCurrentCulture())
			{
				cancellationToken.ThrowIfCancellationRequested();
				if (predicate(e.Current))
				{
					result = e.Current;
					count = checked(count + 1);
				}
			}
		}
		return count switch
		{
			0L => throw Error.NoMatch(), 
			1L => result, 
			_ => throw Error.MoreThanOneMatch(), 
		};
	}

	internal static Task<TSource> SingleOrDefaultAsync<TSource>(this IDbAsyncEnumerable<TSource> source)
	{
		return source.SingleOrDefaultAsync(CancellationToken.None);
	}

	internal static async Task<TSource> SingleOrDefaultAsync<TSource>(this IDbAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		using (IDbAsyncEnumerator<TSource> e = source.GetAsyncEnumerator())
		{
			if (!(await e.MoveNextAsync(cancellationToken).WithCurrentCulture()))
			{
				return default(TSource);
			}
			cancellationToken.ThrowIfCancellationRequested();
			TSource result = e.Current;
			if (!(await e.MoveNextAsync(cancellationToken).WithCurrentCulture()))
			{
				return result;
			}
		}
		throw Error.MoreThanOneElement();
	}

	internal static Task<TSource> SingleOrDefaultAsync<TSource>(this IDbAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		return source.SingleOrDefaultAsync(predicate, CancellationToken.None);
	}

	internal static async Task<TSource> SingleOrDefaultAsync<TSource>(this IDbAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		TSource result = default(TSource);
		long count = 0L;
		using (IDbAsyncEnumerator<TSource> e = source.GetAsyncEnumerator())
		{
			while (await e.MoveNextAsync(cancellationToken).WithCurrentCulture())
			{
				cancellationToken.ThrowIfCancellationRequested();
				if (predicate(e.Current))
				{
					result = e.Current;
					count = checked(count + 1);
				}
			}
		}
		if (count < 2)
		{
			return result;
		}
		throw Error.MoreThanOneMatch();
	}

	internal static Task<bool> ContainsAsync<TSource>(this IDbAsyncEnumerable<TSource> source, TSource value)
	{
		return source.ContainsAsync(value, CancellationToken.None);
	}

	internal static async Task<bool> ContainsAsync<TSource>(this IDbAsyncEnumerable<TSource> source, TSource value, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		using (IDbAsyncEnumerator<TSource> e = source.GetAsyncEnumerator())
		{
			while (await e.MoveNextAsync(cancellationToken).WithCurrentCulture())
			{
				if (EqualityComparer<TSource>.Default.Equals(e.Current, value))
				{
					return true;
				}
				cancellationToken.ThrowIfCancellationRequested();
			}
		}
		return false;
	}

	internal static Task<bool> AnyAsync<TSource>(this IDbAsyncEnumerable<TSource> source)
	{
		return source.AnyAsync(CancellationToken.None);
	}

	internal static async Task<bool> AnyAsync<TSource>(this IDbAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		using (IDbAsyncEnumerator<TSource> e = source.GetAsyncEnumerator())
		{
			if (await e.MoveNextAsync(cancellationToken).WithCurrentCulture())
			{
				return true;
			}
		}
		return false;
	}

	internal static Task<bool> AnyAsync<TSource>(this IDbAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		return source.AnyAsync(predicate, CancellationToken.None);
	}

	internal static async Task<bool> AnyAsync<TSource>(this IDbAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		using (IDbAsyncEnumerator<TSource> e = source.GetAsyncEnumerator())
		{
			while (await e.MoveNextAsync(cancellationToken).WithCurrentCulture())
			{
				if (predicate(e.Current))
				{
					return true;
				}
				cancellationToken.ThrowIfCancellationRequested();
			}
		}
		return false;
	}

	internal static Task<bool> AllAsync<TSource>(this IDbAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		return source.AllAsync(predicate, CancellationToken.None);
	}

	internal static async Task<bool> AllAsync<TSource>(this IDbAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		using (IDbAsyncEnumerator<TSource> e = source.GetAsyncEnumerator())
		{
			while (await e.MoveNextAsync(cancellationToken).WithCurrentCulture())
			{
				if (!predicate(e.Current))
				{
					return false;
				}
				cancellationToken.ThrowIfCancellationRequested();
			}
		}
		return true;
	}

	internal static Task<int> CountAsync<TSource>(this IDbAsyncEnumerable<TSource> source)
	{
		return source.CountAsync(CancellationToken.None);
	}

	internal static async Task<int> CountAsync<TSource>(this IDbAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		int count = 0;
		using (IDbAsyncEnumerator<TSource> e = source.GetAsyncEnumerator())
		{
			while (await e.MoveNextAsync(cancellationToken).WithCurrentCulture())
			{
				cancellationToken.ThrowIfCancellationRequested();
				count = checked(count + 1);
			}
		}
		return count;
	}

	internal static Task<int> CountAsync<TSource>(this IDbAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		return source.CountAsync(predicate, CancellationToken.None);
	}

	internal static async Task<int> CountAsync<TSource>(this IDbAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		int count = 0;
		using (IDbAsyncEnumerator<TSource> e = source.GetAsyncEnumerator())
		{
			while (await e.MoveNextAsync(cancellationToken).WithCurrentCulture())
			{
				cancellationToken.ThrowIfCancellationRequested();
				if (predicate(e.Current))
				{
					count = checked(count + 1);
				}
			}
		}
		return count;
	}

	internal static Task<long> LongCountAsync<TSource>(this IDbAsyncEnumerable<TSource> source)
	{
		return source.LongCountAsync(CancellationToken.None);
	}

	internal static async Task<long> LongCountAsync<TSource>(this IDbAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		long count = 0L;
		using (IDbAsyncEnumerator<TSource> e = source.GetAsyncEnumerator())
		{
			while (await e.MoveNextAsync(cancellationToken).WithCurrentCulture())
			{
				cancellationToken.ThrowIfCancellationRequested();
				count = checked(count + 1);
			}
		}
		return count;
	}

	internal static Task<long> LongCountAsync<TSource>(this IDbAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		return source.LongCountAsync(predicate, CancellationToken.None);
	}

	internal static async Task<long> LongCountAsync<TSource>(this IDbAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		long count = 0L;
		using (IDbAsyncEnumerator<TSource> e = source.GetAsyncEnumerator())
		{
			while (await e.MoveNextAsync(cancellationToken).WithCurrentCulture())
			{
				cancellationToken.ThrowIfCancellationRequested();
				if (predicate(e.Current))
				{
					count = checked(count + 1);
				}
			}
		}
		return count;
	}

	internal static Task<TSource> MinAsync<TSource>(this IDbAsyncEnumerable<TSource> source)
	{
		return source.MinAsync(CancellationToken.None);
	}

	internal static async Task<TSource> MinAsync<TSource>(this IDbAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		Comparer<TSource> comparer = Comparer<TSource>.Default;
		TSource value = default(TSource);
		if (value == null)
		{
			using (IDbAsyncEnumerator<TSource> e = source.GetAsyncEnumerator())
			{
				while (await e.MoveNextAsync(cancellationToken).WithCurrentCulture())
				{
					cancellationToken.ThrowIfCancellationRequested();
					if (e.Current != null && (value == null || comparer.Compare(e.Current, value) < 0))
					{
						value = e.Current;
					}
				}
			}
			return value;
		}
		bool hasValue = false;
		using (IDbAsyncEnumerator<TSource> e = source.GetAsyncEnumerator())
		{
			while (await e.MoveNextAsync(cancellationToken).WithCurrentCulture())
			{
				cancellationToken.ThrowIfCancellationRequested();
				if (hasValue)
				{
					if (comparer.Compare(e.Current, value) < 0)
					{
						value = e.Current;
					}
				}
				else
				{
					value = e.Current;
					hasValue = true;
				}
			}
		}
		if (hasValue)
		{
			return value;
		}
		throw Error.EmptySequence();
	}

	internal static Task<TSource> MaxAsync<TSource>(this IDbAsyncEnumerable<TSource> source)
	{
		return source.MaxAsync(CancellationToken.None);
	}

	internal static async Task<TSource> MaxAsync<TSource>(this IDbAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		Comparer<TSource> comparer = Comparer<TSource>.Default;
		TSource value = default(TSource);
		if (value == null)
		{
			using (IDbAsyncEnumerator<TSource> e = source.GetAsyncEnumerator())
			{
				while (await e.MoveNextAsync(cancellationToken).WithCurrentCulture())
				{
					cancellationToken.ThrowIfCancellationRequested();
					if (e.Current != null && (value == null || comparer.Compare(e.Current, value) > 0))
					{
						value = e.Current;
					}
				}
			}
			return value;
		}
		bool hasValue = false;
		using (IDbAsyncEnumerator<TSource> e = source.GetAsyncEnumerator())
		{
			while (await e.MoveNextAsync(cancellationToken).WithCurrentCulture())
			{
				cancellationToken.ThrowIfCancellationRequested();
				if (hasValue)
				{
					if (comparer.Compare(e.Current, value) > 0)
					{
						value = e.Current;
					}
				}
				else
				{
					value = e.Current;
					hasValue = true;
				}
			}
		}
		if (hasValue)
		{
			return value;
		}
		throw Error.EmptySequence();
	}

	internal static Task<int> SumAsync(this IDbAsyncEnumerable<int> source)
	{
		return source.SumAsync(CancellationToken.None);
	}

	internal static async Task<int> SumAsync(this IDbAsyncEnumerable<int> source, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		long sum = 0L;
		using (IDbAsyncEnumerator<int> e = source.GetAsyncEnumerator())
		{
			while (await e.MoveNextAsync(cancellationToken).WithCurrentCulture())
			{
				cancellationToken.ThrowIfCancellationRequested();
				sum = checked(sum + e.Current);
			}
		}
		return (int)sum;
	}

	internal static Task<int?> SumAsync(this IDbAsyncEnumerable<int?> source)
	{
		return source.SumAsync(CancellationToken.None);
	}

	internal static async Task<int?> SumAsync(this IDbAsyncEnumerable<int?> source, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		long sum = 0L;
		using (IDbAsyncEnumerator<int?> e = source.GetAsyncEnumerator())
		{
			while (await e.MoveNextAsync(cancellationToken).WithCurrentCulture())
			{
				cancellationToken.ThrowIfCancellationRequested();
				if (e.Current.HasValue)
				{
					sum = checked(sum + e.Current.GetValueOrDefault());
				}
			}
		}
		return (int)sum;
	}

	internal static Task<long> SumAsync(this IDbAsyncEnumerable<long> source)
	{
		return source.SumAsync(CancellationToken.None);
	}

	internal static async Task<long> SumAsync(this IDbAsyncEnumerable<long> source, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		long sum = 0L;
		using (IDbAsyncEnumerator<long> e = source.GetAsyncEnumerator())
		{
			while (await e.MoveNextAsync(cancellationToken).WithCurrentCulture())
			{
				cancellationToken.ThrowIfCancellationRequested();
				sum = checked(sum + e.Current);
			}
		}
		return sum;
	}

	internal static Task<long?> SumAsync(this IDbAsyncEnumerable<long?> source)
	{
		return source.SumAsync(CancellationToken.None);
	}

	internal static async Task<long?> SumAsync(this IDbAsyncEnumerable<long?> source, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		long sum = 0L;
		using (IDbAsyncEnumerator<long?> e = source.GetAsyncEnumerator())
		{
			while (await e.MoveNextAsync(cancellationToken).WithCurrentCulture())
			{
				cancellationToken.ThrowIfCancellationRequested();
				if (e.Current.HasValue)
				{
					sum = checked(sum + e.Current.GetValueOrDefault());
				}
			}
		}
		return sum;
	}

	internal static Task<float> SumAsync(this IDbAsyncEnumerable<float> source)
	{
		return source.SumAsync(CancellationToken.None);
	}

	internal static async Task<float> SumAsync(this IDbAsyncEnumerable<float> source, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		double sum = 0.0;
		using (IDbAsyncEnumerator<float> e = source.GetAsyncEnumerator())
		{
			while (await e.MoveNextAsync(cancellationToken).WithCurrentCulture())
			{
				cancellationToken.ThrowIfCancellationRequested();
				sum += (double)e.Current;
			}
		}
		return (float)sum;
	}

	internal static Task<float?> SumAsync(this IDbAsyncEnumerable<float?> source)
	{
		return source.SumAsync(CancellationToken.None);
	}

	internal static async Task<float?> SumAsync(this IDbAsyncEnumerable<float?> source, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		double sum = 0.0;
		using (IDbAsyncEnumerator<float?> e = source.GetAsyncEnumerator())
		{
			while (await e.MoveNextAsync(cancellationToken).WithCurrentCulture())
			{
				cancellationToken.ThrowIfCancellationRequested();
				if (e.Current.HasValue)
				{
					sum += (double)e.Current.GetValueOrDefault();
				}
			}
		}
		return (float)sum;
	}

	internal static Task<double> SumAsync(this IDbAsyncEnumerable<double> source)
	{
		return source.SumAsync(CancellationToken.None);
	}

	internal static async Task<double> SumAsync(this IDbAsyncEnumerable<double> source, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		double sum = 0.0;
		using (IDbAsyncEnumerator<double> e = source.GetAsyncEnumerator())
		{
			while (await e.MoveNextAsync(cancellationToken).WithCurrentCulture())
			{
				cancellationToken.ThrowIfCancellationRequested();
				sum += e.Current;
			}
		}
		return sum;
	}

	internal static Task<double?> SumAsync(this IDbAsyncEnumerable<double?> source)
	{
		return source.SumAsync(CancellationToken.None);
	}

	internal static async Task<double?> SumAsync(this IDbAsyncEnumerable<double?> source, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		double sum = 0.0;
		using (IDbAsyncEnumerator<double?> e = source.GetAsyncEnumerator())
		{
			while (await e.MoveNextAsync(cancellationToken).WithCurrentCulture())
			{
				cancellationToken.ThrowIfCancellationRequested();
				if (e.Current.HasValue)
				{
					sum += e.Current.GetValueOrDefault();
				}
			}
		}
		return sum;
	}

	internal static Task<decimal> SumAsync(this IDbAsyncEnumerable<decimal> source)
	{
		return source.SumAsync(CancellationToken.None);
	}

	internal static async Task<decimal> SumAsync(this IDbAsyncEnumerable<decimal> source, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		decimal sum = default(decimal);
		using (IDbAsyncEnumerator<decimal> e = source.GetAsyncEnumerator())
		{
			while (await e.MoveNextAsync(cancellationToken).WithCurrentCulture())
			{
				cancellationToken.ThrowIfCancellationRequested();
				sum += e.Current;
			}
		}
		return sum;
	}

	internal static Task<decimal?> SumAsync(this IDbAsyncEnumerable<decimal?> source)
	{
		return source.SumAsync(CancellationToken.None);
	}

	internal static async Task<decimal?> SumAsync(this IDbAsyncEnumerable<decimal?> source, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		decimal sum = default(decimal);
		using (IDbAsyncEnumerator<decimal?> e = source.GetAsyncEnumerator())
		{
			while (await e.MoveNextAsync(cancellationToken).WithCurrentCulture())
			{
				cancellationToken.ThrowIfCancellationRequested();
				if (e.Current.HasValue)
				{
					sum += e.Current.GetValueOrDefault();
				}
			}
		}
		return sum;
	}

	internal static Task<double> AverageAsync(this IDbAsyncEnumerable<int> source)
	{
		return source.AverageAsync(CancellationToken.None);
	}

	internal static async Task<double> AverageAsync(this IDbAsyncEnumerable<int> source, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		long sum = 0L;
		long count = 0L;
		checked
		{
			using (IDbAsyncEnumerator<int> e = source.GetAsyncEnumerator())
			{
				while (await e.MoveNextAsync(cancellationToken).WithCurrentCulture())
				{
					cancellationToken.ThrowIfCancellationRequested();
					sum += e.Current;
					count++;
				}
			}
			if (count > 0)
			{
				return (double)sum / (double)count;
			}
			throw Error.EmptySequence();
		}
	}

	internal static Task<double?> AverageAsync(this IDbAsyncEnumerable<int?> source)
	{
		return source.AverageAsync(CancellationToken.None);
	}

	internal static async Task<double?> AverageAsync(this IDbAsyncEnumerable<int?> source, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		long sum = 0L;
		long count = 0L;
		checked
		{
			using (IDbAsyncEnumerator<int?> e = source.GetAsyncEnumerator())
			{
				while (await e.MoveNextAsync(cancellationToken).WithCurrentCulture())
				{
					cancellationToken.ThrowIfCancellationRequested();
					if (e.Current.HasValue)
					{
						sum += e.Current.GetValueOrDefault();
						count++;
					}
				}
			}
			if (count > 0)
			{
				return (double)sum / (double)count;
			}
			throw Error.EmptySequence();
		}
	}

	internal static Task<double> AverageAsync(this IDbAsyncEnumerable<long> source)
	{
		return source.AverageAsync(CancellationToken.None);
	}

	internal static async Task<double> AverageAsync(this IDbAsyncEnumerable<long> source, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		long sum = 0L;
		long count = 0L;
		checked
		{
			using (IDbAsyncEnumerator<long> e = source.GetAsyncEnumerator())
			{
				while (await e.MoveNextAsync(cancellationToken).WithCurrentCulture())
				{
					cancellationToken.ThrowIfCancellationRequested();
					sum += e.Current;
					count++;
				}
			}
			if (count > 0)
			{
				return (double)sum / (double)count;
			}
			throw Error.EmptySequence();
		}
	}

	internal static Task<double?> AverageAsync(this IDbAsyncEnumerable<long?> source)
	{
		return source.AverageAsync(CancellationToken.None);
	}

	internal static async Task<double?> AverageAsync(this IDbAsyncEnumerable<long?> source, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		long sum = 0L;
		long count = 0L;
		checked
		{
			using (IDbAsyncEnumerator<long?> e = source.GetAsyncEnumerator())
			{
				while (await e.MoveNextAsync(cancellationToken).WithCurrentCulture())
				{
					cancellationToken.ThrowIfCancellationRequested();
					if (e.Current.HasValue)
					{
						sum += e.Current.GetValueOrDefault();
						count++;
					}
				}
			}
			if (count > 0)
			{
				return (double)sum / (double)count;
			}
			throw Error.EmptySequence();
		}
	}

	internal static Task<float> AverageAsync(this IDbAsyncEnumerable<float> source)
	{
		return source.AverageAsync(CancellationToken.None);
	}

	internal static async Task<float> AverageAsync(this IDbAsyncEnumerable<float> source, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		double sum = 0.0;
		long count = 0L;
		using (IDbAsyncEnumerator<float> e = source.GetAsyncEnumerator())
		{
			while (await e.MoveNextAsync(cancellationToken).WithCurrentCulture())
			{
				cancellationToken.ThrowIfCancellationRequested();
				sum += (double)e.Current;
				count = checked(count + 1);
			}
		}
		if (count > 0)
		{
			return (float)(sum / (double)count);
		}
		throw Error.EmptySequence();
	}

	internal static Task<float?> AverageAsync(this IDbAsyncEnumerable<float?> source)
	{
		return source.AverageAsync(CancellationToken.None);
	}

	internal static async Task<float?> AverageAsync(this IDbAsyncEnumerable<float?> source, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		double sum = 0.0;
		long count = 0L;
		using (IDbAsyncEnumerator<float?> e = source.GetAsyncEnumerator())
		{
			while (await e.MoveNextAsync(cancellationToken).WithCurrentCulture())
			{
				cancellationToken.ThrowIfCancellationRequested();
				if (e.Current.HasValue)
				{
					sum += (double)e.Current.GetValueOrDefault();
					count = checked(count + 1);
				}
			}
		}
		if (count > 0)
		{
			return (float)(sum / (double)count);
		}
		throw Error.EmptySequence();
	}

	internal static Task<double> AverageAsync(this IDbAsyncEnumerable<double> source)
	{
		return source.AverageAsync(CancellationToken.None);
	}

	internal static async Task<double> AverageAsync(this IDbAsyncEnumerable<double> source, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		double sum = 0.0;
		long count = 0L;
		using (IDbAsyncEnumerator<double> e = source.GetAsyncEnumerator())
		{
			while (await e.MoveNextAsync(cancellationToken).WithCurrentCulture())
			{
				cancellationToken.ThrowIfCancellationRequested();
				sum += e.Current;
				count = checked(count + 1);
			}
		}
		if (count > 0)
		{
			return (float)(sum / (double)count);
		}
		throw Error.EmptySequence();
	}

	internal static Task<double?> AverageAsync(this IDbAsyncEnumerable<double?> source)
	{
		return source.AverageAsync(CancellationToken.None);
	}

	internal static async Task<double?> AverageAsync(this IDbAsyncEnumerable<double?> source, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		double sum = 0.0;
		long count = 0L;
		using (IDbAsyncEnumerator<double?> e = source.GetAsyncEnumerator())
		{
			while (await e.MoveNextAsync(cancellationToken).WithCurrentCulture())
			{
				cancellationToken.ThrowIfCancellationRequested();
				if (e.Current.HasValue)
				{
					sum += e.Current.GetValueOrDefault();
					count = checked(count + 1);
				}
			}
		}
		if (count > 0)
		{
			return (float)(sum / (double)count);
		}
		throw Error.EmptySequence();
	}

	internal static Task<decimal> AverageAsync(this IDbAsyncEnumerable<decimal> source)
	{
		return source.AverageAsync(CancellationToken.None);
	}

	internal static async Task<decimal> AverageAsync(this IDbAsyncEnumerable<decimal> source, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		decimal sum = default(decimal);
		long count = 0L;
		using (IDbAsyncEnumerator<decimal> e = source.GetAsyncEnumerator())
		{
			while (await e.MoveNextAsync(cancellationToken).WithCurrentCulture())
			{
				cancellationToken.ThrowIfCancellationRequested();
				sum += e.Current;
				count = checked(count + 1);
			}
		}
		if (count > 0)
		{
			return sum / (decimal)count;
		}
		throw Error.EmptySequence();
	}

	internal static Task<decimal?> AverageAsync(this IDbAsyncEnumerable<decimal?> source)
	{
		return source.AverageAsync(CancellationToken.None);
	}

	internal static async Task<decimal?> AverageAsync(this IDbAsyncEnumerable<decimal?> source, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		decimal sum = default(decimal);
		long count = 0L;
		using (IDbAsyncEnumerator<decimal?> e = source.GetAsyncEnumerator())
		{
			while (await e.MoveNextAsync(cancellationToken).WithCurrentCulture())
			{
				cancellationToken.ThrowIfCancellationRequested();
				if (e.Current.HasValue)
				{
					sum += e.Current.GetValueOrDefault();
					count = checked(count + 1);
				}
			}
		}
		if (count > 0)
		{
			return sum / (decimal)count;
		}
		throw Error.EmptySequence();
	}
}
