using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Internal;

internal class DbLocalView<TEntity> : ObservableCollection<TEntity>, ICollection<TEntity>, IEnumerable<TEntity>, IEnumerable, IList, ICollection where TEntity : class
{
	private readonly InternalContext _internalContext;

	private bool _inStateManagerChanged;

	private ObservableBackedBindingList<TEntity> _bindingList;

	internal ObservableBackedBindingList<TEntity> BindingList => _bindingList ?? (_bindingList = new ObservableBackedBindingList<TEntity>(this));

	public DbLocalView()
	{
	}

	public DbLocalView(IEnumerable<TEntity> collection)
	{
		Check.NotNull(collection, "collection");
		collection.Each(Add);
	}

	internal DbLocalView(InternalContext internalContext)
	{
		_internalContext = internalContext;
		try
		{
			_inStateManagerChanged = true;
			foreach (TEntity localEntity in _internalContext.GetLocalEntities<TEntity>())
			{
				Add(localEntity);
			}
		}
		finally
		{
			_inStateManagerChanged = false;
		}
		_internalContext.RegisterObjectStateManagerChangedEvent(StateManagerChangedHandler);
	}

	protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
	{
		if (!_inStateManagerChanged && _internalContext != null)
		{
			if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Replace)
			{
				foreach (TEntity oldItem in e.OldItems)
				{
					_internalContext.Set<TEntity>().Remove(oldItem);
				}
			}
			if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace)
			{
				foreach (TEntity newItem in e.NewItems)
				{
					if (!_internalContext.EntityInContextAndNotDeleted(newItem))
					{
						_internalContext.Set<TEntity>().Add(newItem);
					}
				}
			}
		}
		base.OnCollectionChanged(e);
	}

	private void StateManagerChangedHandler(object sender, CollectionChangeEventArgs e)
	{
		try
		{
			_inStateManagerChanged = true;
			if (e.Element is TEntity item)
			{
				if (e.Action == CollectionChangeAction.Remove && Contains(item))
				{
					Remove(item);
				}
				else if (e.Action == CollectionChangeAction.Add && !Contains(item))
				{
					Add(item);
				}
			}
		}
		finally
		{
			_inStateManagerChanged = false;
		}
	}

	protected override void ClearItems()
	{
		new List<TEntity>(this).Each((TEntity t) => Remove(t));
	}

	protected override void InsertItem(int index, TEntity item)
	{
		if (!Contains(item))
		{
			base.InsertItem(index, item);
		}
	}

	public new virtual bool Contains(TEntity item)
	{
		IEqualityComparer<TEntity> @default = ObjectReferenceEqualityComparer.Default;
		foreach (TEntity item2 in base.Items)
		{
			if (@default.Equals(item2, item))
			{
				return true;
			}
		}
		return false;
	}

	public new virtual bool Remove(TEntity item)
	{
		IEqualityComparer<TEntity> @default = ObjectReferenceEqualityComparer.Default;
		int i;
		for (i = 0; i < base.Count && !@default.Equals(base.Items[i], item); i++)
		{
		}
		if (i == base.Count)
		{
			return false;
		}
		RemoveItem(i);
		return true;
	}

	bool ICollection<TEntity>.Contains(TEntity item)
	{
		return Contains(item);
	}

	bool ICollection<TEntity>.Remove(TEntity item)
	{
		return Remove(item);
	}

	bool IList.Contains(object value)
	{
		if (IsCompatibleObject(value))
		{
			return Contains((TEntity)value);
		}
		return false;
	}

	void IList.Remove(object value)
	{
		if (IsCompatibleObject(value))
		{
			Remove((TEntity)value);
		}
	}

	private static bool IsCompatibleObject(object value)
	{
		if (!(value is TEntity))
		{
			return value == null;
		}
		return true;
	}
}
