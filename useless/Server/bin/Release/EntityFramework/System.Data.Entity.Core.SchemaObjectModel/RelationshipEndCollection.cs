using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal sealed class RelationshipEndCollection : IList<IRelationshipEnd>, ICollection<IRelationshipEnd>, IEnumerable<IRelationshipEnd>, IEnumerable
{
	private sealed class Enumerator : IEnumerator<IRelationshipEnd>, IDisposable, IEnumerator
	{
		private List<string>.Enumerator _Enumerator;

		private readonly Dictionary<string, IRelationshipEnd> _Data;

		public IRelationshipEnd Current => _Data[_Enumerator.Current];

		object IEnumerator.Current => _Data[_Enumerator.Current];

		public Enumerator(Dictionary<string, IRelationshipEnd> data, List<string> keysInDefOrder)
		{
			_Enumerator = keysInDefOrder.GetEnumerator();
			_Data = data;
		}

		public void Reset()
		{
			((IEnumerator)_Enumerator).Reset();
		}

		public bool MoveNext()
		{
			return _Enumerator.MoveNext();
		}

		public void Dispose()
		{
		}
	}

	private Dictionary<string, IRelationshipEnd> _endLookup;

	private List<string> _keysInDefOrder;

	public int Count => KeysInDefOrder.Count;

	public IRelationshipEnd this[int index]
	{
		get
		{
			return EndLookup[KeysInDefOrder[index]];
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	private Dictionary<string, IRelationshipEnd> EndLookup
	{
		get
		{
			if (_endLookup == null)
			{
				_endLookup = new Dictionary<string, IRelationshipEnd>(StringComparer.Ordinal);
			}
			return _endLookup;
		}
	}

	private List<string> KeysInDefOrder
	{
		get
		{
			if (_keysInDefOrder == null)
			{
				_keysInDefOrder = new List<string>();
			}
			return _keysInDefOrder;
		}
	}

	public bool IsReadOnly => false;

	public void Add(IRelationshipEnd end)
	{
		SchemaElement end2 = end as SchemaElement;
		if (IsEndValid(end) && ValidateUniqueName(end2, end.Name))
		{
			EndLookup.Add(end.Name, end);
			KeysInDefOrder.Add(end.Name);
		}
	}

	private static bool IsEndValid(IRelationshipEnd end)
	{
		return !string.IsNullOrEmpty(end.Name);
	}

	private bool ValidateUniqueName(SchemaElement end, string name)
	{
		if (EndLookup.ContainsKey(name))
		{
			end.AddError(ErrorCode.AlreadyDefined, EdmSchemaErrorSeverity.Error, Strings.EndNameAlreadyDefinedDuplicate(name));
			return false;
		}
		return true;
	}

	public bool Remove(IRelationshipEnd end)
	{
		if (!IsEndValid(end))
		{
			return false;
		}
		KeysInDefOrder.Remove(end.Name);
		return EndLookup.Remove(end.Name);
	}

	public bool Contains(string name)
	{
		return EndLookup.ContainsKey(name);
	}

	public bool Contains(IRelationshipEnd end)
	{
		return Contains(end.Name);
	}

	public IEnumerator<IRelationshipEnd> GetEnumerator()
	{
		return new Enumerator(EndLookup, KeysInDefOrder);
	}

	public bool TryGetEnd(string name, out IRelationshipEnd end)
	{
		return EndLookup.TryGetValue(name, out end);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(EndLookup, KeysInDefOrder);
	}

	public void Clear()
	{
		EndLookup.Clear();
		KeysInDefOrder.Clear();
	}

	int IList<IRelationshipEnd>.IndexOf(IRelationshipEnd end)
	{
		throw new NotSupportedException();
	}

	void IList<IRelationshipEnd>.Insert(int index, IRelationshipEnd end)
	{
		throw new NotSupportedException();
	}

	void IList<IRelationshipEnd>.RemoveAt(int index)
	{
		throw new NotSupportedException();
	}

	public void CopyTo(IRelationshipEnd[] ends, int index)
	{
		using IEnumerator<IRelationshipEnd> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			IRelationshipEnd current = enumerator.Current;
			ends[index++] = current;
		}
	}
}
