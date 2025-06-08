using System.Data.Entity.Internal;
using System.Data.Entity.Resources;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Infrastructure;

public class DbReferenceEntry : DbMemberEntry
{
	private readonly InternalReferenceEntry _internalReferenceEntry;

	public override string Name => _internalReferenceEntry.Name;

	public override object CurrentValue
	{
		get
		{
			return _internalReferenceEntry.CurrentValue;
		}
		set
		{
			_internalReferenceEntry.CurrentValue = value;
		}
	}

	public bool IsLoaded
	{
		get
		{
			return _internalReferenceEntry.IsLoaded;
		}
		set
		{
			_internalReferenceEntry.IsLoaded = value;
		}
	}

	public override DbEntityEntry EntityEntry => new DbEntityEntry(_internalReferenceEntry.InternalEntityEntry);

	internal override InternalMemberEntry InternalMemberEntry => _internalReferenceEntry;

	internal static DbReferenceEntry Create(InternalReferenceEntry internalReferenceEntry)
	{
		return (DbReferenceEntry)internalReferenceEntry.CreateDbMemberEntry();
	}

	internal DbReferenceEntry(InternalReferenceEntry internalReferenceEntry)
	{
		_internalReferenceEntry = internalReferenceEntry;
	}

	public void Load()
	{
		_internalReferenceEntry.Load();
	}

	public Task LoadAsync()
	{
		return LoadAsync(CancellationToken.None);
	}

	public Task LoadAsync(CancellationToken cancellationToken)
	{
		return _internalReferenceEntry.LoadAsync(cancellationToken);
	}

	public IQueryable Query()
	{
		return _internalReferenceEntry.Query();
	}

	public new DbReferenceEntry<TEntity, TProperty> Cast<TEntity, TProperty>() where TEntity : class
	{
		MemberEntryMetadata entryMetadata = _internalReferenceEntry.EntryMetadata;
		if (!typeof(TEntity).IsAssignableFrom(entryMetadata.DeclaringType) || !typeof(TProperty).IsAssignableFrom(entryMetadata.ElementType))
		{
			throw Error.DbMember_BadTypeForCast(typeof(DbReferenceEntry).Name, typeof(TEntity).Name, typeof(TProperty).Name, entryMetadata.DeclaringType.Name, entryMetadata.MemberType.Name);
		}
		return DbReferenceEntry<TEntity, TProperty>.Create(_internalReferenceEntry);
	}
}
public class DbReferenceEntry<TEntity, TProperty> : DbMemberEntry<TEntity, TProperty> where TEntity : class
{
	private readonly InternalReferenceEntry _internalReferenceEntry;

	public override string Name => _internalReferenceEntry.Name;

	public override TProperty CurrentValue
	{
		get
		{
			return (TProperty)_internalReferenceEntry.CurrentValue;
		}
		set
		{
			_internalReferenceEntry.CurrentValue = value;
		}
	}

	public bool IsLoaded
	{
		get
		{
			return _internalReferenceEntry.IsLoaded;
		}
		set
		{
			_internalReferenceEntry.IsLoaded = value;
		}
	}

	internal override InternalMemberEntry InternalMemberEntry => _internalReferenceEntry;

	public override DbEntityEntry<TEntity> EntityEntry => new DbEntityEntry<TEntity>(_internalReferenceEntry.InternalEntityEntry);

	internal static DbReferenceEntry<TEntity, TProperty> Create(InternalReferenceEntry internalReferenceEntry)
	{
		return (DbReferenceEntry<TEntity, TProperty>)internalReferenceEntry.CreateDbMemberEntry<TEntity, TProperty>();
	}

	internal DbReferenceEntry(InternalReferenceEntry internalReferenceEntry)
	{
		_internalReferenceEntry = internalReferenceEntry;
	}

	public void Load()
	{
		_internalReferenceEntry.Load();
	}

	public Task LoadAsync()
	{
		return LoadAsync(CancellationToken.None);
	}

	public Task LoadAsync(CancellationToken cancellationToken)
	{
		return _internalReferenceEntry.LoadAsync(cancellationToken);
	}

	public IQueryable<TProperty> Query()
	{
		return (IQueryable<TProperty>)_internalReferenceEntry.Query();
	}

	public static implicit operator DbReferenceEntry(DbReferenceEntry<TEntity, TProperty> entry)
	{
		return DbReferenceEntry.Create(entry._internalReferenceEntry);
	}
}
