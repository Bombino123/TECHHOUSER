using System.Data.Entity.Infrastructure;
using System.Data.Entity.Resources;

namespace System.Data.Entity.Internal;

internal abstract class InternalPropertyEntry : InternalMemberEntry
{
	private bool _getterIsCached;

	private Func<object, object> _getter;

	private bool _setterIsCached;

	private Action<object, object> _setter;

	public abstract InternalPropertyEntry ParentPropertyEntry { get; }

	public abstract InternalPropertyValues ParentCurrentValues { get; }

	public abstract InternalPropertyValues ParentOriginalValues { get; }

	public Func<object, object> Getter
	{
		get
		{
			if (!_getterIsCached)
			{
				_getter = CreateGetter();
				_getterIsCached = true;
			}
			return _getter;
		}
	}

	public Action<object, object> Setter
	{
		get
		{
			if (!_setterIsCached)
			{
				_setter = CreateSetter();
				_setterIsCached = true;
			}
			return _setter;
		}
	}

	public virtual object OriginalValue
	{
		get
		{
			ValidateNotDetachedAndInModel("OriginalValue");
			object obj = ParentOriginalValues?[Name];
			if (obj is InternalPropertyValues internalPropertyValues)
			{
				obj = internalPropertyValues.ToObject();
			}
			return obj;
		}
		set
		{
			ValidateNotDetachedAndInModel("OriginalValue");
			CheckNotSettingComplexPropertyToNull(value);
			InternalPropertyValues parentOriginalValues = ParentOriginalValues;
			if (parentOriginalValues == null)
			{
				throw Error.DbPropertyValues_CannotSetPropertyOnNullOriginalValue(Name, ParentPropertyEntry.Name);
			}
			SetPropertyValueUsingValues(parentOriginalValues, value);
		}
	}

	public override object CurrentValue
	{
		get
		{
			if (Getter != null)
			{
				return Getter(InternalEntityEntry.Entity);
			}
			if (!InternalEntityEntry.IsDetached && EntryMetadata.IsMapped)
			{
				object obj = ParentCurrentValues?[Name];
				if (obj is InternalPropertyValues internalPropertyValues)
				{
					obj = internalPropertyValues.ToObject();
				}
				return obj;
			}
			throw Error.DbPropertyEntry_CannotGetCurrentValue(Name, base.EntryMetadata.DeclaringType.Name);
		}
		set
		{
			CheckNotSettingComplexPropertyToNull(value);
			if (!EntryMetadata.IsMapped || InternalEntityEntry.IsDetached || InternalEntityEntry.State == EntityState.Deleted)
			{
				if (!SetCurrentValueOnClrObject(value))
				{
					throw Error.DbPropertyEntry_CannotSetCurrentValue(Name, base.EntryMetadata.DeclaringType.Name);
				}
				return;
			}
			InternalPropertyValues parentCurrentValues = ParentCurrentValues;
			if (parentCurrentValues == null)
			{
				throw Error.DbPropertyValues_CannotSetPropertyOnNullCurrentValue(Name, ParentPropertyEntry.Name);
			}
			SetPropertyValueUsingValues(parentCurrentValues, value);
			if (EntryMetadata.IsComplex)
			{
				SetCurrentValueOnClrObject(value);
			}
		}
	}

	public virtual bool IsModified
	{
		get
		{
			if (InternalEntityEntry.IsDetached || !EntryMetadata.IsMapped)
			{
				return false;
			}
			return EntityPropertyIsModified();
		}
		set
		{
			ValidateNotDetachedAndInModel("IsModified");
			if (value)
			{
				SetEntityPropertyModified();
			}
			else if (IsModified)
			{
				RejectEntityPropertyChanges();
			}
		}
	}

	public new PropertyEntryMetadata EntryMetadata => (PropertyEntryMetadata)base.EntryMetadata;

	protected InternalPropertyEntry(InternalEntityEntry internalEntityEntry, PropertyEntryMetadata propertyMetadata)
		: base(internalEntityEntry, propertyMetadata)
	{
	}

	protected abstract Func<object, object> CreateGetter();

	protected abstract Action<object, object> CreateSetter();

	public abstract bool EntityPropertyIsModified();

	public abstract void SetEntityPropertyModified();

	public abstract void RejectEntityPropertyChanges();

	public abstract void UpdateComplexPropertyState();

	private void CheckNotSettingComplexPropertyToNull(object value)
	{
		if (value == null && EntryMetadata.IsComplex)
		{
			throw Error.DbPropertyValues_ComplexObjectCannotBeNull(Name, base.EntryMetadata.DeclaringType.Name);
		}
	}

	private bool SetCurrentValueOnClrObject(object value)
	{
		if (Setter == null)
		{
			return false;
		}
		if (Getter == null || !DbHelpers.PropertyValuesEqual(value, Getter(InternalEntityEntry.Entity)))
		{
			Setter(InternalEntityEntry.Entity, value);
			if (EntryMetadata.IsMapped && (InternalEntityEntry.State == EntityState.Modified || InternalEntityEntry.State == EntityState.Unchanged))
			{
				IsModified = true;
			}
		}
		return true;
	}

	private void SetPropertyValueUsingValues(InternalPropertyValues internalValues, object value)
	{
		if (internalValues[Name] is InternalPropertyValues internalPropertyValues)
		{
			if (!internalPropertyValues.ObjectType.IsAssignableFrom(value.GetType()))
			{
				throw Error.DbPropertyValues_AttemptToSetValuesFromWrongObject(value.GetType().Name, internalPropertyValues.ObjectType.Name);
			}
			internalPropertyValues.SetValues(value);
		}
		else
		{
			internalValues[Name] = value;
		}
	}

	public virtual InternalPropertyEntry Property(string property, Type requestedType = null, bool requireComplex = false)
	{
		return InternalEntityEntry.Property(this, property, requestedType ?? typeof(object), requireComplex);
	}

	private void ValidateNotDetachedAndInModel(string method)
	{
		if (!EntryMetadata.IsMapped)
		{
			throw Error.DbPropertyEntry_NotSupportedForPropertiesNotInTheModel(method, base.EntryMetadata.MemberName, InternalEntityEntry.EntityType.Name);
		}
		if (InternalEntityEntry.IsDetached)
		{
			throw Error.DbPropertyEntry_NotSupportedForDetached(method, base.EntryMetadata.MemberName, InternalEntityEntry.EntityType.Name);
		}
	}

	public override DbMemberEntry CreateDbMemberEntry()
	{
		if (!EntryMetadata.IsComplex)
		{
			return new DbPropertyEntry(this);
		}
		return new DbComplexPropertyEntry(this);
	}

	public override DbMemberEntry<TEntity, TProperty> CreateDbMemberEntry<TEntity, TProperty>()
	{
		if (!EntryMetadata.IsComplex)
		{
			return new DbPropertyEntry<TEntity, TProperty>(this);
		}
		return new DbComplexPropertyEntry<TEntity, TProperty>(this);
	}
}
