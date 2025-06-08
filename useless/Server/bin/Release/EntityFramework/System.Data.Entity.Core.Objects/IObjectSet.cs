using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace System.Data.Entity.Core.Objects;

public interface IObjectSet<TEntity> : IQueryable<TEntity>, IEnumerable<TEntity>, IEnumerable, IQueryable where TEntity : class
{
	void AddObject(TEntity entity);

	void Attach(TEntity entity);

	void DeleteObject(TEntity entity);

	void Detach(TEntity entity);
}
