using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace System.Data.Entity;

public interface IDbSet<TEntity> : IQueryable<TEntity>, IEnumerable<TEntity>, IEnumerable, IQueryable where TEntity : class
{
	ObservableCollection<TEntity> Local { get; }

	TEntity Find(params object[] keyValues);

	TEntity Add(TEntity entity);

	TEntity Remove(TEntity entity);

	TEntity Attach(TEntity entity);

	TEntity Create();

	TDerivedEntity Create<TDerivedEntity>() where TDerivedEntity : class, TEntity;
}
