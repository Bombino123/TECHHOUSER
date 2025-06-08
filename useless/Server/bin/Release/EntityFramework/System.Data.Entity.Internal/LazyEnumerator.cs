using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;

namespace System.Data.Entity.Internal;

internal class LazyEnumerator<T> : IEnumerator<T>, IDisposable, IEnumerator
{
	private readonly Func<ObjectResult<T>> _getObjectResult;

	private IEnumerator<T> _objectResultEnumerator;

	public T Current
	{
		get
		{
			if (_objectResultEnumerator != null)
			{
				return _objectResultEnumerator.Current;
			}
			return default(T);
		}
	}

	object IEnumerator.Current => Current;

	public LazyEnumerator(Func<ObjectResult<T>> getObjectResult)
	{
		_getObjectResult = getObjectResult;
	}

	public void Dispose()
	{
		if (_objectResultEnumerator != null)
		{
			_objectResultEnumerator.Dispose();
		}
	}

	public bool MoveNext()
	{
		if (_objectResultEnumerator == null)
		{
			ObjectResult<T> objectResult = _getObjectResult();
			try
			{
				_objectResultEnumerator = objectResult.GetEnumerator();
			}
			catch
			{
				objectResult.Dispose();
				throw;
			}
		}
		return _objectResultEnumerator.MoveNext();
	}

	public void Reset()
	{
		if (_objectResultEnumerator != null)
		{
			_objectResultEnumerator.Reset();
		}
	}
}
