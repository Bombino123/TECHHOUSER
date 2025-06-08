using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Data.Entity.Core.Metadata.Edm;

public class ReadOnlyMetadataCollection<T> : ReadOnlyCollection<T> where T : MetadataItem
{
	public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
	{
		private int _nextIndex;

		private readonly IList<T> _parent;

		private T _current;

		public T Current => _current;

		object IEnumerator.Current => Current;

		internal Enumerator(IList<T> collection)
		{
			_parent = collection;
			_nextIndex = 0;
			_current = null;
		}

		public void Dispose()
		{
		}

		public bool MoveNext()
		{
			if ((uint)_nextIndex < (uint)_parent.Count)
			{
				_current = _parent[_nextIndex];
				_nextIndex++;
				return true;
			}
			_current = null;
			return false;
		}

		public void Reset()
		{
			_current = null;
			_nextIndex = 0;
		}
	}

	public bool IsReadOnly => true;

	public virtual T this[string identity] => ((MetadataCollection<T>)base.Items)[identity];

	internal MetadataCollection<T> Source
	{
		get
		{
			try
			{
				return (MetadataCollection<T>)base.Items;
			}
			finally
			{
				this.SourceAccessed?.Invoke(this, null);
			}
		}
	}

	internal event EventHandler SourceAccessed;

	internal ReadOnlyMetadataCollection()
		: base((IList<T>)new MetadataCollection<T>())
	{
	}

	internal ReadOnlyMetadataCollection(MetadataCollection<T> collection)
		: base((IList<T>)collection)
	{
	}

	internal ReadOnlyMetadataCollection(List<T> list)
		: base((IList<T>)MetadataCollection<T>.Wrap(list))
	{
	}

	public virtual T GetValue(string identity, bool ignoreCase)
	{
		return ((MetadataCollection<T>)base.Items).GetValue(identity, ignoreCase);
	}

	public virtual bool Contains(string identity)
	{
		return ((MetadataCollection<T>)base.Items).ContainsIdentity(identity);
	}

	public virtual bool TryGetValue(string identity, bool ignoreCase, out T item)
	{
		return ((MetadataCollection<T>)base.Items).TryGetValue(identity, ignoreCase, out item);
	}

	public new Enumerator GetEnumerator()
	{
		return new Enumerator(base.Items);
	}

	public new virtual int IndexOf(T value)
	{
		return base.IndexOf(value);
	}
}
