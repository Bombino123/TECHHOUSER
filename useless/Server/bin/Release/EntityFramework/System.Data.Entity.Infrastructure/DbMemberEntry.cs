using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Internal;
using System.Data.Entity.Resources;
using System.Data.Entity.Validation;
using System.Linq;

namespace System.Data.Entity.Infrastructure;

public abstract class DbMemberEntry
{
	public abstract string Name { get; }

	public abstract object CurrentValue { get; set; }

	public abstract DbEntityEntry EntityEntry { get; }

	internal abstract InternalMemberEntry InternalMemberEntry { get; }

	internal static DbMemberEntry Create(InternalMemberEntry internalMemberEntry)
	{
		return internalMemberEntry.CreateDbMemberEntry();
	}

	public ICollection<DbValidationError> GetValidationErrors()
	{
		return InternalMemberEntry.GetValidationErrors().ToList();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override string ToString()
	{
		return base.ToString();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override bool Equals(object obj)
	{
		return base.Equals(obj);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public new Type GetType()
	{
		return base.GetType();
	}

	public DbMemberEntry<TEntity, TProperty> Cast<TEntity, TProperty>() where TEntity : class
	{
		MemberEntryMetadata entryMetadata = InternalMemberEntry.EntryMetadata;
		if (!typeof(TEntity).IsAssignableFrom(entryMetadata.DeclaringType) || !typeof(TProperty).IsAssignableFrom(entryMetadata.MemberType))
		{
			throw Error.DbMember_BadTypeForCast(typeof(DbMemberEntry).Name, typeof(TEntity).Name, typeof(TProperty).Name, entryMetadata.DeclaringType.Name, entryMetadata.MemberType.Name);
		}
		return DbMemberEntry<TEntity, TProperty>.Create(InternalMemberEntry);
	}
}
public abstract class DbMemberEntry<TEntity, TProperty> where TEntity : class
{
	public abstract string Name { get; }

	public abstract TProperty CurrentValue { get; set; }

	internal abstract InternalMemberEntry InternalMemberEntry { get; }

	public abstract DbEntityEntry<TEntity> EntityEntry { get; }

	internal static DbMemberEntry<TEntity, TProperty> Create(InternalMemberEntry internalMemberEntry)
	{
		return internalMemberEntry.CreateDbMemberEntry<TEntity, TProperty>();
	}

	public static implicit operator DbMemberEntry(DbMemberEntry<TEntity, TProperty> entry)
	{
		return DbMemberEntry.Create(entry.InternalMemberEntry);
	}

	public ICollection<DbValidationError> GetValidationErrors()
	{
		return InternalMemberEntry.GetValidationErrors().ToList();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override string ToString()
	{
		return base.ToString();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override bool Equals(object obj)
	{
		return base.Equals(obj);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public new Type GetType()
	{
		return base.GetType();
	}
}
