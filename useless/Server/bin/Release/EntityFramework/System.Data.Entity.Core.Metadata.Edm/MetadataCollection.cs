using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Runtime.CompilerServices;

namespace System.Data.Entity.Core.Metadata.Edm;

internal class MetadataCollection<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable where T : MetadataItem
{
	internal const int UseDictionaryCrossover = 8;

	private bool _readOnly;

	private List<T> _metadataList;

	private volatile Dictionary<string, T> _caseSensitiveDictionary;

	private volatile Dictionary<string, int> _caseInsensitiveDictionary;

	public virtual int Count => _metadataList.Count;

	public virtual T this[int index]
	{
		get
		{
			return _metadataList[index];
		}
		set
		{
			ThrowIfReadOnly();
			string identity = _metadataList[index].Identity;
			_metadataList[index] = value;
			HandleIdentityChange(value, identity, validate: false);
		}
	}

	public virtual T this[string identity]
	{
		get
		{
			return GetValue(identity, ignoreCase: false);
		}
		set
		{
			throw new InvalidOperationException(Strings.OperationOnReadOnlyCollection);
		}
	}

	public virtual ReadOnlyCollection<T> AsReadOnly => new ReadOnlyCollection<T>(_metadataList);

	public bool IsReadOnly => _readOnly;

	internal bool HasCaseSensitiveDictionary => _caseSensitiveDictionary != null;

	internal bool HasCaseInsensitiveDictionary => _caseInsensitiveDictionary != null;

	internal MetadataCollection()
	{
		_metadataList = new List<T>();
	}

	internal MetadataCollection(IEnumerable<T> items)
	{
		_metadataList = new List<T>();
		if (items == null)
		{
			return;
		}
		foreach (T item in items)
		{
			if (item == null)
			{
				throw new ArgumentException(Strings.ADP_CollectionParameterElementIsNull("items"));
			}
			AddInternal(item);
		}
	}

	private MetadataCollection(List<T> items)
	{
		_metadataList = items;
	}

	internal static MetadataCollection<T> Wrap(List<T> items)
	{
		return new MetadataCollection<T>(items);
	}

	internal void HandleIdentityChange(T item, string initialIdentity)
	{
		HandleIdentityChange(item, initialIdentity, validate: true);
	}

	private void HandleIdentityChange(T item, string initialIdentity, bool validate)
	{
		if (_caseSensitiveDictionary != null && (!validate || (_caseSensitiveDictionary.TryGetValue(initialIdentity, out var value) && value == item)))
		{
			RemoveFromCaseSensitiveDictionary(initialIdentity);
			string identity = item.Identity;
			if (_caseSensitiveDictionary.ContainsKey(identity))
			{
				_caseSensitiveDictionary = null;
			}
			else
			{
				_caseSensitiveDictionary.Add(identity, item);
			}
		}
		_caseInsensitiveDictionary = null;
	}

	public virtual T GetValue(string identity, bool ignoreCase)
	{
		if (!TryGetValue(identity, ignoreCase, out var item))
		{
			throw new ArgumentException(Strings.ItemInvalidIdentity(identity), "identity");
		}
		return item;
	}

	public virtual bool TryGetValue(string identity, bool ignoreCase, out T item)
	{
		if (!ignoreCase)
		{
			return FindCaseSensitive(identity, out item);
		}
		return FindCaseInsensitive(identity, out item, throwOnMultipleMatches: false);
	}

	public virtual void Add(T item)
	{
		ThrowIfReadOnly();
		AddInternal(item);
	}

	private void AddInternal(T item)
	{
		string identity = item.Identity;
		if (ContainsIdentityCaseSensitive(identity))
		{
			throw new ArgumentException(Strings.ItemDuplicateIdentity(identity), "item");
		}
		_metadataList.Add(item);
		if (_caseSensitiveDictionary != null)
		{
			_caseSensitiveDictionary.Add(identity, item);
		}
		_caseInsensitiveDictionary = null;
	}

	internal void AddRange(IEnumerable<T> items)
	{
		Check.NotNull(items, "items");
		foreach (T item in items)
		{
			if (item == null)
			{
				throw new ArgumentException(Strings.ADP_CollectionParameterElementIsNull("items"));
			}
			AddInternal(item);
		}
	}

	internal bool Remove(T item)
	{
		ThrowIfReadOnly();
		if (!_metadataList.Remove(item))
		{
			return false;
		}
		if (_caseSensitiveDictionary != null)
		{
			RemoveFromCaseSensitiveDictionary(item.Identity);
		}
		_caseInsensitiveDictionary = null;
		return true;
	}

	public virtual ReadOnlyMetadataCollection<T> AsReadOnlyMetadataCollection()
	{
		return new ReadOnlyMetadataCollection<T>(this);
	}

	internal void ResetReadOnly()
	{
		_readOnly = false;
	}

	public MetadataCollection<T> SetReadOnly()
	{
		for (int i = 0; i < _metadataList.Count; i++)
		{
			_metadataList[i].SetReadOnly();
		}
		_readOnly = true;
		_metadataList.TrimExcess();
		if (_metadataList.Count <= 8)
		{
			_caseSensitiveDictionary = null;
			_caseInsensitiveDictionary = null;
		}
		return this;
	}

	void IList<T>.Insert(int index, T item)
	{
		throw new InvalidOperationException(Strings.OperationOnReadOnlyCollection);
	}

	bool ICollection<T>.Remove(T item)
	{
		throw new InvalidOperationException(Strings.OperationOnReadOnlyCollection);
	}

	void IList<T>.RemoveAt(int index)
	{
		throw new InvalidOperationException(Strings.OperationOnReadOnlyCollection);
	}

	void ICollection<T>.Clear()
	{
		throw new InvalidOperationException(Strings.OperationOnReadOnlyCollection);
	}

	public bool Contains(T item)
	{
		if (TryGetValue(item.Identity, ignoreCase: false, out var item2))
		{
			return item2 == item;
		}
		return false;
	}

	public virtual bool ContainsIdentity(string identity)
	{
		return ContainsIdentityCaseSensitive(identity);
	}

	public virtual int IndexOf(T item)
	{
		return _metadataList.IndexOf(item);
	}

	public virtual void CopyTo(T[] array, int arrayIndex)
	{
		_metadataList.CopyTo(array, arrayIndex);
	}

	public ReadOnlyMetadataCollection<T>.Enumerator GetEnumerator()
	{
		return new ReadOnlyMetadataCollection<T>.Enumerator(this);
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	internal void InvalidateCache()
	{
		_caseSensitiveDictionary = null;
		_caseInsensitiveDictionary = null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal Dictionary<string, T> GetCaseSensitiveDictionary()
	{
		if (_caseSensitiveDictionary == null && _metadataList.Count > 8)
		{
			_caseSensitiveDictionary = CreateCaseSensitiveDictionary();
		}
		return _caseSensitiveDictionary;
	}

	private Dictionary<string, T> CreateCaseSensitiveDictionary()
	{
		Dictionary<string, T> dictionary = new Dictionary<string, T>(_metadataList.Count, StringComparer.Ordinal);
		for (int i = 0; i < _metadataList.Count; i++)
		{
			T val = _metadataList[i];
			dictionary.Add(val.Identity, val);
		}
		return dictionary;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal Dictionary<string, int> GetCaseInsensitiveDictionary()
	{
		if (_caseInsensitiveDictionary == null && _metadataList.Count > 8)
		{
			_caseInsensitiveDictionary = CreateCaseInsensitiveDictionary();
		}
		return _caseInsensitiveDictionary;
	}

	private Dictionary<string, int> CreateCaseInsensitiveDictionary()
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>(_metadataList.Count, StringComparer.OrdinalIgnoreCase) { 
		{
			_metadataList[0].Identity,
			0
		} };
		for (int i = 1; i < _metadataList.Count; i++)
		{
			string identity = _metadataList[i].Identity;
			if (!dictionary.TryGetValue(identity, out var value))
			{
				dictionary[identity] = i;
			}
			else if (value >= 0)
			{
				dictionary[identity] = -1;
			}
		}
		return dictionary;
	}

	private bool ContainsIdentityCaseSensitive(string identity)
	{
		return GetCaseSensitiveDictionary()?.ContainsKey(identity) ?? ListContainsIdentityCaseSensitive(identity);
	}

	private bool ListContainsIdentityCaseSensitive(string identity)
	{
		for (int i = 0; i < _metadataList.Count; i++)
		{
			if (_metadataList[i].Identity.Equals(identity, StringComparison.Ordinal))
			{
				return true;
			}
		}
		return false;
	}

	private bool FindCaseSensitive(string identity, out T item)
	{
		Dictionary<string, T> caseSensitiveDictionary = GetCaseSensitiveDictionary();
		if (caseSensitiveDictionary != null)
		{
			if (caseSensitiveDictionary.TryGetValue(identity, out item))
			{
				return true;
			}
			return false;
		}
		return ListFindCaseSensitive(identity, out item);
	}

	private bool ListFindCaseSensitive(string identity, out T item)
	{
		for (int i = 0; i < _metadataList.Count; i++)
		{
			T val = _metadataList[i];
			if (val.Identity.Equals(identity, StringComparison.Ordinal))
			{
				item = val;
				return true;
			}
		}
		item = null;
		return false;
	}

	private bool FindCaseInsensitive(string identity, out T item, bool throwOnMultipleMatches)
	{
		Dictionary<string, int> caseInsensitiveDictionary = GetCaseInsensitiveDictionary();
		if (caseInsensitiveDictionary != null)
		{
			if (caseInsensitiveDictionary.TryGetValue(identity, out var value))
			{
				if (value >= 0)
				{
					item = _metadataList[value];
					return true;
				}
				if (throwOnMultipleMatches)
				{
					throw new InvalidOperationException(Strings.MoreThanOneItemMatchesIdentity(identity));
				}
			}
			item = null;
			return false;
		}
		return ListFindCaseInsensitive(identity, out item, throwOnMultipleMatches);
	}

	private bool ListFindCaseInsensitive(string identity, out T item, bool throwOnMultipleMatches)
	{
		bool flag = false;
		item = null;
		for (int i = 0; i < _metadataList.Count; i++)
		{
			T val = _metadataList[i];
			if (!val.Identity.Equals(identity, StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}
			if (flag)
			{
				if (throwOnMultipleMatches)
				{
					throw new InvalidOperationException(Strings.MoreThanOneItemMatchesIdentity(identity));
				}
				item = null;
				return false;
			}
			flag = true;
			item = val;
		}
		return flag;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void RemoveFromCaseSensitiveDictionary(string identity)
	{
		_caseSensitiveDictionary.Remove(identity);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ThrowIfReadOnly()
	{
		if (IsReadOnly)
		{
			throw new InvalidOperationException(Strings.OperationOnReadOnlyCollection);
		}
	}
}
