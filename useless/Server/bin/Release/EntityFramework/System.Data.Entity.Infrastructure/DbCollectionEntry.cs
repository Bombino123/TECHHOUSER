using System.Collections.Generic;
using System.Data.Entity.Internal;
using System.Data.Entity.Resources;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Infrastructure;

public class DbCollectionEntry : DbMemberEntry
{
	private readonly InternalCollectionEntry _internalCollectionEntry;

	public override string Name => _internalCollectionEntry.Name;

	public override object CurrentValue
	{
		get
		{
			return _internalCollectionEntry.CurrentValue;
		}
		set
		{
			_internalCollectionEntry.CurrentValue = value;
		}
	}

	public bool IsLoaded
	{
		get
		{
			return _internalCollectionEntry.IsLoaded;
		}
		set
		{
			_internalCollectionEntry.IsLoaded = value;
		}
	}

	public override DbEntityEntry EntityEntry => new DbEntityEntry(_internalCollectionEntry.InternalEntityEntry);

	internal override InternalMemberEntry InternalMemberEntry => _internalCollectionEntry;

	internal static DbCollectionEntry Create(InternalCollectionEntry internalCollectionEntry)
	{
		return (DbCollectionEntry)internalCollectionEntry.CreateDbMemberEntry();
	}

	internal DbCollectionEntry(InternalCollectionEntry internalCollectionEntry)
	{
		_internalCollectionEntry = internalCollectionEntry;
	}

	public void Load()
	{
		_internalCollectionEntry.Load();
	}

	public Task LoadAsync()
	{
		return LoadAsync(CancellationToken.None);
	}

	public Task LoadAsync(CancellationToken cancellationToken)
	{
		return _internalCollectionEntry.LoadAsync(cancellationToken);
	}

	public IQueryable Query()
	{
		return _internalCollectionEntry.Query();
	}

	public new DbCollectionEntry<TEntity, TElement> Cast<TEntity, TElement>() where TEntity : class
	{
		MemberEntryMetadata entryMetadata = _internalCollectionEntry.EntryMetadata;
		if (!typeof(TEntity).IsAssignableFrom(entryMetadata.DeclaringType) || !typeof(TElement).IsAssignableFrom(entryMetadata.ElementType))
		{
			throw Error.DbMember_BadTypeForCast(typeof(DbCollectionEntry).Name, typeof(TEntity).Name, typeof(TElement).Name, entryMetadata.DeclaringType.Name, entryMetadata.ElementType.Name);
		}
		return DbCollectionEntry<TEntity, TElement>.Create(_internalCollectionEntry);
	}
}
public class DbCollectionEntry<TEntity, TElement> : DbMemberEntry<TEntity, ICollection<TElement>> where TEntity : class
{
	private readonly InternalCollectionEntry _internalCollectionEntry;

	public override string Name => _internalCollectionEntry.Name;

	public override ICollection<TElement> CurrentValue
	{
		get
		{
			return (ICollection<TElement>)_internalCollectionEntry.CurrentValue;
		}
		set
		{
			_internalCollectionEntry.CurrentValue = value;
		}
	}

	public bool IsLoaded
	{
		get
		{
			return _internalCollectionEntry.IsLoaded;
		}
		set
		{
			_internalCollectionEntry.IsLoaded = value;
		}
	}

	internal override InternalMemberEntry InternalMemberEntry => _internalCollectionEntry;

	public override DbEntityEntry<TEntity> EntityEntry => new DbEntityEntry<TEntity>(_internalCollectionEntry.InternalEntityEntry);

	internal static DbCollectionEntry<TEntity, TElement> Create(InternalCollectionEntry internalCollectionEntry)
	{
		return internalCollectionEntry.CreateDbCollectionEntry<TEntity, TElement>();
	}

	internal DbCollectionEntry(InternalCollectionEntry internalCollectionEntry)
	{
		_internalCollectionEntry = internalCollectionEntry;
	}

	public void Load()
	{
		_internalCollectionEntry.Load();
	}

	public Task LoadAsync()
	{
		return LoadAsync(CancellationToken.None);
	}

	public Task LoadAsync(CancellationToken cancellationToken)
	{
		return _internalCollectionEntry.LoadAsync(cancellationToken);
	}

	public IQueryable<TElement> Query()
	{
		return (IQueryable<TElement>)_internalCollectionEntry.Query();
	}

	public static implicit operator DbCollectionEntry(DbCollectionEntry<TEntity, TElement> entry)
	{
		return DbCollectionEntry.Create(entry._internalCollectionEntry);
	}
}
