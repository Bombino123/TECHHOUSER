using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace System.Data.Entity.Core.Common.Utils;

internal class Set<TElement> : InternalBase, IEnumerable<TElement>, IEnumerable
{
	public class Enumerator : IEnumerator<TElement>, IDisposable, IEnumerator
	{
		private Dictionary<TElement, bool>.KeyCollection.Enumerator keys;

		public TElement Current => keys.Current;

		object IEnumerator.Current => ((IEnumerator)keys).Current;

		internal Enumerator(Dictionary<TElement, bool>.KeyCollection.Enumerator keys)
		{
			this.keys = keys;
		}

		public void Dispose()
		{
			keys.Dispose();
		}

		public bool MoveNext()
		{
			return keys.MoveNext();
		}

		void IEnumerator.Reset()
		{
			((IEnumerator)keys).Reset();
		}
	}

	internal static readonly Set<TElement> Empty = new Set<TElement>().MakeReadOnly();

	private readonly HashSet<TElement> _values;

	private bool _isReadOnly;

	internal int Count => _values.Count;

	internal IEqualityComparer<TElement> Comparer => _values.Comparer;

	internal Set(Set<TElement> other)
		: this((IEnumerable<TElement>)other._values, other.Comparer)
	{
	}

	internal Set()
		: this((IEnumerable<TElement>)null, (IEqualityComparer<TElement>)null)
	{
	}

	internal Set(IEnumerable<TElement> elements)
		: this(elements, (IEqualityComparer<TElement>)null)
	{
	}

	internal Set(IEqualityComparer<TElement> comparer)
		: this((IEnumerable<TElement>)null, comparer)
	{
	}

	internal Set(IEnumerable<TElement> elements, IEqualityComparer<TElement> comparer)
	{
		_values = new HashSet<TElement>(elements ?? Enumerable.Empty<TElement>(), comparer ?? EqualityComparer<TElement>.Default);
	}

	internal bool Contains(TElement element)
	{
		return _values.Contains(element);
	}

	internal void Add(TElement element)
	{
		_values.Add(element);
	}

	internal void AddRange(IEnumerable<TElement> elements)
	{
		foreach (TElement element in elements)
		{
			Add(element);
		}
	}

	internal void Remove(TElement element)
	{
		_values.Remove(element);
	}

	internal void Clear()
	{
		_values.Clear();
	}

	internal TElement[] ToArray()
	{
		return _values.ToArray();
	}

	internal bool SetEquals(Set<TElement> other)
	{
		if (_values.Count == other._values.Count)
		{
			return _values.IsSubsetOf(other._values);
		}
		return false;
	}

	internal bool IsSubsetOf(Set<TElement> other)
	{
		return _values.IsSubsetOf(other._values);
	}

	internal bool Overlaps(Set<TElement> other)
	{
		return _values.Overlaps(other._values);
	}

	internal void Subtract(IEnumerable<TElement> other)
	{
		_values.ExceptWith(other);
	}

	internal Set<TElement> Difference(IEnumerable<TElement> other)
	{
		Set<TElement> set = new Set<TElement>(this);
		set.Subtract(other);
		return set;
	}

	internal void Unite(IEnumerable<TElement> other)
	{
		_values.UnionWith(other);
	}

	internal Set<TElement> Union(IEnumerable<TElement> other)
	{
		Set<TElement> set = new Set<TElement>(this);
		set.Unite(other);
		return set;
	}

	internal void Intersect(Set<TElement> other)
	{
		_values.IntersectWith(other._values);
	}

	internal Set<TElement> AsReadOnly()
	{
		if (_isReadOnly)
		{
			return this;
		}
		return new Set<TElement>(this)
		{
			_isReadOnly = true
		};
	}

	internal Set<TElement> MakeReadOnly()
	{
		_isReadOnly = true;
		return this;
	}

	internal int GetElementsHashCode()
	{
		int num = 0;
		using HashSet<TElement>.Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			TElement current = enumerator.Current;
			num ^= Comparer.GetHashCode(current);
		}
		return num;
	}

	public HashSet<TElement>.Enumerator GetEnumerator()
	{
		return _values.GetEnumerator();
	}

	[Conditional("DEBUG")]
	private void AssertReadWrite()
	{
	}

	[Conditional("DEBUG")]
	private void AssertSetCompatible(Set<TElement> other)
	{
	}

	IEnumerator<TElement> IEnumerable<TElement>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		StringUtil.ToCommaSeparatedStringSorted(builder, this);
	}
}
