using System.Collections;
using System.Collections.Concurrent;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.Internal;

internal class InternalReferenceEntry : InternalNavigationEntry
{
	private static readonly ConcurrentDictionary<Type, Action<IRelatedEnd, object>> _entityReferenceValueSetters = new ConcurrentDictionary<Type, Action<IRelatedEnd, object>>();

	public static readonly MethodInfo SetValueOnEntityReferenceMethod = typeof(InternalReferenceEntry).GetOnlyDeclaredMethod("SetValueOnEntityReference");

	public override object CurrentValue
	{
		get
		{
			return base.CurrentValue;
		}
		set
		{
			if (base.RelatedEnd != null && InternalEntityEntry.State != EntityState.Deleted)
			{
				SetNavigationPropertyOnRelatedEnd(value);
				return;
			}
			if (base.Setter != null)
			{
				base.Setter(InternalEntityEntry.Entity, value);
				return;
			}
			throw Error.DbPropertyEntry_SettingEntityRefNotSupported(Name, InternalEntityEntry.EntityType.Name, InternalEntityEntry.State);
		}
	}

	public InternalReferenceEntry(InternalEntityEntry internalEntityEntry, NavigationEntryMetadata navigationMetadata)
		: base(internalEntityEntry, navigationMetadata)
	{
	}

	protected override object GetNavigationPropertyFromRelatedEnd(object entity)
	{
		IEnumerator enumerator = base.RelatedEnd.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return null;
		}
		return enumerator.Current;
	}

	protected virtual void SetNavigationPropertyOnRelatedEnd(object value)
	{
		Type type = base.RelatedEnd.GetType();
		if (!_entityReferenceValueSetters.TryGetValue(type, out var value2))
		{
			MethodInfo method = SetValueOnEntityReferenceMethod.MakeGenericMethod(type.GetGenericArguments().Single());
			value2 = (Action<IRelatedEnd, object>)Delegate.CreateDelegate(typeof(Action<IRelatedEnd, object>), method);
			_entityReferenceValueSetters.TryAdd(type, value2);
		}
		value2(base.RelatedEnd, value);
	}

	private static void SetValueOnEntityReference<TRelatedEntity>(IRelatedEnd entityReference, object value) where TRelatedEntity : class
	{
		((EntityReference<TRelatedEntity>)entityReference).Value = (TRelatedEntity)value;
	}

	public override DbMemberEntry CreateDbMemberEntry()
	{
		return new DbReferenceEntry(this);
	}

	public override DbMemberEntry<TEntity, TProperty> CreateDbMemberEntry<TEntity, TProperty>()
	{
		return new DbReferenceEntry<TEntity, TProperty>(this);
	}
}
