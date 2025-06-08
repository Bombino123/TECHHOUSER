using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Internal;
using System.Data.Entity.Internal.Linq;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity;

public abstract class DbSet : DbQuery, IInternalSetAdapter
{
	public virtual IList Local
	{
		get
		{
			throw new NotImplementedException(Strings.TestDoubleNotImplemented("Local", GetType().Name, typeof(DbSet).Name));
		}
	}

	IInternalSet IInternalSetAdapter.InternalSet => InternalSet;

	internal virtual IInternalSet InternalSet => null;

	protected internal DbSet()
	{
	}

	public virtual object Find(params object[] keyValues)
	{
		throw new NotImplementedException(Strings.TestDoubleNotImplemented("Find", GetType().Name, typeof(DbSet).Name));
	}

	public virtual Task<object> FindAsync(params object[] keyValues)
	{
		return FindAsync(CancellationToken.None, keyValues);
	}

	public virtual Task<object> FindAsync(CancellationToken cancellationToken, params object[] keyValues)
	{
		throw new NotImplementedException(Strings.TestDoubleNotImplemented("FindAsync", GetType().Name, typeof(DbSet).Name));
	}

	public virtual object Attach(object entity)
	{
		Check.NotNull(entity, "entity");
		GetInternalSetWithCheck("Attach").Attach(entity);
		return entity;
	}

	public virtual object Add(object entity)
	{
		Check.NotNull(entity, "entity");
		GetInternalSetWithCheck("Add").Add(entity);
		return entity;
	}

	public virtual IEnumerable AddRange(IEnumerable entities)
	{
		Check.NotNull(entities, "entities");
		GetInternalSetWithCheck("AddRange").AddRange(entities);
		return entities;
	}

	public virtual object Remove(object entity)
	{
		Check.NotNull(entity, "entity");
		GetInternalSetWithCheck("Remove").Remove(entity);
		return entity;
	}

	public virtual IEnumerable RemoveRange(IEnumerable entities)
	{
		Check.NotNull(entities, "entities");
		GetInternalSetWithCheck("RemoveRange").RemoveRange(entities);
		return entities;
	}

	public virtual object Create()
	{
		throw new NotImplementedException(Strings.TestDoubleNotImplemented("Create", GetType().Name, typeof(DbSet).Name));
	}

	public virtual object Create(Type derivedEntityType)
	{
		throw new NotImplementedException(Strings.TestDoubleNotImplemented("Create", GetType().Name, typeof(DbSet).Name));
	}

	public new DbSet<TEntity> Cast<TEntity>() where TEntity : class
	{
		if (InternalSet == null)
		{
			throw new NotSupportedException(Strings.TestDoublesCannotBeConverted);
		}
		if (typeof(TEntity) != InternalSet.ElementType)
		{
			throw Error.DbEntity_BadTypeForCast(typeof(DbSet).Name, typeof(TEntity).Name, InternalSet.ElementType.Name);
		}
		return (DbSet<TEntity>)InternalSet.InternalContext.Set<TEntity>();
	}

	internal virtual IInternalSet GetInternalSetWithCheck(string memberName)
	{
		throw new NotImplementedException(Strings.TestDoubleNotImplemented(memberName, GetType().Name, typeof(DbSet).Name));
	}

	public virtual DbSqlQuery SqlQuery(string sql, params object[] parameters)
	{
		Check.NotEmpty(sql, "sql");
		Check.NotNull(parameters, "parameters");
		return new DbSqlQuery((InternalSet == null) ? null : new InternalSqlSetQuery(InternalSet, sql, isNoTracking: false, parameters));
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
public class DbSet<TEntity> : DbQuery<TEntity>, IDbSet<TEntity>, IQueryable<TEntity>, IEnumerable<TEntity>, IEnumerable, IQueryable, IInternalSetAdapter where TEntity : class
{
	private readonly InternalSet<TEntity> _internalSet;

	public virtual ObservableCollection<TEntity> Local => GetInternalSetWithCheck("Local").Local;

	IInternalSet IInternalSetAdapter.InternalSet => _internalSet;

	internal DbSet(InternalSet<TEntity> internalSet)
		: base((IInternalQuery<TEntity>)internalSet)
	{
		_internalSet = internalSet;
	}

	protected DbSet()
		: this((InternalSet<TEntity>)null)
	{
	}

	public virtual TEntity Find(params object[] keyValues)
	{
		return GetInternalSetWithCheck("Find").Find(keyValues);
	}

	public virtual Task<TEntity> FindAsync(CancellationToken cancellationToken, params object[] keyValues)
	{
		return GetInternalSetWithCheck("FindAsync").FindAsync(cancellationToken, keyValues);
	}

	public virtual Task<TEntity> FindAsync(params object[] keyValues)
	{
		return FindAsync(CancellationToken.None, keyValues);
	}

	public virtual TEntity Attach(TEntity entity)
	{
		Check.NotNull(entity, "entity");
		GetInternalSetWithCheck("Attach").Attach(entity);
		return entity;
	}

	public virtual TEntity Add(TEntity entity)
	{
		Check.NotNull(entity, "entity");
		GetInternalSetWithCheck("Add").Add(entity);
		return entity;
	}

	public virtual IEnumerable<TEntity> AddRange(IEnumerable<TEntity> entities)
	{
		Check.NotNull(entities, "entities");
		GetInternalSetWithCheck("AddRange").AddRange(entities);
		return entities;
	}

	public virtual TEntity Remove(TEntity entity)
	{
		Check.NotNull(entity, "entity");
		GetInternalSetWithCheck("Remove").Remove(entity);
		return entity;
	}

	public virtual IEnumerable<TEntity> RemoveRange(IEnumerable<TEntity> entities)
	{
		Check.NotNull(entities, "entities");
		GetInternalSetWithCheck("RemoveRange").RemoveRange(entities);
		return entities;
	}

	public virtual TEntity Create()
	{
		return GetInternalSetWithCheck("Create").Create();
	}

	public virtual TDerivedEntity Create<TDerivedEntity>() where TDerivedEntity : class, TEntity
	{
		return (TDerivedEntity)GetInternalSetWithCheck("Create").Create(typeof(TDerivedEntity));
	}

	public static implicit operator DbSet(DbSet<TEntity> entry)
	{
		Check.NotNull(entry, "entry");
		if (entry._internalSet == null)
		{
			throw new NotSupportedException(Strings.TestDoublesCannotBeConverted);
		}
		return (DbSet)entry._internalSet.InternalContext.Set(entry._internalSet.ElementType);
	}

	private InternalSet<TEntity> GetInternalSetWithCheck(string memberName)
	{
		if (_internalSet == null)
		{
			throw new NotImplementedException(Strings.TestDoubleNotImplemented(memberName, GetType().Name, typeof(DbSet<>).Name));
		}
		return _internalSet;
	}

	public virtual DbSqlQuery<TEntity> SqlQuery(string sql, params object[] parameters)
	{
		Check.NotEmpty(sql, "sql");
		Check.NotNull(parameters, "parameters");
		return new DbSqlQuery<TEntity>((_internalSet != null) ? new InternalSqlSetQuery(_internalSet, sql, isNoTracking: false, parameters) : null);
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
