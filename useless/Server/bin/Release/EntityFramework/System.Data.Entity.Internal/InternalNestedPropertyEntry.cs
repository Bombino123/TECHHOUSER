using System.Data.Entity.Resources;

namespace System.Data.Entity.Internal;

internal class InternalNestedPropertyEntry : InternalPropertyEntry
{
	private readonly InternalPropertyEntry _parentPropertyEntry;

	public override InternalPropertyEntry ParentPropertyEntry => _parentPropertyEntry;

	public override InternalPropertyValues ParentCurrentValues => (InternalPropertyValues)(_parentPropertyEntry.ParentCurrentValues?[_parentPropertyEntry.Name]);

	public override InternalPropertyValues ParentOriginalValues => (InternalPropertyValues)(_parentPropertyEntry.ParentOriginalValues?[_parentPropertyEntry.Name]);

	public InternalNestedPropertyEntry(InternalPropertyEntry parentPropertyEntry, PropertyEntryMetadata propertyMetadata)
		: base(parentPropertyEntry.InternalEntityEntry, propertyMetadata)
	{
		_parentPropertyEntry = parentPropertyEntry;
	}

	protected override Func<object, object> CreateGetter()
	{
		Func<object, object> parentGetter = _parentPropertyEntry.Getter;
		if (parentGetter == null)
		{
			return null;
		}
		if (!DbHelpers.GetPropertyGetters(base.EntryMetadata.DeclaringType).TryGetValue(Name, out var getter))
		{
			return null;
		}
		return delegate(object o)
		{
			object obj = parentGetter(o);
			return (obj != null) ? getter(obj) : null;
		};
	}

	protected override Action<object, object> CreateSetter()
	{
		Func<object, object> parentGetter = _parentPropertyEntry.Getter;
		if (parentGetter == null)
		{
			return null;
		}
		if (!DbHelpers.GetPropertySetters(base.EntryMetadata.DeclaringType).TryGetValue(Name, out var setter))
		{
			return null;
		}
		return delegate(object o, object v)
		{
			if (parentGetter(o) == null)
			{
				throw Error.DbPropertyValues_CannotSetPropertyOnNullCurrentValue(Name, ParentPropertyEntry.Name);
			}
			setter(parentGetter(o), v);
		};
	}

	public override bool EntityPropertyIsModified()
	{
		return _parentPropertyEntry.EntityPropertyIsModified();
	}

	public override void SetEntityPropertyModified()
	{
		_parentPropertyEntry.SetEntityPropertyModified();
	}

	public override void RejectEntityPropertyChanges()
	{
		CurrentValue = OriginalValue;
		UpdateComplexPropertyState();
	}

	public override void UpdateComplexPropertyState()
	{
		_parentPropertyEntry.UpdateComplexPropertyState();
	}
}
