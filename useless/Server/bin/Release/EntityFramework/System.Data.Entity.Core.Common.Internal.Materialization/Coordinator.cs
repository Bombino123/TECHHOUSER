using System.Collections.Generic;
using System.Data.Entity.Core.Objects.Internal;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Core.Common.Internal.Materialization;

internal abstract class Coordinator
{
	internal readonly CoordinatorFactory CoordinatorFactory;

	internal readonly Coordinator Parent;

	internal readonly Coordinator Next;

	public Coordinator Child { get; protected set; }

	public bool IsEntered { get; protected set; }

	internal bool IsRoot => Parent == null;

	protected Coordinator(CoordinatorFactory coordinatorFactory, Coordinator parent, Coordinator next)
	{
		CoordinatorFactory = coordinatorFactory;
		Parent = parent;
		Next = next;
	}

	internal void Initialize(Shaper shaper)
	{
		ResetCollection(shaper);
		shaper.State[CoordinatorFactory.StateSlot] = this;
		if (Child != null)
		{
			Child.Initialize(shaper);
		}
		if (Next != null)
		{
			Next.Initialize(shaper);
		}
	}

	internal int MaxDistanceToLeaf()
	{
		int num = 0;
		for (Coordinator coordinator = Child; coordinator != null; coordinator = coordinator.Next)
		{
			num = Math.Max(num, coordinator.MaxDistanceToLeaf() + 1);
		}
		return num;
	}

	internal abstract void ResetCollection(Shaper shaper);

	internal bool HasNextElement(Shaper shaper)
	{
		bool result = false;
		if (!IsEntered || !CoordinatorFactory.CheckKeys(shaper))
		{
			CoordinatorFactory.SetKeys(shaper);
			IsEntered = true;
			result = true;
		}
		return result;
	}

	internal abstract void ReadNextElement(Shaper shaper);
}
internal class Coordinator<T> : Coordinator
{
	internal readonly CoordinatorFactory<T> TypedCoordinatorFactory;

	private T _current;

	private ICollection<T> _elements;

	private List<IEntityWrapper> _wrappedElements;

	private Action<Shaper, List<IEntityWrapper>> _handleClose;

	private readonly bool IsUsingElementCollection;

	internal virtual T Current => _current;

	internal Coordinator(CoordinatorFactory<T> coordinatorFactory, Coordinator parent, Coordinator next)
		: base(coordinatorFactory, parent, next)
	{
		TypedCoordinatorFactory = coordinatorFactory;
		Coordinator next2 = null;
		foreach (CoordinatorFactory item in coordinatorFactory.NestedCoordinators.Reverse())
		{
			base.Child = item.CreateCoordinator(this, next2);
			next2 = base.Child;
		}
		IsUsingElementCollection = !base.IsRoot && typeof(T) != typeof(RecordState);
	}

	internal override void ResetCollection(Shaper shaper)
	{
		if (_handleClose != null)
		{
			_handleClose(shaper, _wrappedElements);
			_handleClose = null;
		}
		base.IsEntered = false;
		if (IsUsingElementCollection)
		{
			_elements = TypedCoordinatorFactory.InitializeCollection(shaper);
			_wrappedElements = new List<IEntityWrapper>();
		}
		if (base.Child != null)
		{
			base.Child.ResetCollection(shaper);
		}
		if (Next != null)
		{
			Next.ResetCollection(shaper);
		}
	}

	internal override void ReadNextElement(Shaper shaper)
	{
		IEntityWrapper entityWrapper = null;
		T val;
		try
		{
			if (TypedCoordinatorFactory.WrappedElement == null)
			{
				val = TypedCoordinatorFactory.Element(shaper);
			}
			else
			{
				entityWrapper = TypedCoordinatorFactory.WrappedElement(shaper);
				val = (T)entityWrapper.Entity;
			}
		}
		catch (Exception e)
		{
			if (e.IsCatchableExceptionType() && !shaper.Reader.IsClosed)
			{
				ResetCollection(shaper);
				val = TypedCoordinatorFactory.ElementWithErrorHandling(shaper);
			}
			throw;
		}
		if (IsUsingElementCollection)
		{
			_elements.Add(val);
			if (entityWrapper != null)
			{
				_wrappedElements.Add(entityWrapper);
			}
		}
		else
		{
			_current = val;
		}
	}

	internal void RegisterCloseHandler(Action<Shaper, List<IEntityWrapper>> closeHandler)
	{
		_handleClose = closeHandler;
	}

	internal void SetCurrentToDefault()
	{
		_current = default(T);
	}

	private IEnumerable<T> GetElements()
	{
		return _elements;
	}
}
