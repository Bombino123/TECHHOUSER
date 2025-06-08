using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Reflection;

namespace System.Data.Entity.Internal;

internal class InternalCollectionEntry : InternalNavigationEntry
{
	private static readonly ConcurrentDictionary<Type, Func<InternalCollectionEntry, object>> _entryFactories = new ConcurrentDictionary<Type, Func<InternalCollectionEntry, object>>();

	public override object CurrentValue
	{
		get
		{
			return base.CurrentValue;
		}
		set
		{
			if (base.Setter != null)
			{
				base.Setter(InternalEntityEntry.Entity, value);
			}
			else if (InternalEntityEntry.IsDetached || base.RelatedEnd != value)
			{
				throw Error.DbCollectionEntry_CannotSetCollectionProp(Name, InternalEntityEntry.Entity.GetType().ToString());
			}
		}
	}

	public InternalCollectionEntry(InternalEntityEntry internalEntityEntry, NavigationEntryMetadata navigationMetadata)
		: base(internalEntityEntry, navigationMetadata)
	{
	}

	protected override object GetNavigationPropertyFromRelatedEnd(object entity)
	{
		return base.RelatedEnd;
	}

	public override DbMemberEntry CreateDbMemberEntry()
	{
		return new DbCollectionEntry(this);
	}

	public override DbMemberEntry<TEntity, TProperty> CreateDbMemberEntry<TEntity, TProperty>()
	{
		return CreateDbCollectionEntry<TEntity, TProperty>(EntryMetadata.ElementType);
	}

	public virtual DbCollectionEntry<TEntity, TElement> CreateDbCollectionEntry<TEntity, TElement>() where TEntity : class
	{
		return new DbCollectionEntry<TEntity, TElement>(this);
	}

	private DbMemberEntry<TEntity, TProperty> CreateDbCollectionEntry<TEntity, TProperty>(Type elementType) where TEntity : class
	{
		Type typeFromHandle = typeof(DbMemberEntry<TEntity, TProperty>);
		if (!_entryFactories.TryGetValue(typeFromHandle, out var value))
		{
			Type type = typeof(DbCollectionEntry<, >).MakeGenericType(typeof(TEntity), elementType);
			if (!typeFromHandle.IsAssignableFrom(type))
			{
				throw Error.DbEntityEntry_WrongGenericForCollectionNavProp(typeof(TProperty), Name, EntryMetadata.DeclaringType, typeof(ICollection<>).MakeGenericType(elementType));
			}
			MethodInfo declaredMethod = type.GetDeclaredMethod("Create", typeof(InternalCollectionEntry));
			value = (Func<InternalCollectionEntry, object>)Delegate.CreateDelegate(typeof(Func<InternalCollectionEntry, object>), declaredMethod);
			_entryFactories.TryAdd(typeFromHandle, value);
		}
		return (DbMemberEntry<TEntity, TProperty>)value(this);
	}
}
