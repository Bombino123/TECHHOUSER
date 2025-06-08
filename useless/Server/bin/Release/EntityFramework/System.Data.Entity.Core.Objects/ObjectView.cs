using System.Collections;
using System.ComponentModel;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Objects;

internal class ObjectView<TElement> : IBindingList, IList, ICollection, IEnumerable, ICancelAddNew, IObjectView
{
	private bool _suspendEvent;

	private ListChangedEventHandler onListChanged;

	private readonly ObjectViewListener _listener;

	private int _addNewIndex = -1;

	private readonly IObjectViewData<TElement> _viewData;

	private static bool IsElementTypeAbstract => typeof(TElement).IsAbstract();

	bool IBindingList.AllowNew
	{
		get
		{
			if (_viewData.AllowNew)
			{
				return !IsElementTypeAbstract;
			}
			return false;
		}
	}

	bool IBindingList.AllowEdit => _viewData.AllowEdit;

	bool IBindingList.AllowRemove => _viewData.AllowRemove;

	bool IBindingList.SupportsChangeNotification => true;

	bool IBindingList.SupportsSearching => false;

	bool IBindingList.SupportsSorting => false;

	bool IBindingList.IsSorted => false;

	PropertyDescriptor IBindingList.SortProperty
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	ListSortDirection IBindingList.SortDirection
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public TElement this[int index]
	{
		get
		{
			return _viewData.List[index];
		}
		set
		{
			throw new InvalidOperationException(Strings.ObjectView_CannotReplacetheEntityorRow);
		}
	}

	object IList.this[int index]
	{
		get
		{
			return _viewData.List[index];
		}
		set
		{
			throw new InvalidOperationException(Strings.ObjectView_CannotReplacetheEntityorRow);
		}
	}

	bool IList.IsReadOnly
	{
		get
		{
			if (!_viewData.AllowNew)
			{
				return !_viewData.AllowRemove;
			}
			return false;
		}
	}

	bool IList.IsFixedSize => false;

	public int Count => _viewData.List.Count;

	object ICollection.SyncRoot => this;

	bool ICollection.IsSynchronized => false;

	public event ListChangedEventHandler ListChanged
	{
		add
		{
			onListChanged = (ListChangedEventHandler)Delegate.Combine(onListChanged, value);
		}
		remove
		{
			onListChanged = (ListChangedEventHandler)Delegate.Remove(onListChanged, value);
		}
	}

	internal ObjectView(IObjectViewData<TElement> viewData, object eventDataSource)
	{
		_viewData = viewData;
		_listener = new ObjectViewListener(this, (IList)_viewData.List, eventDataSource);
	}

	private void EnsureWritableList()
	{
		if (((IList)this).IsReadOnly)
		{
			throw new InvalidOperationException(Strings.ObjectView_WriteOperationNotAllowedOnReadOnlyBindingList);
		}
	}

	void ICancelAddNew.CancelNew(int itemIndex)
	{
		if (_addNewIndex >= 0 && itemIndex == _addNewIndex)
		{
			TElement val = _viewData.List[_addNewIndex];
			_listener.UnregisterEntityEvents(val);
			int addNewIndex = _addNewIndex;
			_addNewIndex = -1;
			try
			{
				_suspendEvent = true;
				_viewData.Remove(val, isCancelNew: true);
			}
			finally
			{
				_suspendEvent = false;
			}
			OnListChanged(ListChangedType.ItemDeleted, addNewIndex, -1);
		}
	}

	void ICancelAddNew.EndNew(int itemIndex)
	{
		if (_addNewIndex >= 0 && itemIndex == _addNewIndex)
		{
			_viewData.CommitItemAt(_addNewIndex);
			_addNewIndex = -1;
		}
	}

	object IBindingList.AddNew()
	{
		EnsureWritableList();
		if (IsElementTypeAbstract)
		{
			throw new InvalidOperationException(Strings.ObjectView_AddNewOperationNotAllowedOnAbstractBindingList);
		}
		_viewData.EnsureCanAddNew();
		((ICancelAddNew)this).EndNew(_addNewIndex);
		TElement val = (TElement)Activator.CreateInstance(typeof(TElement));
		_addNewIndex = _viewData.Add(val, isAddNew: true);
		_listener.RegisterEntityEvents(val);
		OnListChanged(ListChangedType.ItemAdded, _addNewIndex, -1);
		return val;
	}

	void IBindingList.AddIndex(PropertyDescriptor property)
	{
		throw new NotSupportedException();
	}

	void IBindingList.ApplySort(PropertyDescriptor property, ListSortDirection direction)
	{
		throw new NotSupportedException();
	}

	int IBindingList.Find(PropertyDescriptor property, object key)
	{
		throw new NotSupportedException();
	}

	void IBindingList.RemoveIndex(PropertyDescriptor property)
	{
		throw new NotSupportedException();
	}

	void IBindingList.RemoveSort()
	{
		throw new NotSupportedException();
	}

	int IList.Add(object value)
	{
		Check.NotNull(value, "value");
		EnsureWritableList();
		if (!(value is TElement))
		{
			throw new ArgumentException(Strings.ObjectView_IncompatibleArgument);
		}
		((ICancelAddNew)this).EndNew(_addNewIndex);
		int num = ((IList)this).IndexOf(value);
		if (num == -1)
		{
			num = _viewData.Add((TElement)value, isAddNew: false);
			if (!_viewData.FiresEventOnAdd)
			{
				_listener.RegisterEntityEvents(value);
				OnListChanged(ListChangedType.ItemAdded, num, -1);
			}
		}
		return num;
	}

	void IList.Clear()
	{
		EnsureWritableList();
		((ICancelAddNew)this).EndNew(_addNewIndex);
		if (_viewData.FiresEventOnClear)
		{
			_viewData.Clear();
			return;
		}
		try
		{
			_suspendEvent = true;
			_viewData.Clear();
		}
		finally
		{
			_suspendEvent = false;
		}
		OnListChanged(ListChangedType.Reset, -1, -1);
	}

	bool IList.Contains(object value)
	{
		if (value is TElement)
		{
			return _viewData.List.Contains((TElement)value);
		}
		return false;
	}

	int IList.IndexOf(object value)
	{
		if (value is TElement)
		{
			return _viewData.List.IndexOf((TElement)value);
		}
		return -1;
	}

	void IList.Insert(int index, object value)
	{
		throw new NotSupportedException(Strings.ObjectView_IndexBasedInsertIsNotSupported);
	}

	void IList.Remove(object value)
	{
		Check.NotNull(value, "value");
		EnsureWritableList();
		if (!(value is TElement))
		{
			throw new ArgumentException(Strings.ObjectView_IncompatibleArgument);
		}
		((ICancelAddNew)this).EndNew(_addNewIndex);
		TElement val = (TElement)value;
		int newIndex = _viewData.List.IndexOf(val);
		if (_viewData.Remove(val, isCancelNew: false) && !_viewData.FiresEventOnRemove)
		{
			_listener.UnregisterEntityEvents(val);
			OnListChanged(ListChangedType.ItemDeleted, newIndex, -1);
		}
	}

	void IList.RemoveAt(int index)
	{
		((IList)this).Remove(((IList)this)[index]);
	}

	public void CopyTo(Array array, int index)
	{
		((IList)_viewData.List).CopyTo(array, index);
	}

	public IEnumerator GetEnumerator()
	{
		return _viewData.List.GetEnumerator();
	}

	private void OnListChanged(ListChangedType listchangedType, int newIndex, int oldIndex)
	{
		ListChangedEventArgs changeArgs = new ListChangedEventArgs(listchangedType, newIndex, oldIndex);
		OnListChanged(changeArgs);
	}

	private void OnListChanged(ListChangedEventArgs changeArgs)
	{
		if (onListChanged != null && !_suspendEvent)
		{
			onListChanged(this, changeArgs);
		}
	}

	void IObjectView.EntityPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		int num = ((IList)this).IndexOf((object?)(TElement)sender);
		OnListChanged(ListChangedType.ItemChanged, num, num);
	}

	void IObjectView.CollectionChanged(object sender, CollectionChangeEventArgs e)
	{
		TElement val = default(TElement);
		if (_addNewIndex >= 0)
		{
			val = this[_addNewIndex];
		}
		ListChangedEventArgs listChangedEventArgs = _viewData.OnCollectionChanged(sender, e, _listener);
		if (_addNewIndex >= 0)
		{
			if (_addNewIndex >= Count)
			{
				_addNewIndex = ((IList)this).IndexOf((object?)val);
			}
			else if (!this[_addNewIndex].Equals(val))
			{
				_addNewIndex = ((IList)this).IndexOf((object?)val);
			}
		}
		if (listChangedEventArgs != null)
		{
			OnListChanged(listChangedEventArgs);
		}
	}
}
