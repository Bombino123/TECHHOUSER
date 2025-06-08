using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;

namespace System.Data.Entity.Core.Objects;

internal sealed class ObjectViewQueryResultData<TElement> : IObjectViewData<TElement>
{
	private readonly List<TElement> _bindingList;

	private readonly ObjectContext _objectContext;

	private readonly EntitySet _entitySet;

	private readonly bool _canEditItems;

	private readonly bool _canModifyList;

	public IList<TElement> List => _bindingList;

	public bool AllowNew
	{
		get
		{
			if (_canModifyList)
			{
				return _entitySet != null;
			}
			return false;
		}
	}

	public bool AllowEdit => _canEditItems;

	public bool AllowRemove => _canModifyList;

	public bool FiresEventOnAdd => false;

	public bool FiresEventOnRemove => true;

	public bool FiresEventOnClear => false;

	internal ObjectViewQueryResultData(IEnumerable queryResults, ObjectContext objectContext, bool forceReadOnlyList, EntitySet entitySet)
	{
		bool flag = IsEditable(typeof(TElement));
		_objectContext = objectContext;
		_entitySet = entitySet;
		_canEditItems = flag;
		_canModifyList = !forceReadOnlyList && flag && _objectContext != null;
		_bindingList = new List<TElement>();
		foreach (TElement queryResult in queryResults)
		{
			_bindingList.Add(queryResult);
		}
	}

	private static bool IsEditable(Type elementType)
	{
		if (!(elementType == typeof(DbDataRecord)))
		{
			if (elementType != typeof(DbDataRecord))
			{
				return !elementType.IsSubclassOf(typeof(DbDataRecord));
			}
			return true;
		}
		return false;
	}

	private void EnsureEntitySet()
	{
		if (_entitySet == null)
		{
			throw new InvalidOperationException(Strings.ObjectView_CannotResolveTheEntitySet(typeof(TElement).FullName));
		}
	}

	public void EnsureCanAddNew()
	{
		EnsureEntitySet();
	}

	public int Add(TElement item, bool isAddNew)
	{
		EnsureEntitySet();
		if (!isAddNew)
		{
			_objectContext.AddObject(TypeHelpers.GetFullName(_entitySet.EntityContainer.Name, _entitySet.Name), item);
		}
		_bindingList.Add(item);
		return _bindingList.Count - 1;
	}

	public void CommitItemAt(int index)
	{
		EnsureEntitySet();
		TElement val = _bindingList[index];
		_objectContext.AddObject(TypeHelpers.GetFullName(_entitySet.EntityContainer.Name, _entitySet.Name), val);
	}

	public void Clear()
	{
		while (0 < _bindingList.Count)
		{
			TElement item = _bindingList[_bindingList.Count - 1];
			Remove(item, isCancelNew: false);
		}
	}

	public bool Remove(TElement item, bool isCancelNew)
	{
		if (isCancelNew)
		{
			return _bindingList.Remove(item);
		}
		EntityEntry entityEntry = _objectContext.ObjectStateManager.FindEntityEntry(item);
		if (entityEntry != null)
		{
			entityEntry.Delete();
			return true;
		}
		return false;
	}

	public ListChangedEventArgs OnCollectionChanged(object sender, CollectionChangeEventArgs e, ObjectViewListener listener)
	{
		ListChangedEventArgs result = null;
		if (e.Element.GetType().IsAssignableFrom(typeof(TElement)) && _bindingList.Contains((TElement)e.Element))
		{
			TElement val = (TElement)e.Element;
			int num = _bindingList.IndexOf(val);
			if (num >= 0 && e.Action == CollectionChangeAction.Remove)
			{
				_bindingList.Remove(val);
				listener.UnregisterEntityEvents(val);
				result = new ListChangedEventArgs(ListChangedType.ItemDeleted, num, -1);
			}
		}
		return result;
	}
}
