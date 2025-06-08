using System.Collections;
using System.Collections.Generic;

namespace System.Data.Entity.Core.Common.Utils;

internal class DisposableCollectionWrapper<T> : IDisposable, IEnumerable<T>, IEnumerable where T : IDisposable
{
	private readonly IEnumerable<T> _enumerable;

	internal DisposableCollectionWrapper(IEnumerable<T> enumerable)
	{
		_enumerable = enumerable;
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
		if (_enumerable == null)
		{
			return;
		}
		foreach (T item in _enumerable)
		{
			item?.Dispose();
		}
	}

	public IEnumerator<T> GetEnumerator()
	{
		return _enumerable.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)_enumerable).GetEnumerator();
	}
}
