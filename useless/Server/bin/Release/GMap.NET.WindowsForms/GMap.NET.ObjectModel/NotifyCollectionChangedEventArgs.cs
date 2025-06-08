using System;
using System.Collections;

namespace GMap.NET.ObjectModel;

public class NotifyCollectionChangedEventArgs : EventArgs
{
	private NotifyCollectionChangedAction _action;

	private IList _newItems;

	private int _newStartingIndex;

	private IList _oldItems;

	private int _oldStartingIndex;

	public NotifyCollectionChangedAction Action => _action;

	public IList NewItems => _newItems;

	public int NewStartingIndex => _newStartingIndex;

	public IList OldItems => _oldItems;

	public int OldStartingIndex => _oldStartingIndex;

	public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action)
	{
		_newStartingIndex = -1;
		_oldStartingIndex = -1;
		if (action != NotifyCollectionChangedAction.Reset)
		{
			throw new ArgumentException("WrongActionForCtor", "action");
		}
		InitializeAdd(action, null, -1);
	}

	public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList changedItems)
	{
		_newStartingIndex = -1;
		_oldStartingIndex = -1;
		if (action != 0 && action != NotifyCollectionChangedAction.Remove && action != NotifyCollectionChangedAction.Reset)
		{
			throw new ArgumentException("MustBeResetAddOrRemoveActionForCtor", "action");
		}
		if (action == NotifyCollectionChangedAction.Reset)
		{
			if (changedItems != null)
			{
				throw new ArgumentException("ResetActionRequiresNullItem", "action");
			}
			InitializeAdd(action, null, -1);
		}
		else
		{
			if (changedItems == null)
			{
				throw new ArgumentNullException("changedItems");
			}
			InitializeAddOrRemove(action, changedItems, -1);
		}
	}

	public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object changedItem)
	{
		_newStartingIndex = -1;
		_oldStartingIndex = -1;
		if (action != 0 && action != NotifyCollectionChangedAction.Remove && action != NotifyCollectionChangedAction.Reset)
		{
			throw new ArgumentException("MustBeResetAddOrRemoveActionForCtor", "action");
		}
		if (action == NotifyCollectionChangedAction.Reset)
		{
			if (changedItem != null)
			{
				throw new ArgumentException("ResetActionRequiresNullItem", "action");
			}
			InitializeAdd(action, null, -1);
		}
		else
		{
			InitializeAddOrRemove(action, new object[1] { changedItem }, -1);
		}
	}

	public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList newItems, IList oldItems)
	{
		_newStartingIndex = -1;
		_oldStartingIndex = -1;
		if (action != NotifyCollectionChangedAction.Replace)
		{
			throw new ArgumentException("WrongActionForCtor", "action");
		}
		if (newItems == null)
		{
			throw new ArgumentNullException("newItems");
		}
		if (oldItems == null)
		{
			throw new ArgumentNullException("oldItems");
		}
		InitializeMoveOrReplace(action, newItems, oldItems, -1, -1);
	}

	public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList changedItems, int startingIndex)
	{
		_newStartingIndex = -1;
		_oldStartingIndex = -1;
		if (action != 0 && action != NotifyCollectionChangedAction.Remove && action != NotifyCollectionChangedAction.Reset)
		{
			throw new ArgumentException("MustBeResetAddOrRemoveActionForCtor", "action");
		}
		if (action == NotifyCollectionChangedAction.Reset)
		{
			if (changedItems != null)
			{
				throw new ArgumentException("ResetActionRequiresNullItem", "action");
			}
			if (startingIndex != -1)
			{
				throw new ArgumentException("ResetActionRequiresIndexMinus1", "action");
			}
			InitializeAdd(action, null, -1);
		}
		else
		{
			if (changedItems == null)
			{
				throw new ArgumentNullException("changedItems");
			}
			if (startingIndex < -1)
			{
				throw new ArgumentException("IndexCannotBeNegative", "startingIndex");
			}
			InitializeAddOrRemove(action, changedItems, startingIndex);
		}
	}

	public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object changedItem, int index)
	{
		_newStartingIndex = -1;
		_oldStartingIndex = -1;
		if (action != 0 && action != NotifyCollectionChangedAction.Remove && action != NotifyCollectionChangedAction.Reset)
		{
			throw new ArgumentException("MustBeResetAddOrRemoveActionForCtor", "action");
		}
		if (action == NotifyCollectionChangedAction.Reset)
		{
			if (changedItem != null)
			{
				throw new ArgumentException("ResetActionRequiresNullItem", "action");
			}
			if (index != -1)
			{
				throw new ArgumentException("ResetActionRequiresIndexMinus1", "action");
			}
			InitializeAdd(action, null, -1);
		}
		else
		{
			InitializeAddOrRemove(action, new object[1] { changedItem }, index);
		}
	}

	public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object newItem, object oldItem)
	{
		_newStartingIndex = -1;
		_oldStartingIndex = -1;
		if (action != NotifyCollectionChangedAction.Replace)
		{
			throw new ArgumentException("WrongActionForCtor", "action");
		}
		InitializeMoveOrReplace(action, new object[1] { newItem }, new object[1] { oldItem }, -1, -1);
	}

	public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList newItems, IList oldItems, int startingIndex)
	{
		_newStartingIndex = -1;
		_oldStartingIndex = -1;
		if (action != NotifyCollectionChangedAction.Replace)
		{
			throw new ArgumentException("WrongActionForCtor", "action");
		}
		if (newItems == null)
		{
			throw new ArgumentNullException("newItems");
		}
		if (oldItems == null)
		{
			throw new ArgumentNullException("oldItems");
		}
		InitializeMoveOrReplace(action, newItems, oldItems, startingIndex, startingIndex);
	}

	public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList changedItems, int index, int oldIndex)
	{
		_newStartingIndex = -1;
		_oldStartingIndex = -1;
		if (action != NotifyCollectionChangedAction.Move)
		{
			throw new ArgumentException("WrongActionForCtor", "action");
		}
		if (index < 0)
		{
			throw new ArgumentException("IndexCannotBeNegative", "index");
		}
		InitializeMoveOrReplace(action, changedItems, changedItems, index, oldIndex);
	}

	public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object changedItem, int index, int oldIndex)
	{
		_newStartingIndex = -1;
		_oldStartingIndex = -1;
		if (action != NotifyCollectionChangedAction.Move)
		{
			throw new ArgumentException("WrongActionForCtor", "action");
		}
		if (index < 0)
		{
			throw new ArgumentException("IndexCannotBeNegative", "index");
		}
		object[] array = new object[1] { changedItem };
		InitializeMoveOrReplace(action, array, array, index, oldIndex);
	}

	public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object newItem, object oldItem, int index)
	{
		_newStartingIndex = -1;
		_oldStartingIndex = -1;
		if (action != NotifyCollectionChangedAction.Replace)
		{
			throw new ArgumentException("WrongActionForCtor", "action");
		}
		InitializeMoveOrReplace(action, new object[1] { newItem }, new object[1] { oldItem }, index, index);
	}

	private void InitializeAdd(NotifyCollectionChangedAction action, IList newItems, int newStartingIndex)
	{
		_action = action;
		_newItems = ((newItems == null) ? null : ArrayList.ReadOnly(newItems));
		_newStartingIndex = newStartingIndex;
	}

	private void InitializeAddOrRemove(NotifyCollectionChangedAction action, IList changedItems, int startingIndex)
	{
		switch (action)
		{
		case NotifyCollectionChangedAction.Add:
			InitializeAdd(action, changedItems, startingIndex);
			break;
		case NotifyCollectionChangedAction.Remove:
			InitializeRemove(action, changedItems, startingIndex);
			break;
		default:
			throw new ArgumentException($"InvariantFailure, Unsupported action: {action.ToString()}");
		}
	}

	private void InitializeMoveOrReplace(NotifyCollectionChangedAction action, IList newItems, IList oldItems, int startingIndex, int oldStartingIndex)
	{
		InitializeAdd(action, newItems, startingIndex);
		InitializeRemove(action, oldItems, oldStartingIndex);
	}

	private void InitializeRemove(NotifyCollectionChangedAction action, IList oldItems, int oldStartingIndex)
	{
		_action = action;
		_oldItems = ((oldItems == null) ? null : ArrayList.ReadOnly(oldItems));
		_oldStartingIndex = oldStartingIndex;
	}
}
