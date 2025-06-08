using System.Collections;
using System.Collections.Generic;

namespace System.Runtime.InteropServices;

internal class ComEnumerator<T, TIn> : IEnumerator<T>, IDisposable, IEnumerator where T : class where TIn : class
{
	protected readonly Func<TIn, T> converter;

	protected IEnumerator<TIn> iEnum;

	object IEnumerator.Current => Current;

	public virtual T Current
	{
		get
		{
			Func<TIn, T> func = converter;
			IEnumerator<TIn> enumerator = iEnum;
			return func((enumerator != null) ? enumerator.Current : null);
		}
	}

	public ComEnumerator(Func<int> getCount, Func<int, TIn> indexer, Func<TIn, T> converter)
	{
		this.converter = converter;
		iEnum = Enumerate();
		IEnumerator<TIn> Enumerate()
		{
			for (int x = 1; x <= getCount(); x++)
			{
				yield return indexer(x);
			}
		}
	}

	public ComEnumerator(Func<int> getCount, Func<object, TIn> indexer, Func<TIn, T> converter)
	{
		this.converter = converter;
		iEnum = Enumerate();
		IEnumerator<TIn> Enumerate()
		{
			for (int x = 1; x <= getCount(); x++)
			{
				yield return indexer(x);
			}
		}
	}

	public virtual void Dispose()
	{
		iEnum?.Dispose();
		iEnum = null;
	}

	public virtual bool MoveNext()
	{
		return iEnum?.MoveNext() ?? false;
	}

	public virtual void Reset()
	{
		iEnum?.Reset();
	}
}
