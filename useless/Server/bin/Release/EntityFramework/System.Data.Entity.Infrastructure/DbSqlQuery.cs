using System.ComponentModel;
using System.Data.Entity.Internal;

namespace System.Data.Entity.Infrastructure;

public class DbSqlQuery : DbRawSqlQuery
{
	internal DbSqlQuery(InternalSqlQuery internalQuery)
		: base(internalQuery)
	{
	}

	protected DbSqlQuery()
		: this(null)
	{
	}

	public virtual DbSqlQuery AsNoTracking()
	{
		if (base.InternalQuery != null)
		{
			return new DbSqlQuery(base.InternalQuery.AsNoTracking());
		}
		return this;
	}

	[Obsolete("Queries are now streaming by default unless a retrying ExecutionStrategy is used. Calling this method will have no effect.")]
	public new virtual DbSqlQuery AsStreaming()
	{
		if (base.InternalQuery != null)
		{
			return new DbSqlQuery(base.InternalQuery.AsStreaming());
		}
		return this;
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
public class DbSqlQuery<TEntity> : DbRawSqlQuery<TEntity> where TEntity : class
{
	internal DbSqlQuery(InternalSqlQuery internalQuery)
		: base(internalQuery)
	{
	}

	protected DbSqlQuery()
		: this((InternalSqlQuery)null)
	{
	}

	public virtual DbSqlQuery<TEntity> AsNoTracking()
	{
		if (base.InternalQuery != null)
		{
			return new DbSqlQuery<TEntity>(base.InternalQuery.AsNoTracking());
		}
		return this;
	}

	[Obsolete("Queries are now streaming by default unless a retrying ExecutionStrategy is used. Calling this method will have no effect.")]
	public new virtual DbSqlQuery<TEntity> AsStreaming()
	{
		if (base.InternalQuery != null)
		{
			return new DbSqlQuery<TEntity>(base.InternalQuery.AsStreaming());
		}
		return this;
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
