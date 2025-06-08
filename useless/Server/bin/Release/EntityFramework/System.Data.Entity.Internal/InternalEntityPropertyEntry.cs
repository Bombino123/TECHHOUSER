using System.Linq;

namespace System.Data.Entity.Internal;

internal class InternalEntityPropertyEntry : InternalPropertyEntry
{
	public override InternalPropertyEntry ParentPropertyEntry => null;

	public override InternalPropertyValues ParentCurrentValues => InternalEntityEntry.CurrentValues;

	public override InternalPropertyValues ParentOriginalValues => InternalEntityEntry.OriginalValues;

	public InternalEntityPropertyEntry(InternalEntityEntry internalEntityEntry, PropertyEntryMetadata propertyMetadata)
		: base(internalEntityEntry, propertyMetadata)
	{
	}

	protected override Func<object, object> CreateGetter()
	{
		DbHelpers.GetPropertyGetters(InternalEntityEntry.EntityType).TryGetValue(Name, out var value);
		return value;
	}

	protected override Action<object, object> CreateSetter()
	{
		DbHelpers.GetPropertySetters(InternalEntityEntry.EntityType).TryGetValue(Name, out var value);
		return value;
	}

	public override bool EntityPropertyIsModified()
	{
		return InternalEntityEntry.ObjectStateEntry.GetModifiedProperties().Contains(Name);
	}

	public override void SetEntityPropertyModified()
	{
		InternalEntityEntry.ObjectStateEntry.SetModifiedProperty(Name);
	}

	public override void RejectEntityPropertyChanges()
	{
		InternalEntityEntry.ObjectStateEntry.RejectPropertyChanges(Name);
	}

	public override void UpdateComplexPropertyState()
	{
		if (!InternalEntityEntry.ObjectStateEntry.IsPropertyChanged(Name))
		{
			RejectEntityPropertyChanges();
		}
	}
}
