using System;
using System.Windows.Forms;

namespace GMap.NET.ObjectModel;

public class ObservableCollectionThreadSafe<T> : ObservableCollection<T>
{
	private NotifyCollectionChangedEventHandler _collectionChanged;

	public override event NotifyCollectionChangedEventHandler CollectionChanged
	{
		add
		{
			_collectionChanged = (NotifyCollectionChangedEventHandler)Delegate.Combine(_collectionChanged, value);
		}
		remove
		{
			_collectionChanged = (NotifyCollectionChangedEventHandler)Delegate.Remove(_collectionChanged, value);
		}
	}

	protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
	{
		using (BlockReentrancy())
		{
			if (_collectionChanged == null)
			{
				return;
			}
			Delegate[] invocationList = _collectionChanged.GetInvocationList();
			for (int i = 0; i < invocationList.Length; i++)
			{
				NotifyCollectionChangedEventHandler notifyCollectionChangedEventHandler = (NotifyCollectionChangedEventHandler)invocationList[i];
				object? target = notifyCollectionChangedEventHandler.Target;
				Control val = (Control)((target is Control) ? target : null);
				if (val != null && val.InvokeRequired)
				{
					val.Invoke((Delegate)notifyCollectionChangedEventHandler, new object[2] { this, e });
				}
				else
				{
					_collectionChanged(this, e);
				}
			}
		}
	}
}
