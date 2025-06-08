using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace GMap.NET.ObjectModel;

[Serializable]
public class ObservableCollection<T> : ICollection<T>, IEnumerable<T>, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
{
	[Serializable]
	private class SimpleMonitor : IDisposable
	{
		private int _busyCount;

		public bool Busy => _busyCount > 0;

		public void Dispose()
		{
			_busyCount--;
		}

		public void Enter()
		{
			_busyCount++;
		}
	}

	protected Collection<T> _inner;

	protected object _lock = new object();

	private SimpleMonitor _monitor;

	private const string CountString = "Count";

	private const string IndexerName = "Item[]";

	public int Count
	{
		get
		{
			lock (_lock)
			{
				return _inner.Count;
			}
		}
	}

	public bool IsReadOnly => false;

	public T this[int index]
	{
		get
		{
			lock (_lock)
			{
				return _inner[index];
			}
		}
		set
		{
			SetItem(index, value);
		}
	}

	public virtual event NotifyCollectionChangedEventHandler CollectionChanged;

	protected event PropertyChangedEventHandler PropertyChanged;

	event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
	{
		add
		{
			PropertyChanged += value;
		}
		remove
		{
			PropertyChanged -= value;
		}
	}

	public ObservableCollection()
	{
		_monitor = new SimpleMonitor();
		_inner = new Collection<T>();
	}

	public ObservableCollection(IEnumerable<T> collection)
	{
		_monitor = new SimpleMonitor();
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		_inner = new Collection<T>();
		CopyFrom(collection);
	}

	public ObservableCollection(List<T> list)
	{
		_monitor = new SimpleMonitor();
		_inner = new Collection<T>((list != null) ? new List<T>(list.Count) : list);
		CopyFrom(list);
	}

	protected IDisposable BlockReentrancy()
	{
		_monitor.Enter();
		return _monitor;
	}

	protected void CheckReentrancy()
	{
		if (_monitor.Busy && this.CollectionChanged != null && this.CollectionChanged.GetInvocationList().Length > 1)
		{
			throw new InvalidOperationException("ObservableCollectionReentrancyNotAllowed");
		}
	}

	protected void ClearItems()
	{
		lock (_lock)
		{
			CheckReentrancy();
			_inner.Clear();
			OnPropertyChanged("Count");
			OnPropertyChanged("Item[]");
			OnCollectionReset();
		}
	}

	private void CopyFrom(IEnumerable<T> collection)
	{
		if (collection == null)
		{
			return;
		}
		lock (_lock)
		{
			foreach (T item in collection)
			{
				AddItem(item);
			}
		}
	}

	protected void AddItem(T item)
	{
		lock (_lock)
		{
			CheckReentrancy();
			_inner.Add(item);
			OnPropertyChanged("Count");
			OnPropertyChanged("Item[]");
			OnCollectionChanged(NotifyCollectionChangedAction.Add, item, _inner.Count - 1);
		}
	}

	protected void InsertItem(int index, T item)
	{
		lock (_lock)
		{
			CheckReentrancy();
			_inner.Insert(index, item);
			OnPropertyChanged("Count");
			OnPropertyChanged("Item[]");
			OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
		}
	}

	public void Move(int oldIndex, int newIndex)
	{
		MoveItem(oldIndex, newIndex);
	}

	protected virtual void MoveItem(int oldIndex, int newIndex)
	{
		lock (_lock)
		{
			CheckReentrancy();
			T val = _inner[oldIndex];
			_inner.RemoveAt(oldIndex);
			_inner.Insert(newIndex, val);
			OnPropertyChanged("Item[]");
			OnCollectionChanged(NotifyCollectionChangedAction.Move, val, newIndex, oldIndex);
		}
	}

	protected void RemoveItem(int index)
	{
		lock (_lock)
		{
			CheckReentrancy();
			T val = _inner[index];
			_inner.RemoveAt(index);
			OnPropertyChanged("Count");
			OnPropertyChanged("Item[]");
			OnCollectionChanged(NotifyCollectionChangedAction.Remove, val, index);
		}
	}

	protected void SetItem(int index, T item)
	{
		lock (_lock)
		{
			CheckReentrancy();
			T val = _inner[index];
			_inner[index] = item;
			OnPropertyChanged("Item[]");
			OnCollectionChanged(NotifyCollectionChangedAction.Replace, val, item, index);
		}
	}

	protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
	{
		if (this.CollectionChanged != null)
		{
			using (BlockReentrancy())
			{
				this.CollectionChanged(this, e);
			}
		}
	}

	private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
	{
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
	}

	private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index, int oldIndex)
	{
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index, oldIndex));
	}

	private void OnCollectionChanged(NotifyCollectionChangedAction action, object oldItem, object newItem, int index)
	{
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
	}

	private void OnCollectionReset()
	{
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
	}

	protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
	{
		if (this.PropertyChanged != null)
		{
			this.PropertyChanged(this, e);
		}
	}

	private void OnPropertyChanged(string propertyName)
	{
		OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
	}

	public void Add(T item)
	{
		AddItem(item);
	}

	public void Clear()
	{
		ClearItems();
	}

	public bool Contains(T item)
	{
		lock (_lock)
		{
			return _inner.Contains(item);
		}
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		lock (_lock)
		{
			_inner.CopyTo(array, arrayIndex);
		}
	}

	public bool Remove(T item)
	{
		RemoveItem(_inner.IndexOf(item));
		return true;
	}

	public void RemoveAt(int index)
	{
		RemoveItem(index);
	}

	public void Insert(int index, T item)
	{
		InsertItem(index, item);
	}

	public int IndexOf(T item)
	{
		lock (_lock)
		{
			return _inner.IndexOf(item);
		}
	}

	public IEnumerator<T> GetEnumerator()
	{
		lock (_lock)
		{
			return new ThreadSafeEnumerator<T>(_inner.GetEnumerator(), _lock);
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
