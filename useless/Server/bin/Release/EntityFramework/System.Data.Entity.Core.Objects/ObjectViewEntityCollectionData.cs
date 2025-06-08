using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Core.Objects.DataClasses;

namespace System.Data.Entity.Core.Objects;

internal sealed class ObjectViewEntityCollectionData<TViewElement, TItemElement> : IObjectViewData<TViewElement> where TViewElement : TItemElement where TItemElement : class
{
	private readonly List<TViewElement> _bindingList;

	private readonly EntityCollection<TItemElement> _entityCollection;

	private readonly bool _canEditItems;

	private bool _itemCommitPending;

	public IList<TViewElement> List => _bindingList;

	public bool AllowNew => !_entityCollection.IsReadOnly;

	public bool AllowEdit => _canEditItems;

	public bool AllowRemove => !_entityCollection.IsReadOnly;

	public bool FiresEventOnAdd => true;

	public bool FiresEventOnRemove => true;

	public bool FiresEventOnClear => true;

	internal ObjectViewEntityCollectionData(EntityCollection<TItemElement> entityCollection)
	{
		_entityCollection = entityCollection;
		_canEditItems = true;
		_bindingList = new List<TViewElement>(entityCollection.Count);
		foreach (TViewElement item in entityCollection)
		{
			_bindingList.Add(item);
		}
	}

	public void EnsureCanAddNew()
	{
	}

	public int Add(TViewElement item, bool isAddNew)
	{
		if (isAddNew)
		{
			_bindingList.Add(item);
		}
		else
		{
			_entityCollection.Add((TItemElement)(object)item);
		}
		return _bindingList.Count - 1;
	}

	public void CommitItemAt(int index)
	{
		TViewElement val = _bindingList[index];
		try
		{
			_itemCommitPending = true;
			_entityCollection.Add((TItemElement)(object)val);
		}
		finally
		{
			_itemCommitPending = false;
		}
	}

	public void Clear()
	{
		if (0 >= _bindingList.Count)
		{
			return;
		}
		List<object> list = new List<object>();
		foreach (TViewElement binding in _bindingList)
		{
			object item = binding;
			list.Add(item);
		}
		_entityCollection.BulkDeleteAll(list);
	}

	public bool Remove(TViewElement item, bool isCancelNew)
	{
		if (isCancelNew)
		{
			return _bindingList.Remove(item);
		}
		return _entityCollection.RemoveInternal((TItemElement)(object)item);
	}

	public ListChangedEventArgs OnCollectionChanged(object sender, CollectionChangeEventArgs e, ObjectViewListener listener)
	{
		ListChangedEventArgs result = null;
		switch (e.Action)
		{
		case CollectionChangeAction.Remove:
			if (e.Element is TViewElement)
			{
				TViewElement val3 = (TViewElement)e.Element;
				int num = _bindingList.IndexOf(val3);
				if (num != -1)
				{
					_bindingList.Remove(val3);
					listener.UnregisterEntityEvents(val3);
					result = new ListChangedEventArgs(ListChangedType.ItemDeleted, num, -1);
				}
			}
			break;
		case CollectionChangeAction.Add:
			if (e.Element is TViewElement && !_itemCommitPending)
			{
				TViewElement val2 = (TViewElement)e.Element;
				_bindingList.Add(val2);
				listener.RegisterEntityEvents(val2);
				result = new ListChangedEventArgs(ListChangedType.ItemAdded, _bindingList.Count - 1, -1);
			}
			break;
		case CollectionChangeAction.Refresh:
			foreach (TViewElement binding in _bindingList)
			{
				listener.UnregisterEntityEvents(binding);
			}
			_bindingList.Clear();
			foreach (TViewElement item in _entityCollection.GetInternalEnumerable())
			{
				_bindingList.Add(item);
				listener.RegisterEntityEvents(item);
			}
			result = new ListChangedEventArgs(ListChangedType.Reset, -1, -1);
			break;
		}
		return result;
	}
}
