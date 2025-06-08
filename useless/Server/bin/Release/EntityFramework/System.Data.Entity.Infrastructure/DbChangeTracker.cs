using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Internal;
using System.Linq;

namespace System.Data.Entity.Infrastructure;

public class DbChangeTracker
{
	private readonly InternalContext _internalContext;

	internal DbChangeTracker(InternalContext internalContext)
	{
		_internalContext = internalContext;
	}

	public IEnumerable<DbEntityEntry> Entries()
	{
		return from e in _internalContext.GetStateEntries()
			select new DbEntityEntry(new InternalEntityEntry(_internalContext, e));
	}

	public IEnumerable<DbEntityEntry<TEntity>> Entries<TEntity>() where TEntity : class
	{
		return from e in _internalContext.GetStateEntries<TEntity>()
			select new DbEntityEntry<TEntity>(new InternalEntityEntry(_internalContext, e));
	}

	public bool HasChanges()
	{
		_internalContext.DetectChanges();
		return _internalContext.ObjectContext.ObjectStateManager.HasChanges();
	}

	public void DetectChanges()
	{
		_internalContext.DetectChanges(force: true);
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
