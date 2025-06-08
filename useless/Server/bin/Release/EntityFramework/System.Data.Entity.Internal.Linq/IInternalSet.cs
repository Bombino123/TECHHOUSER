using System.Collections;
using System.Collections.ObjectModel;
using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Internal.Linq;

internal interface IInternalSet : IInternalQuery
{
	void Attach(object entity);

	void Add(object entity);

	void AddRange(IEnumerable entities);

	void RemoveRange(IEnumerable entities);

	void Remove(object entity);

	void Initialize();

	void TryInitialize();

	IEnumerator ExecuteSqlQuery(string sql, bool asNoTracking, bool? streaming, object[] parameters);

	IDbAsyncEnumerator ExecuteSqlQueryAsync(string sql, bool asNoTracking, bool? streaming, object[] parameters);
}
internal interface IInternalSet<TEntity> : IInternalSet, IInternalQuery, IInternalQuery<TEntity> where TEntity : class
{
	ObservableCollection<TEntity> Local { get; }

	TEntity Find(params object[] keyValues);

	Task<TEntity> FindAsync(CancellationToken cancellationToken, params object[] keyValues);

	TEntity Create();

	TEntity Create(Type derivedEntityType);
}
