using System.Collections;
using System.Collections.Generic;

namespace System.Data.Entity.Core.Common.EntitySql;

internal sealed class Scope : IEnumerable<KeyValuePair<string, ScopeEntry>>, IEnumerable
{
	private readonly Dictionary<string, ScopeEntry> _scopeEntries;

	internal Scope(IEqualityComparer<string> keyComparer)
	{
		_scopeEntries = new Dictionary<string, ScopeEntry>(keyComparer);
	}

	internal Scope Add(string key, ScopeEntry value)
	{
		_scopeEntries.Add(key, value);
		return this;
	}

	internal void Remove(string key)
	{
		_scopeEntries.Remove(key);
	}

	internal void Replace(string key, ScopeEntry value)
	{
		_scopeEntries[key] = value;
	}

	internal bool Contains(string key)
	{
		return _scopeEntries.ContainsKey(key);
	}

	internal bool TryLookup(string key, out ScopeEntry value)
	{
		return _scopeEntries.TryGetValue(key, out value);
	}

	public Dictionary<string, ScopeEntry>.Enumerator GetEnumerator()
	{
		return _scopeEntries.GetEnumerator();
	}

	IEnumerator<KeyValuePair<string, ScopeEntry>> IEnumerable<KeyValuePair<string, ScopeEntry>>.GetEnumerator()
	{
		return _scopeEntries.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _scopeEntries.GetEnumerator();
	}
}
