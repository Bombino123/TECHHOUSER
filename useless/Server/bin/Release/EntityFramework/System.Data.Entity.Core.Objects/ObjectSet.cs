using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Globalization;
using System.Linq;

namespace System.Data.Entity.Core.Objects;

public class ObjectSet<TEntity> : ObjectQuery<TEntity>, IObjectSet<TEntity>, IQueryable<TEntity>, IEnumerable<TEntity>, IEnumerable, IQueryable where TEntity : class
{
	private readonly EntitySet _entitySet;

	public EntitySet EntitySet => _entitySet;

	private string FullyQualifiedEntitySetName => string.Format(CultureInfo.InvariantCulture, "{0}.{1}", new object[2]
	{
		_entitySet.EntityContainer.Name,
		_entitySet.Name
	});

	internal ObjectSet(EntitySet entitySet, ObjectContext context)
		: base((EntitySetBase)entitySet, context, MergeOption.AppendOnly)
	{
		_entitySet = entitySet;
	}

	public void AddObject(TEntity entity)
	{
		base.Context.AddObject(FullyQualifiedEntitySetName, entity);
	}

	public void Attach(TEntity entity)
	{
		base.Context.AttachTo(FullyQualifiedEntitySetName, entity);
	}

	public void DeleteObject(TEntity entity)
	{
		base.Context.DeleteObject(entity, EntitySet);
	}

	public void Detach(TEntity entity)
	{
		base.Context.Detach(entity, EntitySet);
	}

	public TEntity ApplyCurrentValues(TEntity currentEntity)
	{
		return base.Context.ApplyCurrentValues(FullyQualifiedEntitySetName, currentEntity);
	}

	public TEntity ApplyOriginalValues(TEntity originalEntity)
	{
		return base.Context.ApplyOriginalValues(FullyQualifiedEntitySetName, originalEntity);
	}

	public TEntity CreateObject()
	{
		return base.Context.CreateObject<TEntity>();
	}

	public T CreateObject<T>() where T : class, TEntity
	{
		return base.Context.CreateObject<T>();
	}
}
