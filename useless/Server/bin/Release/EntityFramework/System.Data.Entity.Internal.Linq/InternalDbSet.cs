using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Internal.Linq;

internal class InternalDbSet<TEntity> : DbSet, IQueryable<TEntity>, IEnumerable<TEntity>, IEnumerable, IQueryable, IDbAsyncEnumerable<TEntity>, IDbAsyncEnumerable where TEntity : class
{
	private readonly IInternalSet<TEntity> _internalSet;

	internal override IInternalQuery InternalQuery => _internalSet;

	internal override IInternalSet InternalSet => _internalSet;

	public override IList Local => _internalSet.Local;

	public InternalDbSet(IInternalSet<TEntity> internalSet)
	{
		_internalSet = internalSet;
	}

	public static InternalDbSet<TEntity> Create(InternalContext internalContext, IInternalSet internalSet)
	{
		return new InternalDbSet<TEntity>(((IInternalSet<TEntity>)internalSet) ?? new InternalSet<TEntity>(internalContext));
	}

	public override DbQuery Include(string path)
	{
		Check.NotEmpty(path, "path");
		return new InternalDbQuery<TEntity>(_internalSet.Include(path));
	}

	public override DbQuery AsNoTracking()
	{
		return new InternalDbQuery<TEntity>(_internalSet.AsNoTracking());
	}

	[Obsolete("Queries are now streaming by default unless a retrying ExecutionStrategy is used. Calling this method will have no effect.")]
	public override DbQuery AsStreaming()
	{
		return new InternalDbQuery<TEntity>(_internalSet.AsStreaming());
	}

	internal override DbQuery WithExecutionStrategy(IDbExecutionStrategy executionStrategy)
	{
		return new InternalDbQuery<TEntity>(_internalSet.WithExecutionStrategy(executionStrategy));
	}

	public override object Find(params object[] keyValues)
	{
		return _internalSet.Find(keyValues);
	}

	internal override IInternalQuery GetInternalQueryWithCheck(string memberName)
	{
		return _internalSet;
	}

	internal override IInternalSet GetInternalSetWithCheck(string memberName)
	{
		return _internalSet;
	}

	public override async Task<object> FindAsync(CancellationToken cancellationToken, params object[] keyValues)
	{
		return await _internalSet.FindAsync(cancellationToken, keyValues).WithCurrentCulture();
	}

	public override object Create()
	{
		return _internalSet.Create();
	}

	public override object Create(Type derivedEntityType)
	{
		Check.NotNull(derivedEntityType, "derivedEntityType");
		return _internalSet.Create(derivedEntityType);
	}

	public IEnumerator<TEntity> GetEnumerator()
	{
		return _internalSet.GetEnumerator();
	}

	public IDbAsyncEnumerator<TEntity> GetAsyncEnumerator()
	{
		return _internalSet.GetAsyncEnumerator();
	}
}
