using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Resources;
using System.Runtime.CompilerServices;

namespace System.Data.Entity.Core.Metadata.Edm;

internal class FilteredReadOnlyMetadataCollection<TDerived, TBase> : ReadOnlyMetadataCollection<TDerived>, IBaseList<TBase>, IList, ICollection, IEnumerable where TDerived : TBase where TBase : MetadataItem
{
	private readonly ReadOnlyMetadataCollection<TBase> _source;

	private readonly Predicate<TBase> _predicate;

	public override TDerived this[string identity]
	{
		get
		{
			TBase val = _source[identity];
			if (_predicate(val))
			{
				return (TDerived)val;
			}
			throw new ArgumentException(Strings.ItemInvalidIdentity(identity), "identity");
		}
	}

	TBase IBaseList<TBase>.this[string identity] => (TBase)this[identity];

	TBase IBaseList<TBase>.this[int index] => (TBase)base[index];

	internal FilteredReadOnlyMetadataCollection(ReadOnlyMetadataCollection<TBase> collection, Predicate<TBase> predicate)
		: base(FilterCollection(collection, predicate))
	{
		_source = collection;
		_predicate = predicate;
	}

	public override TDerived GetValue(string identity, bool ignoreCase)
	{
		TBase value = _source.GetValue(identity, ignoreCase);
		if (_predicate(value))
		{
			return (TDerived)value;
		}
		throw new ArgumentException(Strings.ItemInvalidIdentity(identity), "identity");
	}

	public override bool Contains(string identity)
	{
		if (_source.TryGetValue(identity, ignoreCase: false, out var item))
		{
			return _predicate(item);
		}
		return false;
	}

	public override bool TryGetValue(string identity, bool ignoreCase, out TDerived item)
	{
		item = null;
		if (_source.TryGetValue(identity, ignoreCase, out var item2) && _predicate(item2))
		{
			item = (TDerived)item2;
			return true;
		}
		return false;
	}

	internal static List<TDerived> FilterCollection(ReadOnlyMetadataCollection<TBase> collection, Predicate<TBase> predicate)
	{
		List<TDerived> list = new List<TDerived>(collection.Count);
		for (int i = 0; i < collection.Count; i++)
		{
			TBase val = collection[i];
			if (predicate(val))
			{
				list.Add((TDerived)val);
			}
		}
		return list;
	}

	public override int IndexOf(TDerived value)
	{
		if (_source.TryGetValue(value.Identity, ignoreCase: false, out var item) && _predicate(item))
		{
			return base.IndexOf((TDerived)item);
		}
		return -1;
	}

	int IBaseList<TBase>.IndexOf(TBase item)
	{
		if (_predicate(item))
		{
			return IndexOf((TDerived)item);
		}
		return -1;
	}

	[SpecialName]
	bool IList.get_IsReadOnly()
	{
		return base.IsReadOnly;
	}
}
