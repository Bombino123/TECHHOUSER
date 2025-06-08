using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace System.Data.Entity.Internal;

internal class ObservableBackedBindingList<T> : SortableBindingList<T>
{
	private bool _addingNewInstance;

	private T _addNewInstance;

	private T _cancelNewInstance;

	private readonly ObservableCollection<T> _obervableCollection;

	private bool _inCollectionChanged;

	private bool _changingObservableCollection;

	public ObservableBackedBindingList(ObservableCollection<T> obervableCollection)
		: base(obervableCollection.ToList())
	{
		_obervableCollection = obervableCollection;
		_obervableCollection.CollectionChanged += ObservableCollectionChanged;
	}

	protected override object AddNewCore()
	{
		_addingNewInstance = true;
		_addNewInstance = (T)base.AddNewCore();
		return _addNewInstance;
	}

	public override void CancelNew(int itemIndex)
	{
		if (itemIndex >= 0 && itemIndex < base.Count && object.Equals(base[itemIndex], _addNewInstance))
		{
			_cancelNewInstance = _addNewInstance;
			_addNewInstance = default(T);
			_addingNewInstance = false;
		}
		base.CancelNew(itemIndex);
	}

	protected override void ClearItems()
	{
		foreach (T item in base.Items)
		{
			RemoveFromObservableCollection(item);
		}
		base.ClearItems();
	}

	public override void EndNew(int itemIndex)
	{
		if (itemIndex >= 0 && itemIndex < base.Count && object.Equals(base[itemIndex], _addNewInstance))
		{
			AddToObservableCollection(_addNewInstance);
			_addNewInstance = default(T);
			_addingNewInstance = false;
		}
		base.EndNew(itemIndex);
	}

	protected override void InsertItem(int index, T item)
	{
		base.InsertItem(index, item);
		if (!_addingNewInstance && index >= 0 && index <= base.Count)
		{
			AddToObservableCollection(item);
		}
	}

	protected override void RemoveItem(int index)
	{
		if (index >= 0 && index < base.Count && object.Equals(base[index], _cancelNewInstance))
		{
			_cancelNewInstance = default(T);
		}
		else
		{
			RemoveFromObservableCollection(base[index]);
		}
		base.RemoveItem(index);
	}

	protected override void SetItem(int index, T item)
	{
		T val = base[index];
		base.SetItem(index, item);
		if (index >= 0 && index < base.Count)
		{
			if (object.Equals(val, _addNewInstance))
			{
				_addNewInstance = default(T);
				_addingNewInstance = false;
			}
			else
			{
				RemoveFromObservableCollection(val);
			}
			AddToObservableCollection(item);
		}
	}

	private void ObservableCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		if (_changingObservableCollection)
		{
			return;
		}
		try
		{
			_inCollectionChanged = true;
			if (e.Action == NotifyCollectionChangedAction.Reset)
			{
				Clear();
			}
			if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Replace)
			{
				foreach (T oldItem in e.OldItems)
				{
					Remove(oldItem);
				}
			}
			if (e.Action != 0 && e.Action != NotifyCollectionChangedAction.Replace)
			{
				return;
			}
			foreach (T newItem in e.NewItems)
			{
				Add(newItem);
			}
		}
		finally
		{
			_inCollectionChanged = false;
		}
	}

	private void AddToObservableCollection(T item)
	{
		if (!_inCollectionChanged)
		{
			try
			{
				_changingObservableCollection = true;
				_obervableCollection.Add(item);
			}
			finally
			{
				_changingObservableCollection = false;
			}
		}
	}

	private void RemoveFromObservableCollection(T item)
	{
		if (!_inCollectionChanged)
		{
			try
			{
				_changingObservableCollection = true;
				_obervableCollection.Remove(item);
			}
			finally
			{
				_changingObservableCollection = false;
			}
		}
	}
}
