using System.Collections;
using System.Collections.Generic;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal sealed class SchemaElementLookUpTableEnumerator<T, S> : IEnumerator<T>, IDisposable, IEnumerator where T : S where S : SchemaElement
{
	private readonly Dictionary<string, S> _data;

	private List<string>.Enumerator _enumerator;

	public T Current
	{
		get
		{
			string current = _enumerator.Current;
			return _data[current] as T;
		}
	}

	object IEnumerator.Current
	{
		get
		{
			string current = _enumerator.Current;
			return _data[current] as T;
		}
	}

	public SchemaElementLookUpTableEnumerator(Dictionary<string, S> data, List<string> keysInOrder)
	{
		_data = data;
		_enumerator = keysInOrder.GetEnumerator();
	}

	public void Reset()
	{
		((IEnumerator)_enumerator).Reset();
	}

	public bool MoveNext()
	{
		while (_enumerator.MoveNext())
		{
			if (Current != null)
			{
				return true;
			}
		}
		return false;
	}

	public void Dispose()
	{
	}
}
