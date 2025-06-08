using System.Collections.Generic;
using System.Threading;

namespace System.Data.Entity.Core.Common.Utils;

internal sealed class Memoizer<TArg, TResult>
{
	private class Result
	{
		private TResult _value;

		private Func<TResult> _delegate;

		internal Result(Func<TResult> createValueDelegate)
		{
			_delegate = createValueDelegate;
		}

		internal TResult GetValue()
		{
			if (_delegate == null)
			{
				return _value;
			}
			lock (this)
			{
				if (_delegate == null)
				{
					return _value;
				}
				_value = _delegate();
				_delegate = null;
				return _value;
			}
		}
	}

	private readonly Func<TArg, TResult> _function;

	private readonly Dictionary<TArg, Result> _resultCache;

	private readonly ReaderWriterLockSlim _lock;

	internal Memoizer(Func<TArg, TResult> function, IEqualityComparer<TArg> argComparer)
	{
		_function = function;
		_resultCache = new Dictionary<TArg, Result>(argComparer);
		_lock = new ReaderWriterLockSlim();
	}

	internal TResult Evaluate(TArg arg)
	{
		if (!TryGetResult(arg, out var result))
		{
			_lock.EnterWriteLock();
			try
			{
				if (!_resultCache.TryGetValue(arg, out result))
				{
					result = new Result(() => _function(arg));
					_resultCache.Add(arg, result);
				}
			}
			finally
			{
				_lock.ExitWriteLock();
			}
		}
		return result.GetValue();
	}

	internal bool TryGetValue(TArg arg, out TResult value)
	{
		if (TryGetResult(arg, out var result))
		{
			value = result.GetValue();
			return true;
		}
		value = default(TResult);
		return false;
	}

	private bool TryGetResult(TArg arg, out Result result)
	{
		_lock.EnterReadLock();
		try
		{
			return _resultCache.TryGetValue(arg, out result);
		}
		finally
		{
			_lock.ExitReadLock();
		}
	}
}
