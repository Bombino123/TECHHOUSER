using System.Collections;
using System.ComponentModel;
using System.Data.Entity.Core.Objects.DataClasses;

namespace System.Data.Entity.Core.Objects;

internal sealed class ObjectViewListener
{
	private readonly WeakReference _viewWeak;

	private readonly object _dataSource;

	private readonly IList _list;

	internal ObjectViewListener(IObjectView view, IList list, object dataSource)
	{
		_viewWeak = new WeakReference(view);
		_dataSource = dataSource;
		_list = list;
		RegisterCollectionEvents();
		RegisterEntityEvents();
	}

	private void CleanUpListener()
	{
		UnregisterCollectionEvents();
		UnregisterEntityEvents();
	}

	private void RegisterCollectionEvents()
	{
		if (_dataSource is ObjectStateManager objectStateManager)
		{
			objectStateManager.EntityDeleted += CollectionChanged;
		}
		else if (_dataSource != null)
		{
			((RelatedEnd)_dataSource).AssociationChangedForObjectView += CollectionChanged;
		}
	}

	private void UnregisterCollectionEvents()
	{
		if (_dataSource is ObjectStateManager objectStateManager)
		{
			objectStateManager.EntityDeleted -= CollectionChanged;
		}
		else if (_dataSource != null)
		{
			((RelatedEnd)_dataSource).AssociationChangedForObjectView -= CollectionChanged;
		}
	}

	internal void RegisterEntityEvents(object entity)
	{
		if (entity is INotifyPropertyChanged notifyPropertyChanged)
		{
			notifyPropertyChanged.PropertyChanged += EntityPropertyChanged;
		}
	}

	private void RegisterEntityEvents()
	{
		if (_list == null)
		{
			return;
		}
		foreach (object item in _list)
		{
			if (item is INotifyPropertyChanged notifyPropertyChanged)
			{
				notifyPropertyChanged.PropertyChanged += EntityPropertyChanged;
			}
		}
	}

	internal void UnregisterEntityEvents(object entity)
	{
		if (entity is INotifyPropertyChanged notifyPropertyChanged)
		{
			notifyPropertyChanged.PropertyChanged -= EntityPropertyChanged;
		}
	}

	private void UnregisterEntityEvents()
	{
		if (_list == null)
		{
			return;
		}
		foreach (object item in _list)
		{
			if (item is INotifyPropertyChanged notifyPropertyChanged)
			{
				notifyPropertyChanged.PropertyChanged -= EntityPropertyChanged;
			}
		}
	}

	private void EntityPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		IObjectView objectView = (IObjectView)_viewWeak.Target;
		if (objectView != null)
		{
			objectView.EntityPropertyChanged(sender, e);
		}
		else
		{
			CleanUpListener();
		}
	}

	private void CollectionChanged(object sender, CollectionChangeEventArgs e)
	{
		IObjectView objectView = (IObjectView)_viewWeak.Target;
		if (objectView != null)
		{
			objectView.CollectionChanged(sender, e);
		}
		else
		{
			CleanUpListener();
		}
	}
}
