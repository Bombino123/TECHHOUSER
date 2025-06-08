using System.Data.Entity.Internal;
using System.Data.Entity.Resources;

namespace System.Data.Entity.Infrastructure;

public class DbPropertyEntry : DbMemberEntry
{
	private readonly InternalPropertyEntry _internalPropertyEntry;

	public override string Name => _internalPropertyEntry.Name;

	public object OriginalValue
	{
		get
		{
			return _internalPropertyEntry.OriginalValue;
		}
		set
		{
			_internalPropertyEntry.OriginalValue = value;
		}
	}

	public override object CurrentValue
	{
		get
		{
			return _internalPropertyEntry.CurrentValue;
		}
		set
		{
			_internalPropertyEntry.CurrentValue = value;
		}
	}

	public bool IsModified
	{
		get
		{
			return _internalPropertyEntry.IsModified;
		}
		set
		{
			_internalPropertyEntry.IsModified = value;
		}
	}

	public override DbEntityEntry EntityEntry => new DbEntityEntry(_internalPropertyEntry.InternalEntityEntry);

	public DbComplexPropertyEntry ParentProperty
	{
		get
		{
			InternalPropertyEntry parentPropertyEntry = _internalPropertyEntry.ParentPropertyEntry;
			if (parentPropertyEntry == null)
			{
				return null;
			}
			return DbComplexPropertyEntry.Create(parentPropertyEntry);
		}
	}

	internal override InternalMemberEntry InternalMemberEntry => _internalPropertyEntry;

	internal static DbPropertyEntry Create(InternalPropertyEntry internalPropertyEntry)
	{
		return (DbPropertyEntry)internalPropertyEntry.CreateDbMemberEntry();
	}

	internal DbPropertyEntry(InternalPropertyEntry internalPropertyEntry)
	{
		_internalPropertyEntry = internalPropertyEntry;
	}

	public new DbPropertyEntry<TEntity, TProperty> Cast<TEntity, TProperty>() where TEntity : class
	{
		PropertyEntryMetadata entryMetadata = _internalPropertyEntry.EntryMetadata;
		if (!typeof(TEntity).IsAssignableFrom(entryMetadata.DeclaringType) || !typeof(TProperty).IsAssignableFrom(entryMetadata.ElementType))
		{
			throw Error.DbMember_BadTypeForCast(typeof(DbPropertyEntry).Name, typeof(TEntity).Name, typeof(TProperty).Name, entryMetadata.DeclaringType.Name, entryMetadata.MemberType.Name);
		}
		return DbPropertyEntry<TEntity, TProperty>.Create(_internalPropertyEntry);
	}
}
public class DbPropertyEntry<TEntity, TProperty> : DbMemberEntry<TEntity, TProperty> where TEntity : class
{
	private readonly InternalPropertyEntry _internalPropertyEntry;

	public override string Name => _internalPropertyEntry.Name;

	public TProperty OriginalValue
	{
		get
		{
			return (TProperty)_internalPropertyEntry.OriginalValue;
		}
		set
		{
			_internalPropertyEntry.OriginalValue = value;
		}
	}

	public override TProperty CurrentValue
	{
		get
		{
			return (TProperty)_internalPropertyEntry.CurrentValue;
		}
		set
		{
			_internalPropertyEntry.CurrentValue = value;
		}
	}

	public bool IsModified
	{
		get
		{
			return _internalPropertyEntry.IsModified;
		}
		set
		{
			_internalPropertyEntry.IsModified = value;
		}
	}

	public override DbEntityEntry<TEntity> EntityEntry => new DbEntityEntry<TEntity>(_internalPropertyEntry.InternalEntityEntry);

	public DbComplexPropertyEntry ParentProperty
	{
		get
		{
			InternalPropertyEntry parentPropertyEntry = _internalPropertyEntry.ParentPropertyEntry;
			if (parentPropertyEntry == null)
			{
				return null;
			}
			return DbComplexPropertyEntry.Create(parentPropertyEntry);
		}
	}

	internal InternalPropertyEntry InternalPropertyEntry => _internalPropertyEntry;

	internal override InternalMemberEntry InternalMemberEntry => InternalPropertyEntry;

	internal static DbPropertyEntry<TEntity, TProperty> Create(InternalPropertyEntry internalPropertyEntry)
	{
		return (DbPropertyEntry<TEntity, TProperty>)internalPropertyEntry.CreateDbMemberEntry<TEntity, TProperty>();
	}

	internal DbPropertyEntry(InternalPropertyEntry internalPropertyEntry)
	{
		_internalPropertyEntry = internalPropertyEntry;
	}

	public static implicit operator DbPropertyEntry(DbPropertyEntry<TEntity, TProperty> entry)
	{
		return DbPropertyEntry.Create(entry._internalPropertyEntry);
	}
}
