using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Internal;
using System.Data.Entity.Internal.Linq;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity;

public static class QueryableExtensions
{
	private static readonly MethodInfo _first = GetMethod("First", (Type T) => new Type[1] { typeof(IQueryable<>).MakeGenericType(T) });

	private static readonly MethodInfo _first_Predicate = GetMethod("First", (Type T) => new Type[2]
	{
		typeof(IQueryable<>).MakeGenericType(T),
		typeof(Expression<>).MakeGenericType(typeof(Func<, >).MakeGenericType(T, typeof(bool)))
	});

	private static readonly MethodInfo _firstOrDefault = GetMethod("FirstOrDefault", (Type T) => new Type[1] { typeof(IQueryable<>).MakeGenericType(T) });

	private static readonly MethodInfo _firstOrDefault_Predicate = GetMethod("FirstOrDefault", (Type T) => new Type[2]
	{
		typeof(IQueryable<>).MakeGenericType(T),
		typeof(Expression<>).MakeGenericType(typeof(Func<, >).MakeGenericType(T, typeof(bool)))
	});

	private static readonly MethodInfo _single = GetMethod("Single", (Type T) => new Type[1] { typeof(IQueryable<>).MakeGenericType(T) });

	private static readonly MethodInfo _single_Predicate = GetMethod("Single", (Type T) => new Type[2]
	{
		typeof(IQueryable<>).MakeGenericType(T),
		typeof(Expression<>).MakeGenericType(typeof(Func<, >).MakeGenericType(T, typeof(bool)))
	});

	private static readonly MethodInfo _singleOrDefault = GetMethod("SingleOrDefault", (Type T) => new Type[1] { typeof(IQueryable<>).MakeGenericType(T) });

	private static readonly MethodInfo _singleOrDefault_Predicate = GetMethod("SingleOrDefault", (Type T) => new Type[2]
	{
		typeof(IQueryable<>).MakeGenericType(T),
		typeof(Expression<>).MakeGenericType(typeof(Func<, >).MakeGenericType(T, typeof(bool)))
	});

	private static readonly MethodInfo _contains = GetMethod("Contains", (Type T) => new Type[2]
	{
		typeof(IQueryable<>).MakeGenericType(T),
		T
	});

	private static readonly MethodInfo _any = GetMethod("Any", (Type T) => new Type[1] { typeof(IQueryable<>).MakeGenericType(T) });

	private static readonly MethodInfo _any_Predicate = GetMethod("Any", (Type T) => new Type[2]
	{
		typeof(IQueryable<>).MakeGenericType(T),
		typeof(Expression<>).MakeGenericType(typeof(Func<, >).MakeGenericType(T, typeof(bool)))
	});

	private static readonly MethodInfo _all_Predicate = GetMethod("All", (Type T) => new Type[2]
	{
		typeof(IQueryable<>).MakeGenericType(T),
		typeof(Expression<>).MakeGenericType(typeof(Func<, >).MakeGenericType(T, typeof(bool)))
	});

	private static readonly MethodInfo _count = GetMethod("Count", (Type T) => new Type[1] { typeof(IQueryable<>).MakeGenericType(T) });

	private static readonly MethodInfo _count_Predicate = GetMethod("Count", (Type T) => new Type[2]
	{
		typeof(IQueryable<>).MakeGenericType(T),
		typeof(Expression<>).MakeGenericType(typeof(Func<, >).MakeGenericType(T, typeof(bool)))
	});

	private static readonly MethodInfo _longCount = GetMethod("LongCount", (Type T) => new Type[1] { typeof(IQueryable<>).MakeGenericType(T) });

	private static readonly MethodInfo _longCount_Predicate = GetMethod("LongCount", (Type T) => new Type[2]
	{
		typeof(IQueryable<>).MakeGenericType(T),
		typeof(Expression<>).MakeGenericType(typeof(Func<, >).MakeGenericType(T, typeof(bool)))
	});

	private static readonly MethodInfo _min = GetMethod("Min", (Type T) => new Type[1] { typeof(IQueryable<>).MakeGenericType(T) });

	private static readonly MethodInfo _min_Selector = GetMethod("Min", (Type T, Type U) => new Type[2]
	{
		typeof(IQueryable<>).MakeGenericType(T),
		typeof(Expression<>).MakeGenericType(typeof(Func<, >).MakeGenericType(T, U))
	});

	private static readonly MethodInfo _max = GetMethod("Max", (Type T) => new Type[1] { typeof(IQueryable<>).MakeGenericType(T) });

	private static readonly MethodInfo _max_Selector = GetMethod("Max", (Type T, Type U) => new Type[2]
	{
		typeof(IQueryable<>).MakeGenericType(T),
		typeof(Expression<>).MakeGenericType(typeof(Func<, >).MakeGenericType(T, U))
	});

	private static readonly MethodInfo _sum_Int = GetMethod("Sum", () => new Type[1] { typeof(IQueryable<int>) });

	private static readonly MethodInfo _sum_IntNullable = GetMethod("Sum", () => new Type[1] { typeof(IQueryable<int?>) });

	private static readonly MethodInfo _sum_Long = GetMethod("Sum", () => new Type[1] { typeof(IQueryable<long>) });

	private static readonly MethodInfo _sum_LongNullable = GetMethod("Sum", () => new Type[1] { typeof(IQueryable<long?>) });

	private static readonly MethodInfo _sum_Float = GetMethod("Sum", () => new Type[1] { typeof(IQueryable<float>) });

	private static readonly MethodInfo _sum_FloatNullable = GetMethod("Sum", () => new Type[1] { typeof(IQueryable<float?>) });

	private static readonly MethodInfo _sum_Double = GetMethod("Sum", () => new Type[1] { typeof(IQueryable<double>) });

	private static readonly MethodInfo _sum_DoubleNullable = GetMethod("Sum", () => new Type[1] { typeof(IQueryable<double?>) });

	private static readonly MethodInfo _sum_Decimal = GetMethod("Sum", () => new Type[1] { typeof(IQueryable<decimal>) });

	private static readonly MethodInfo _sum_DecimalNullable = GetMethod("Sum", () => new Type[1] { typeof(IQueryable<decimal?>) });

	private static readonly MethodInfo _sum_Int_Selector = GetMethod("Sum", (Type T) => new Type[2]
	{
		typeof(IQueryable<>).MakeGenericType(T),
		typeof(Expression<>).MakeGenericType(typeof(Func<, >).MakeGenericType(T, typeof(int)))
	});

	private static readonly MethodInfo _sum_IntNullable_Selector = GetMethod("Sum", (Type T) => new Type[2]
	{
		typeof(IQueryable<>).MakeGenericType(T),
		typeof(Expression<>).MakeGenericType(typeof(Func<, >).MakeGenericType(T, typeof(int?)))
	});

	private static readonly MethodInfo _sum_Long_Selector = GetMethod("Sum", (Type T) => new Type[2]
	{
		typeof(IQueryable<>).MakeGenericType(T),
		typeof(Expression<>).MakeGenericType(typeof(Func<, >).MakeGenericType(T, typeof(long)))
	});

	private static readonly MethodInfo _sum_LongNullable_Selector = GetMethod("Sum", (Type T) => new Type[2]
	{
		typeof(IQueryable<>).MakeGenericType(T),
		typeof(Expression<>).MakeGenericType(typeof(Func<, >).MakeGenericType(T, typeof(long?)))
	});

	private static readonly MethodInfo _sum_Float_Selector = GetMethod("Sum", (Type T) => new Type[2]
	{
		typeof(IQueryable<>).MakeGenericType(T),
		typeof(Expression<>).MakeGenericType(typeof(Func<, >).MakeGenericType(T, typeof(float)))
	});

	private static readonly MethodInfo _sum_FloatNullable_Selector = GetMethod("Sum", (Type T) => new Type[2]
	{
		typeof(IQueryable<>).MakeGenericType(T),
		typeof(Expression<>).MakeGenericType(typeof(Func<, >).MakeGenericType(T, typeof(float?)))
	});

	private static readonly MethodInfo _sum_Double_Selector = GetMethod("Sum", (Type T) => new Type[2]
	{
		typeof(IQueryable<>).MakeGenericType(T),
		typeof(Expression<>).MakeGenericType(typeof(Func<, >).MakeGenericType(T, typeof(double)))
	});

	private static readonly MethodInfo _sum_DoubleNullable_Selector = GetMethod("Sum", (Type T) => new Type[2]
	{
		typeof(IQueryable<>).MakeGenericType(T),
		typeof(Expression<>).MakeGenericType(typeof(Func<, >).MakeGenericType(T, typeof(double?)))
	});

	private static readonly MethodInfo _sum_Decimal_Selector = GetMethod("Sum", (Type T) => new Type[2]
	{
		typeof(IQueryable<>).MakeGenericType(T),
		typeof(Expression<>).MakeGenericType(typeof(Func<, >).MakeGenericType(T, typeof(decimal)))
	});

	private static readonly MethodInfo _sum_DecimalNullable_Selector = GetMethod("Sum", (Type T) => new Type[2]
	{
		typeof(IQueryable<>).MakeGenericType(T),
		typeof(Expression<>).MakeGenericType(typeof(Func<, >).MakeGenericType(T, typeof(decimal?)))
	});

	private static readonly MethodInfo _average_Int = GetMethod("Average", () => new Type[1] { typeof(IQueryable<int>) });

	private static readonly MethodInfo _average_IntNullable = GetMethod("Average", () => new Type[1] { typeof(IQueryable<int?>) });

	private static readonly MethodInfo _average_Long = GetMethod("Average", () => new Type[1] { typeof(IQueryable<long>) });

	private static readonly MethodInfo _average_LongNullable = GetMethod("Average", () => new Type[1] { typeof(IQueryable<long?>) });

	private static readonly MethodInfo _average_Float = GetMethod("Average", () => new Type[1] { typeof(IQueryable<float>) });

	private static readonly MethodInfo _average_FloatNullable = GetMethod("Average", () => new Type[1] { typeof(IQueryable<float?>) });

	private static readonly MethodInfo _average_Double = GetMethod("Average", () => new Type[1] { typeof(IQueryable<double>) });

	private static readonly MethodInfo _average_DoubleNullable = GetMethod("Average", () => new Type[1] { typeof(IQueryable<double?>) });

	private static readonly MethodInfo _average_Decimal = GetMethod("Average", () => new Type[1] { typeof(IQueryable<decimal>) });

	private static readonly MethodInfo _average_DecimalNullable = GetMethod("Average", () => new Type[1] { typeof(IQueryable<decimal?>) });

	private static readonly MethodInfo _average_Int_Selector = GetMethod("Average", (Type T) => new Type[2]
	{
		typeof(IQueryable<>).MakeGenericType(T),
		typeof(Expression<>).MakeGenericType(typeof(Func<, >).MakeGenericType(T, typeof(int)))
	});

	private static readonly MethodInfo _average_IntNullable_Selector = GetMethod("Average", (Type T) => new Type[2]
	{
		typeof(IQueryable<>).MakeGenericType(T),
		typeof(Expression<>).MakeGenericType(typeof(Func<, >).MakeGenericType(T, typeof(int?)))
	});

	private static readonly MethodInfo _average_Long_Selector = GetMethod("Average", (Type T) => new Type[2]
	{
		typeof(IQueryable<>).MakeGenericType(T),
		typeof(Expression<>).MakeGenericType(typeof(Func<, >).MakeGenericType(T, typeof(long)))
	});

	private static readonly MethodInfo _average_LongNullable_Selector = GetMethod("Average", (Type T) => new Type[2]
	{
		typeof(IQueryable<>).MakeGenericType(T),
		typeof(Expression<>).MakeGenericType(typeof(Func<, >).MakeGenericType(T, typeof(long?)))
	});

	private static readonly MethodInfo _average_Float_Selector = GetMethod("Average", (Type T) => new Type[2]
	{
		typeof(IQueryable<>).MakeGenericType(T),
		typeof(Expression<>).MakeGenericType(typeof(Func<, >).MakeGenericType(T, typeof(float)))
	});

	private static readonly MethodInfo _average_FloatNullable_Selector = GetMethod("Average", (Type T) => new Type[2]
	{
		typeof(IQueryable<>).MakeGenericType(T),
		typeof(Expression<>).MakeGenericType(typeof(Func<, >).MakeGenericType(T, typeof(float?)))
	});

	private static readonly MethodInfo _average_Double_Selector = GetMethod("Average", (Type T) => new Type[2]
	{
		typeof(IQueryable<>).MakeGenericType(T),
		typeof(Expression<>).MakeGenericType(typeof(Func<, >).MakeGenericType(T, typeof(double)))
	});

	private static readonly MethodInfo _average_DoubleNullable_Selector = GetMethod("Average", (Type T) => new Type[2]
	{
		typeof(IQueryable<>).MakeGenericType(T),
		typeof(Expression<>).MakeGenericType(typeof(Func<, >).MakeGenericType(T, typeof(double?)))
	});

	private static readonly MethodInfo _average_Decimal_Selector = GetMethod("Average", (Type T) => new Type[2]
	{
		typeof(IQueryable<>).MakeGenericType(T),
		typeof(Expression<>).MakeGenericType(typeof(Func<, >).MakeGenericType(T, typeof(decimal)))
	});

	private static readonly MethodInfo _average_DecimalNullable_Selector = GetMethod("Average", (Type T) => new Type[2]
	{
		typeof(IQueryable<>).MakeGenericType(T),
		typeof(Expression<>).MakeGenericType(typeof(Func<, >).MakeGenericType(T, typeof(decimal?)))
	});

	private static readonly MethodInfo _skip = GetMethod("Skip", (Type T) => new Type[2]
	{
		typeof(IQueryable<>).MakeGenericType(T),
		typeof(int)
	});

	private static readonly MethodInfo _take = GetMethod("Take", (Type T) => new Type[2]
	{
		typeof(IQueryable<>).MakeGenericType(T),
		typeof(int)
	});

	public static IQueryable<T> Include<T>(this IQueryable<T> source, string path)
	{
		Check.NotNull(source, "source");
		Check.NotEmpty(path, "path");
		if (source is DbQuery<T> dbQuery)
		{
			return dbQuery.Include(path);
		}
		if (source is ObjectQuery<T> objectQuery)
		{
			return objectQuery.Include(path);
		}
		return CommonInclude(source, path);
	}

	public static IQueryable Include(this IQueryable source, string path)
	{
		Check.NotNull(source, "source");
		Check.NotEmpty(path, "path");
		if (!(source is DbQuery dbQuery))
		{
			return CommonInclude(source, path);
		}
		return dbQuery.Include(path);
	}

	private static T CommonInclude<T>(T source, string path)
	{
		MethodInfo runtimeMethod = source.GetType().GetRuntimeMethod("Include", (MethodInfo p) => p.IsPublic && !p.IsStatic, new Type[1] { typeof(string) }, new Type[1] { typeof(IComparable) }, new Type[1] { typeof(ICloneable) }, new Type[1] { typeof(IComparable<string>) }, new Type[1] { typeof(IEnumerable<char>) }, new Type[1] { typeof(IEnumerable) }, new Type[1] { typeof(IEquatable<string>) }, new Type[1] { typeof(object) });
		if (runtimeMethod != null && typeof(T).IsAssignableFrom(runtimeMethod.ReturnType))
		{
			return (T)runtimeMethod.Invoke(source, new object[1] { path });
		}
		return source;
	}

	public static IQueryable<T> Include<T, TProperty>(this IQueryable<T> source, Expression<Func<T, TProperty>> path)
	{
		Check.NotNull(source, "source");
		Check.NotNull(path, "path");
		if (!DbHelpers.TryParsePath(path.Body, out var path2) || path2 == null)
		{
			throw new ArgumentException(Strings.DbExtensions_InvalidIncludePathExpression, "path");
		}
		return source.Include(path2);
	}

	public static IQueryable<T> AsNoTracking<T>(this IQueryable<T> source) where T : class
	{
		Check.NotNull(source, "source");
		if (!(source is DbQuery<T> dbQuery))
		{
			return CommonAsNoTracking(source);
		}
		return dbQuery.AsNoTracking();
	}

	public static IQueryable AsNoTracking(this IQueryable source)
	{
		Check.NotNull(source, "source");
		if (!(source is DbQuery dbQuery))
		{
			return CommonAsNoTracking(source);
		}
		return dbQuery.AsNoTracking();
	}

	private static T CommonAsNoTracking<T>(T source) where T : class
	{
		if (source is ObjectQuery query)
		{
			return (T)DbHelpers.CreateNoTrackingQuery(query);
		}
		MethodInfo publicInstanceMethod = source.GetType().GetPublicInstanceMethod("AsNoTracking");
		if (publicInstanceMethod != null && typeof(T).IsAssignableFrom(publicInstanceMethod.ReturnType))
		{
			return (T)publicInstanceMethod.Invoke(source, null);
		}
		return source;
	}

	[Obsolete("LINQ queries are now streaming by default unless a retrying ExecutionStrategy is used. Calling this method will have no effect.")]
	public static IQueryable<T> AsStreaming<T>(this IQueryable<T> source)
	{
		Check.NotNull(source, "source");
		if (!(source is DbQuery<T> dbQuery))
		{
			return CommonAsStreaming(source);
		}
		return dbQuery.AsStreaming();
	}

	[Obsolete("LINQ queries are now streaming by default unless a retrying ExecutionStrategy is used. Calling this method will have no effect.")]
	public static IQueryable AsStreaming(this IQueryable source)
	{
		Check.NotNull(source, "source");
		if (!(source is DbQuery dbQuery))
		{
			return CommonAsStreaming(source);
		}
		return dbQuery.AsStreaming();
	}

	private static T CommonAsStreaming<T>(T source) where T : class
	{
		if (source is ObjectQuery query)
		{
			return (T)DbHelpers.CreateStreamingQuery(query);
		}
		MethodInfo publicInstanceMethod = source.GetType().GetPublicInstanceMethod("AsStreaming");
		if (publicInstanceMethod != null && typeof(T).IsAssignableFrom(publicInstanceMethod.ReturnType))
		{
			return (T)publicInstanceMethod.Invoke(source, null);
		}
		return source;
	}

	internal static IQueryable<T> WithExecutionStrategy<T>(this IQueryable<T> source, IDbExecutionStrategy executionStrategy)
	{
		Check.NotNull(source, "source");
		if (!(source is DbQuery<T> dbQuery))
		{
			return CommonWithExecutionStrategy(source, executionStrategy);
		}
		return dbQuery.WithExecutionStrategy(executionStrategy);
	}

	internal static IQueryable WithExecutionStrategy(this IQueryable source, IDbExecutionStrategy executionStrategy)
	{
		Check.NotNull(source, "source");
		if (!(source is DbQuery dbQuery))
		{
			return CommonWithExecutionStrategy(source, executionStrategy);
		}
		return dbQuery.WithExecutionStrategy(executionStrategy);
	}

	private static T CommonWithExecutionStrategy<T>(T source, IDbExecutionStrategy executionStrategy) where T : class
	{
		if (source is ObjectQuery query)
		{
			return (T)DbHelpers.CreateQueryWithExecutionStrategy(query, executionStrategy);
		}
		MethodInfo publicInstanceMethod = source.GetType().GetPublicInstanceMethod("WithExecutionStrategy");
		if (publicInstanceMethod != null && typeof(T).IsAssignableFrom(publicInstanceMethod.ReturnType))
		{
			return (T)publicInstanceMethod.Invoke(source, new object[1] { executionStrategy });
		}
		return source;
	}

	public static void Load(this IQueryable source)
	{
		Check.NotNull(source, "source");
		IEnumerator enumerator = source.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
			}
		}
		finally
		{
			IDisposable disposable = enumerator as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
	}

	public static Task LoadAsync(this IQueryable source)
	{
		Check.NotNull(source, "source");
		return source.LoadAsync(CancellationToken.None);
	}

	public static Task LoadAsync(this IQueryable source, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		return source.ForEachAsync(delegate
		{
		}, cancellationToken);
	}

	public static Task ForEachAsync(this IQueryable source, Action<object> action)
	{
		Check.NotNull(source, "source");
		Check.NotNull(action, "action");
		return source.AsDbAsyncEnumerable().ForEachAsync(action, CancellationToken.None);
	}

	public static Task ForEachAsync(this IQueryable source, Action<object> action, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		Check.NotNull(action, "action");
		return source.AsDbAsyncEnumerable().ForEachAsync(action, cancellationToken);
	}

	public static Task ForEachAsync<T>(this IQueryable<T> source, Action<T> action)
	{
		Check.NotNull(source, "source");
		Check.NotNull(action, "action");
		return source.AsDbAsyncEnumerable().ForEachAsync(action, CancellationToken.None);
	}

	public static Task ForEachAsync<T>(this IQueryable<T> source, Action<T> action, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		Check.NotNull(action, "action");
		return source.AsDbAsyncEnumerable().ForEachAsync(action, cancellationToken);
	}

	public static Task<List<object>> ToListAsync(this IQueryable source)
	{
		Check.NotNull(source, "source");
		return source.AsDbAsyncEnumerable().ToListAsync<object>();
	}

	public static Task<List<object>> ToListAsync(this IQueryable source, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		return source.AsDbAsyncEnumerable().ToListAsync<object>(cancellationToken);
	}

	public static Task<List<TSource>> ToListAsync<TSource>(this IQueryable<TSource> source)
	{
		Check.NotNull(source, "source");
		return source.AsDbAsyncEnumerable().ToListAsync();
	}

	public static Task<List<TSource>> ToListAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		return source.AsDbAsyncEnumerable().ToListAsync(cancellationToken);
	}

	public static Task<TSource[]> ToArrayAsync<TSource>(this IQueryable<TSource> source)
	{
		Check.NotNull(source, "source");
		return source.AsDbAsyncEnumerable().ToArrayAsync();
	}

	public static Task<TSource[]> ToArrayAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		return source.AsDbAsyncEnumerable().ToArrayAsync(cancellationToken);
	}

	public static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(this IQueryable<TSource> source, Func<TSource, TKey> keySelector)
	{
		Check.NotNull(source, "source");
		Check.NotNull(keySelector, "keySelector");
		return source.AsDbAsyncEnumerable().ToDictionaryAsync(keySelector);
	}

	public static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(this IQueryable<TSource> source, Func<TSource, TKey> keySelector, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		Check.NotNull(keySelector, "keySelector");
		return source.AsDbAsyncEnumerable().ToDictionaryAsync(keySelector, cancellationToken);
	}

	public static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(this IQueryable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
	{
		Check.NotNull(source, "source");
		Check.NotNull(keySelector, "keySelector");
		return source.AsDbAsyncEnumerable().ToDictionaryAsync(keySelector, comparer);
	}

	public static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(this IQueryable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		Check.NotNull(keySelector, "keySelector");
		return source.AsDbAsyncEnumerable().ToDictionaryAsync(keySelector, comparer, cancellationToken);
	}

	public static Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(this IQueryable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
	{
		Check.NotNull(source, "source");
		Check.NotNull(keySelector, "keySelector");
		Check.NotNull(elementSelector, "elementSelector");
		return source.AsDbAsyncEnumerable().ToDictionaryAsync(keySelector, elementSelector);
	}

	public static Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(this IQueryable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		Check.NotNull(keySelector, "keySelector");
		Check.NotNull(elementSelector, "elementSelector");
		return source.AsDbAsyncEnumerable().ToDictionaryAsync(keySelector, elementSelector, cancellationToken);
	}

	public static Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(this IQueryable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
	{
		Check.NotNull(source, "source");
		Check.NotNull(keySelector, "keySelector");
		Check.NotNull(elementSelector, "elementSelector");
		return source.AsDbAsyncEnumerable().ToDictionaryAsync(keySelector, elementSelector, comparer);
	}

	public static Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(this IQueryable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		Check.NotNull(keySelector, "keySelector");
		Check.NotNull(elementSelector, "elementSelector");
		return source.AsDbAsyncEnumerable().ToDictionaryAsync(keySelector, elementSelector, comparer, cancellationToken);
	}

	public static Task<TSource> FirstAsync<TSource>(this IQueryable<TSource> source)
	{
		Check.NotNull(source, "source");
		return source.FirstAsync(CancellationToken.None);
	}

	public static Task<TSource> FirstAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<TSource>(Expression.Call(null, _first.MakeGenericMethod(typeof(TSource)), source.Expression), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<TSource> FirstAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
	{
		Check.NotNull(source, "source");
		Check.NotNull(predicate, "predicate");
		return source.FirstAsync(predicate, CancellationToken.None);
	}

	public static Task<TSource> FirstAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		Check.NotNull(predicate, "predicate");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<TSource>(Expression.Call(null, _first_Predicate.MakeGenericMethod(typeof(TSource)), new Expression[2]
			{
				source.Expression,
				Expression.Quote(predicate)
			}), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source)
	{
		Check.NotNull(source, "source");
		return source.FirstOrDefaultAsync(CancellationToken.None);
	}

	public static Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<TSource>(Expression.Call(null, _firstOrDefault.MakeGenericMethod(typeof(TSource)), source.Expression), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
	{
		Check.NotNull(source, "source");
		Check.NotNull(predicate, "predicate");
		return source.FirstOrDefaultAsync(predicate, CancellationToken.None);
	}

	public static Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		Check.NotNull(predicate, "predicate");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<TSource>(Expression.Call(null, _firstOrDefault_Predicate.MakeGenericMethod(typeof(TSource)), new Expression[2]
			{
				source.Expression,
				Expression.Quote(predicate)
			}), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<TSource> SingleAsync<TSource>(this IQueryable<TSource> source)
	{
		Check.NotNull(source, "source");
		return source.SingleAsync(CancellationToken.None);
	}

	public static Task<TSource> SingleAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<TSource>(Expression.Call(null, _single.MakeGenericMethod(typeof(TSource)), source.Expression), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<TSource> SingleAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
	{
		Check.NotNull(source, "source");
		Check.NotNull(predicate, "predicate");
		return source.SingleAsync(predicate, CancellationToken.None);
	}

	public static Task<TSource> SingleAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		Check.NotNull(predicate, "predicate");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<TSource>(Expression.Call(null, _single_Predicate.MakeGenericMethod(typeof(TSource)), new Expression[2]
			{
				source.Expression,
				Expression.Quote(predicate)
			}), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<TSource> SingleOrDefaultAsync<TSource>(this IQueryable<TSource> source)
	{
		Check.NotNull(source, "source");
		return source.SingleOrDefaultAsync(CancellationToken.None);
	}

	public static Task<TSource> SingleOrDefaultAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<TSource>(Expression.Call(null, _singleOrDefault.MakeGenericMethod(typeof(TSource)), source.Expression), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<TSource> SingleOrDefaultAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
	{
		Check.NotNull(source, "source");
		Check.NotNull(predicate, "predicate");
		return source.SingleOrDefaultAsync(predicate, CancellationToken.None);
	}

	public static Task<TSource> SingleOrDefaultAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		Check.NotNull(predicate, "predicate");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<TSource>(Expression.Call(null, _singleOrDefault_Predicate.MakeGenericMethod(typeof(TSource)), new Expression[2]
			{
				source.Expression,
				Expression.Quote(predicate)
			}), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<bool> ContainsAsync<TSource>(this IQueryable<TSource> source, TSource item)
	{
		Check.NotNull(source, "source");
		return source.ContainsAsync(item, CancellationToken.None);
	}

	public static Task<bool> ContainsAsync<TSource>(this IQueryable<TSource> source, TSource item, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<bool>(Expression.Call(null, _contains.MakeGenericMethod(typeof(TSource)), new Expression[2]
			{
				source.Expression,
				Expression.Constant(item, typeof(TSource))
			}), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<bool> AnyAsync<TSource>(this IQueryable<TSource> source)
	{
		Check.NotNull(source, "source");
		return source.AnyAsync(CancellationToken.None);
	}

	public static Task<bool> AnyAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<bool>(Expression.Call(null, _any.MakeGenericMethod(typeof(TSource)), source.Expression), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<bool> AnyAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
	{
		Check.NotNull(source, "source");
		Check.NotNull(predicate, "predicate");
		return source.AnyAsync(predicate, CancellationToken.None);
	}

	public static Task<bool> AnyAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		Check.NotNull(predicate, "predicate");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<bool>(Expression.Call(null, _any_Predicate.MakeGenericMethod(typeof(TSource)), new Expression[2]
			{
				source.Expression,
				Expression.Quote(predicate)
			}), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<bool> AllAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
	{
		Check.NotNull(source, "source");
		Check.NotNull(predicate, "predicate");
		return source.AllAsync(predicate, CancellationToken.None);
	}

	public static Task<bool> AllAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		Check.NotNull(predicate, "predicate");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<bool>(Expression.Call(null, _all_Predicate.MakeGenericMethod(typeof(TSource)), new Expression[2]
			{
				source.Expression,
				Expression.Quote(predicate)
			}), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<int> CountAsync<TSource>(this IQueryable<TSource> source)
	{
		Check.NotNull(source, "source");
		return source.CountAsync(CancellationToken.None);
	}

	public static Task<int> CountAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<int>(Expression.Call(null, _count.MakeGenericMethod(typeof(TSource)), source.Expression), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<int> CountAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
	{
		Check.NotNull(source, "source");
		Check.NotNull(predicate, "predicate");
		return source.CountAsync(predicate, CancellationToken.None);
	}

	public static Task<int> CountAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		Check.NotNull(predicate, "predicate");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<int>(Expression.Call(null, _count_Predicate.MakeGenericMethod(typeof(TSource)), new Expression[2]
			{
				source.Expression,
				Expression.Quote(predicate)
			}), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<long> LongCountAsync<TSource>(this IQueryable<TSource> source)
	{
		Check.NotNull(source, "source");
		return source.LongCountAsync(CancellationToken.None);
	}

	public static Task<long> LongCountAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<long>(Expression.Call(null, _longCount.MakeGenericMethod(typeof(TSource)), source.Expression), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<long> LongCountAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
	{
		Check.NotNull(source, "source");
		Check.NotNull(predicate, "predicate");
		return source.LongCountAsync(predicate, CancellationToken.None);
	}

	public static Task<long> LongCountAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		Check.NotNull(predicate, "predicate");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<long>(Expression.Call(null, _longCount_Predicate.MakeGenericMethod(typeof(TSource)), new Expression[2]
			{
				source.Expression,
				Expression.Quote(predicate)
			}), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<TSource> MinAsync<TSource>(this IQueryable<TSource> source)
	{
		Check.NotNull(source, "source");
		return source.MinAsync(CancellationToken.None);
	}

	public static Task<TSource> MinAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<TSource>(Expression.Call(null, _min.MakeGenericMethod(typeof(TSource)), source.Expression), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<TResult> MinAsync<TSource, TResult>(this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selector)
	{
		Check.NotNull(source, "source");
		Check.NotNull(selector, "selector");
		return source.MinAsync(selector, CancellationToken.None);
	}

	public static Task<TResult> MinAsync<TSource, TResult>(this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selector, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		Check.NotNull(selector, "selector");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<TResult>(Expression.Call(null, _min_Selector.MakeGenericMethod(typeof(TSource), typeof(TResult)), new Expression[2]
			{
				source.Expression,
				Expression.Quote(selector)
			}), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<TSource> MaxAsync<TSource>(this IQueryable<TSource> source)
	{
		Check.NotNull(source, "source");
		return source.MaxAsync(CancellationToken.None);
	}

	public static Task<TSource> MaxAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<TSource>(Expression.Call(null, _max.MakeGenericMethod(typeof(TSource)), source.Expression), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<TResult> MaxAsync<TSource, TResult>(this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selector)
	{
		Check.NotNull(source, "source");
		Check.NotNull(selector, "selector");
		return source.MaxAsync(selector, CancellationToken.None);
	}

	public static Task<TResult> MaxAsync<TSource, TResult>(this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selector, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		Check.NotNull(selector, "selector");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<TResult>(Expression.Call(null, _max_Selector.MakeGenericMethod(typeof(TSource), typeof(TResult)), new Expression[2]
			{
				source.Expression,
				Expression.Quote(selector)
			}), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<int> SumAsync(this IQueryable<int> source)
	{
		Check.NotNull(source, "source");
		return source.SumAsync(CancellationToken.None);
	}

	public static Task<int> SumAsync(this IQueryable<int> source, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<int>(Expression.Call(null, _sum_Int, source.Expression), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<int?> SumAsync(this IQueryable<int?> source)
	{
		Check.NotNull(source, "source");
		return source.SumAsync(CancellationToken.None);
	}

	public static Task<int?> SumAsync(this IQueryable<int?> source, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<int?>(Expression.Call(null, _sum_IntNullable, source.Expression), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<long> SumAsync(this IQueryable<long> source)
	{
		Check.NotNull(source, "source");
		return source.SumAsync(CancellationToken.None);
	}

	public static Task<long> SumAsync(this IQueryable<long> source, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<long>(Expression.Call(null, _sum_Long, source.Expression), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<long?> SumAsync(this IQueryable<long?> source)
	{
		Check.NotNull(source, "source");
		return source.SumAsync(CancellationToken.None);
	}

	public static Task<long?> SumAsync(this IQueryable<long?> source, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<long?>(Expression.Call(null, _sum_LongNullable, source.Expression), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<float> SumAsync(this IQueryable<float> source)
	{
		Check.NotNull(source, "source");
		return source.SumAsync(CancellationToken.None);
	}

	public static Task<float> SumAsync(this IQueryable<float> source, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<float>(Expression.Call(null, _sum_Float, source.Expression), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<float?> SumAsync(this IQueryable<float?> source)
	{
		Check.NotNull(source, "source");
		return source.SumAsync(CancellationToken.None);
	}

	public static Task<float?> SumAsync(this IQueryable<float?> source, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<float?>(Expression.Call(null, _sum_FloatNullable, source.Expression), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<double> SumAsync(this IQueryable<double> source)
	{
		Check.NotNull(source, "source");
		return source.SumAsync(CancellationToken.None);
	}

	public static Task<double> SumAsync(this IQueryable<double> source, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<double>(Expression.Call(null, _sum_Double, source.Expression), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<double?> SumAsync(this IQueryable<double?> source)
	{
		Check.NotNull(source, "source");
		return source.SumAsync(CancellationToken.None);
	}

	public static Task<double?> SumAsync(this IQueryable<double?> source, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<double?>(Expression.Call(null, _sum_DoubleNullable, source.Expression), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<decimal> SumAsync(this IQueryable<decimal> source)
	{
		Check.NotNull(source, "source");
		return source.SumAsync(CancellationToken.None);
	}

	public static Task<decimal> SumAsync(this IQueryable<decimal> source, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<decimal>(Expression.Call(null, _sum_Decimal, source.Expression), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<decimal?> SumAsync(this IQueryable<decimal?> source)
	{
		Check.NotNull(source, "source");
		return source.SumAsync(CancellationToken.None);
	}

	public static Task<decimal?> SumAsync(this IQueryable<decimal?> source, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<decimal?>(Expression.Call(null, _sum_DecimalNullable, source.Expression), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<int> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, int>> selector)
	{
		Check.NotNull(source, "source");
		Check.NotNull(selector, "selector");
		return source.SumAsync(selector, CancellationToken.None);
	}

	public static Task<int> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, int>> selector, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		Check.NotNull(selector, "selector");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<int>(Expression.Call(null, _sum_Int_Selector.MakeGenericMethod(typeof(TSource)), new Expression[2]
			{
				source.Expression,
				Expression.Quote(selector)
			}), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<int?> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, int?>> selector)
	{
		Check.NotNull(source, "source");
		Check.NotNull(selector, "selector");
		return source.SumAsync(selector, CancellationToken.None);
	}

	public static Task<int?> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, int?>> selector, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		Check.NotNull(selector, "selector");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<int?>(Expression.Call(null, _sum_IntNullable_Selector.MakeGenericMethod(typeof(TSource)), new Expression[2]
			{
				source.Expression,
				Expression.Quote(selector)
			}), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<long> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, long>> selector)
	{
		Check.NotNull(source, "source");
		Check.NotNull(selector, "selector");
		return source.SumAsync(selector, CancellationToken.None);
	}

	public static Task<long> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, long>> selector, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		Check.NotNull(selector, "selector");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<long>(Expression.Call(null, _sum_Long_Selector.MakeGenericMethod(typeof(TSource)), new Expression[2]
			{
				source.Expression,
				Expression.Quote(selector)
			}), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<long?> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, long?>> selector)
	{
		Check.NotNull(source, "source");
		Check.NotNull(selector, "selector");
		return source.SumAsync(selector, CancellationToken.None);
	}

	public static Task<long?> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, long?>> selector, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		Check.NotNull(selector, "selector");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<long?>(Expression.Call(null, _sum_LongNullable_Selector.MakeGenericMethod(typeof(TSource)), new Expression[2]
			{
				source.Expression,
				Expression.Quote(selector)
			}), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<float> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, float>> selector)
	{
		Check.NotNull(source, "source");
		Check.NotNull(selector, "selector");
		return source.SumAsync(selector, CancellationToken.None);
	}

	public static Task<float> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, float>> selector, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		Check.NotNull(selector, "selector");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<float>(Expression.Call(null, _sum_Float_Selector.MakeGenericMethod(typeof(TSource)), new Expression[2]
			{
				source.Expression,
				Expression.Quote(selector)
			}), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<float?> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, float?>> selector)
	{
		Check.NotNull(source, "source");
		Check.NotNull(selector, "selector");
		return source.SumAsync(selector, CancellationToken.None);
	}

	public static Task<float?> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, float?>> selector, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		Check.NotNull(selector, "selector");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<float?>(Expression.Call(null, _sum_FloatNullable_Selector.MakeGenericMethod(typeof(TSource)), new Expression[2]
			{
				source.Expression,
				Expression.Quote(selector)
			}), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<double> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, double>> selector)
	{
		Check.NotNull(source, "source");
		Check.NotNull(selector, "selector");
		return source.SumAsync(selector, CancellationToken.None);
	}

	public static Task<double> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, double>> selector, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		Check.NotNull(selector, "selector");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<double>(Expression.Call(null, _sum_Double_Selector.MakeGenericMethod(typeof(TSource)), new Expression[2]
			{
				source.Expression,
				Expression.Quote(selector)
			}), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<double?> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, double?>> selector)
	{
		Check.NotNull(source, "source");
		Check.NotNull(selector, "selector");
		return source.SumAsync(selector, CancellationToken.None);
	}

	public static Task<double?> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, double?>> selector, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		Check.NotNull(selector, "selector");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<double?>(Expression.Call(null, _sum_DoubleNullable_Selector.MakeGenericMethod(typeof(TSource)), new Expression[2]
			{
				source.Expression,
				Expression.Quote(selector)
			}), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<decimal> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, decimal>> selector)
	{
		Check.NotNull(source, "source");
		Check.NotNull(selector, "selector");
		return source.SumAsync(selector, CancellationToken.None);
	}

	public static Task<decimal> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, decimal>> selector, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		Check.NotNull(selector, "selector");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<decimal>(Expression.Call(null, _sum_Decimal_Selector.MakeGenericMethod(typeof(TSource)), new Expression[2]
			{
				source.Expression,
				Expression.Quote(selector)
			}), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<decimal?> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, decimal?>> selector)
	{
		Check.NotNull(source, "source");
		Check.NotNull(selector, "selector");
		return source.SumAsync(selector, CancellationToken.None);
	}

	public static Task<decimal?> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, decimal?>> selector, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		Check.NotNull(selector, "selector");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<decimal?>(Expression.Call(null, _sum_DecimalNullable_Selector.MakeGenericMethod(typeof(TSource)), new Expression[2]
			{
				source.Expression,
				Expression.Quote(selector)
			}), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<double> AverageAsync(this IQueryable<int> source)
	{
		Check.NotNull(source, "source");
		return source.AverageAsync(CancellationToken.None);
	}

	public static Task<double> AverageAsync(this IQueryable<int> source, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<double>(Expression.Call(null, _average_Int, source.Expression), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<double?> AverageAsync(this IQueryable<int?> source)
	{
		Check.NotNull(source, "source");
		return source.AverageAsync(CancellationToken.None);
	}

	public static Task<double?> AverageAsync(this IQueryable<int?> source, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<double?>(Expression.Call(null, _average_IntNullable, source.Expression), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<double> AverageAsync(this IQueryable<long> source)
	{
		Check.NotNull(source, "source");
		return source.AverageAsync(CancellationToken.None);
	}

	public static Task<double> AverageAsync(this IQueryable<long> source, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<double>(Expression.Call(null, _average_Long, source.Expression), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<double?> AverageAsync(this IQueryable<long?> source)
	{
		Check.NotNull(source, "source");
		return source.AverageAsync(CancellationToken.None);
	}

	public static Task<double?> AverageAsync(this IQueryable<long?> source, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<double?>(Expression.Call(null, _average_LongNullable, source.Expression), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<float> AverageAsync(this IQueryable<float> source)
	{
		Check.NotNull(source, "source");
		return source.AverageAsync(CancellationToken.None);
	}

	public static Task<float> AverageAsync(this IQueryable<float> source, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<float>(Expression.Call(null, _average_Float, source.Expression), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<float?> AverageAsync(this IQueryable<float?> source)
	{
		Check.NotNull(source, "source");
		return source.AverageAsync(CancellationToken.None);
	}

	public static Task<float?> AverageAsync(this IQueryable<float?> source, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<float?>(Expression.Call(null, _average_FloatNullable, source.Expression), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<double> AverageAsync(this IQueryable<double> source)
	{
		Check.NotNull(source, "source");
		return source.AverageAsync(CancellationToken.None);
	}

	public static Task<double> AverageAsync(this IQueryable<double> source, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<double>(Expression.Call(null, _average_Double, source.Expression), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<double?> AverageAsync(this IQueryable<double?> source)
	{
		Check.NotNull(source, "source");
		return source.AverageAsync(CancellationToken.None);
	}

	public static Task<double?> AverageAsync(this IQueryable<double?> source, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<double?>(Expression.Call(null, _average_DoubleNullable, source.Expression), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<decimal> AverageAsync(this IQueryable<decimal> source)
	{
		Check.NotNull(source, "source");
		return source.AverageAsync(CancellationToken.None);
	}

	public static Task<decimal> AverageAsync(this IQueryable<decimal> source, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<decimal>(Expression.Call(null, _average_Decimal, source.Expression), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<decimal?> AverageAsync(this IQueryable<decimal?> source)
	{
		Check.NotNull(source, "source");
		return source.AverageAsync(CancellationToken.None);
	}

	public static Task<decimal?> AverageAsync(this IQueryable<decimal?> source, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<decimal?>(Expression.Call(null, _average_DecimalNullable, source.Expression), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<double> AverageAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, int>> selector)
	{
		Check.NotNull(source, "source");
		Check.NotNull(selector, "selector");
		return source.AverageAsync(selector, CancellationToken.None);
	}

	public static Task<double> AverageAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, int>> selector, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		Check.NotNull(selector, "selector");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<double>(Expression.Call(null, _average_Int_Selector.MakeGenericMethod(typeof(TSource)), new Expression[2]
			{
				source.Expression,
				Expression.Quote(selector)
			}), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<double?> AverageAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, int?>> selector)
	{
		Check.NotNull(source, "source");
		Check.NotNull(selector, "selector");
		return source.AverageAsync(selector, CancellationToken.None);
	}

	public static Task<double?> AverageAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, int?>> selector, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		Check.NotNull(selector, "selector");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<double?>(Expression.Call(null, _average_IntNullable_Selector.MakeGenericMethod(typeof(TSource)), new Expression[2]
			{
				source.Expression,
				Expression.Quote(selector)
			}), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<double> AverageAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, long>> selector)
	{
		Check.NotNull(source, "source");
		Check.NotNull(selector, "selector");
		return source.AverageAsync(selector, CancellationToken.None);
	}

	public static Task<double> AverageAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, long>> selector, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		Check.NotNull(selector, "selector");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<double>(Expression.Call(null, _average_Long_Selector.MakeGenericMethod(typeof(TSource)), new Expression[2]
			{
				source.Expression,
				Expression.Quote(selector)
			}), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<double?> AverageAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, long?>> selector)
	{
		Check.NotNull(source, "source");
		Check.NotNull(selector, "selector");
		return source.AverageAsync(selector, CancellationToken.None);
	}

	public static Task<double?> AverageAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, long?>> selector, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		Check.NotNull(selector, "selector");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<double?>(Expression.Call(null, _average_LongNullable_Selector.MakeGenericMethod(typeof(TSource)), new Expression[2]
			{
				source.Expression,
				Expression.Quote(selector)
			}), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<float> AverageAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, float>> selector)
	{
		Check.NotNull(source, "source");
		Check.NotNull(selector, "selector");
		return source.AverageAsync(selector, CancellationToken.None);
	}

	public static Task<float> AverageAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, float>> selector, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		Check.NotNull(selector, "selector");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<float>(Expression.Call(null, _average_Float_Selector.MakeGenericMethod(typeof(TSource)), new Expression[2]
			{
				source.Expression,
				Expression.Quote(selector)
			}), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<float?> AverageAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, float?>> selector)
	{
		Check.NotNull(source, "source");
		Check.NotNull(selector, "selector");
		return source.AverageAsync(selector, CancellationToken.None);
	}

	public static Task<float?> AverageAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, float?>> selector, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		Check.NotNull(selector, "selector");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<float?>(Expression.Call(null, _average_FloatNullable_Selector.MakeGenericMethod(typeof(TSource)), new Expression[2]
			{
				source.Expression,
				Expression.Quote(selector)
			}), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<double> AverageAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, double>> selector)
	{
		Check.NotNull(source, "source");
		Check.NotNull(selector, "selector");
		return source.AverageAsync(selector, CancellationToken.None);
	}

	public static Task<double> AverageAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, double>> selector, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		Check.NotNull(selector, "selector");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<double>(Expression.Call(null, _average_Double_Selector.MakeGenericMethod(typeof(TSource)), new Expression[2]
			{
				source.Expression,
				Expression.Quote(selector)
			}), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<double?> AverageAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, double?>> selector)
	{
		Check.NotNull(source, "source");
		Check.NotNull(selector, "selector");
		return source.AverageAsync(selector, CancellationToken.None);
	}

	public static Task<double?> AverageAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, double?>> selector, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		Check.NotNull(selector, "selector");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<double?>(Expression.Call(null, _average_DoubleNullable_Selector.MakeGenericMethod(typeof(TSource)), new Expression[2]
			{
				source.Expression,
				Expression.Quote(selector)
			}), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<decimal> AverageAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, decimal>> selector)
	{
		Check.NotNull(source, "source");
		Check.NotNull(selector, "selector");
		return source.AverageAsync(selector, CancellationToken.None);
	}

	public static Task<decimal> AverageAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, decimal>> selector, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		Check.NotNull(selector, "selector");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<decimal>(Expression.Call(null, _average_Decimal_Selector.MakeGenericMethod(typeof(TSource)), new Expression[2]
			{
				source.Expression,
				Expression.Quote(selector)
			}), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static Task<decimal?> AverageAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, decimal?>> selector)
	{
		Check.NotNull(source, "source");
		Check.NotNull(selector, "selector");
		return source.AverageAsync(selector, CancellationToken.None);
	}

	public static Task<decimal?> AverageAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, decimal?>> selector, CancellationToken cancellationToken)
	{
		Check.NotNull(source, "source");
		Check.NotNull(selector, "selector");
		cancellationToken.ThrowIfCancellationRequested();
		if (source.Provider is IDbAsyncQueryProvider dbAsyncQueryProvider)
		{
			return dbAsyncQueryProvider.ExecuteAsync<decimal?>(Expression.Call(null, _average_DecimalNullable_Selector.MakeGenericMethod(typeof(TSource)), new Expression[2]
			{
				source.Expression,
				Expression.Quote(selector)
			}), cancellationToken);
		}
		throw Error.IQueryable_Provider_Not_Async();
	}

	public static IQueryable<TSource> Skip<TSource>(this IQueryable<TSource> source, Expression<Func<int>> countAccessor)
	{
		Check.NotNull(source, "source");
		Check.NotNull(countAccessor, "countAccessor");
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, _skip.MakeGenericMethod(typeof(TSource)), new Expression[2] { source.Expression, countAccessor.Body }));
	}

	public static IQueryable<TSource> Take<TSource>(this IQueryable<TSource> source, Expression<Func<int>> countAccessor)
	{
		Check.NotNull(source, "source");
		Check.NotNull(countAccessor, "countAccessor");
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, _take.MakeGenericMethod(typeof(TSource)), new Expression[2] { source.Expression, countAccessor.Body }));
	}

	internal static ObjectQuery TryGetObjectQuery(this IQueryable source)
	{
		if (source == null)
		{
			return null;
		}
		if (source is ObjectQuery result)
		{
			return result;
		}
		if (source is IInternalQueryAdapter internalQueryAdapter)
		{
			return internalQueryAdapter.InternalQuery.ObjectQuery;
		}
		return null;
	}

	private static IDbAsyncEnumerable AsDbAsyncEnumerable(this IQueryable source)
	{
		if (source is IDbAsyncEnumerable result)
		{
			return result;
		}
		throw Error.IQueryable_Not_Async(string.Empty);
	}

	private static IDbAsyncEnumerable<T> AsDbAsyncEnumerable<T>(this IQueryable<T> source)
	{
		if (source is IDbAsyncEnumerable<T> result)
		{
			return result;
		}
		throw Error.IQueryable_Not_Async("<" + typeof(T)?.ToString() + ">");
	}

	private static MethodInfo GetMethod(string methodName, Func<Type[]> getParameterTypes)
	{
		return GetMethod(methodName, getParameterTypes, 0);
	}

	private static MethodInfo GetMethod(string methodName, Func<Type, Type, Type[]> getParameterTypes)
	{
		return GetMethod(methodName, getParameterTypes, 2);
	}

	private static MethodInfo GetMethod(string methodName, Func<Type, Type[]> getParameterTypes)
	{
		return GetMethod(methodName, getParameterTypes, 1);
	}

	private static MethodInfo GetMethod(string methodName, Delegate getParameterTypesDelegate, int genericArgumentsCount)
	{
		foreach (MethodInfo declaredMethod in typeof(Queryable).GetDeclaredMethods(methodName))
		{
			Type[] genericArguments = declaredMethod.GetGenericArguments();
			if (genericArguments.Length == genericArgumentsCount)
			{
				object[] args = genericArguments;
				if (Matches(declaredMethod, (Type[])getParameterTypesDelegate.DynamicInvoke(args)))
				{
					return declaredMethod;
				}
			}
		}
		return null;
	}

	private static bool Matches(MethodInfo methodInfo, Type[] parameterTypes)
	{
		return (from p in methodInfo.GetParameters()
			select p.ParameterType).SequenceEqual(parameterTypes);
	}

	private static string PrettyPrint(MethodInfo getParameterTypesMethod, int genericArgumentsCount)
	{
		Type[] array = new Type[genericArgumentsCount];
		for (int i = 0; i < genericArgumentsCount; i++)
		{
			array[i] = typeof(object);
		}
		object[] parameters = array;
		Type[] array2 = (Type[])getParameterTypesMethod.Invoke(null, parameters);
		string[] array3 = new string[array2.Length];
		for (int j = 0; j < array2.Length; j++)
		{
			array3[j] = array2[j].ToString();
		}
		return "(" + string.Join(", ", array3) + ")";
	}
}
