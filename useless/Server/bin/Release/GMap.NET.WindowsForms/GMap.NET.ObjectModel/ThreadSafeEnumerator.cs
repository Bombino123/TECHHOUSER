using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace GMap.NET.ObjectModel;

public class ThreadSafeEnumerator<T> : IEnumerator<T>, IDisposable, IEnumerator
{
	private readonly IEnumerator<T> _inner;

	private readonly object _lock;

	public T Current => _inner.Current;

	object IEnumerator.Current => Current;

	public ThreadSafeEnumerator(IEnumerator<T> inner, object @lock)
	{
		_inner = inner;
		_lock = @lock;
		Monitor.Enter(_lock);
	}

	public void Dispose()
	{
		Monitor.Exit(_lock);
	}

	public bool MoveNext()
	{
		return _inner.MoveNext();
	}

	public void Reset()
	{
		_inner.Reset();
	}
}
