using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Resources;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal sealed class FilteredSchemaElementLookUpTable<T, S> : IEnumerable<T>, IEnumerable, ISchemaElementLookUpTable<T> where T : S where S : SchemaElement
{
	private readonly SchemaElementLookUpTable<S> _lookUpTable;

	public int Count
	{
		get
		{
			int num = 0;
			foreach (S item in _lookUpTable)
			{
				if (item is T)
				{
					num++;
				}
			}
			return num;
		}
	}

	public T this[string key]
	{
		get
		{
			S val = _lookUpTable[key];
			if (val == null)
			{
				return null;
			}
			if (val is T result)
			{
				return result;
			}
			throw new InvalidOperationException(Strings.UnexpectedTypeInCollection(val.GetType(), key));
		}
	}

	public FilteredSchemaElementLookUpTable(SchemaElementLookUpTable<S> lookUpTable)
	{
		_lookUpTable = lookUpTable;
	}

	public IEnumerator<T> GetEnumerator()
	{
		return _lookUpTable.GetFilteredEnumerator<T>();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _lookUpTable.GetFilteredEnumerator<T>();
	}

	public bool ContainsKey(string key)
	{
		if (!_lookUpTable.ContainsKey(key))
		{
			return false;
		}
		return _lookUpTable[key] as T != null;
	}

	public T LookUpEquivalentKey(string key)
	{
		return _lookUpTable.LookUpEquivalentKey(key) as T;
	}
}
