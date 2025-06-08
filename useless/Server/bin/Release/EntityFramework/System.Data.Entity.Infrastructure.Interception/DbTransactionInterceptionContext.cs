using System.ComponentModel;
using System.Data.Common;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Infrastructure.Interception;

public class DbTransactionInterceptionContext : MutableInterceptionContext
{
	private DbConnection _connection;

	public DbConnection Connection => _connection;

	public DbTransactionInterceptionContext()
	{
	}

	public DbTransactionInterceptionContext(DbInterceptionContext copyFrom)
		: base(copyFrom)
	{
		if (copyFrom is DbTransactionInterceptionContext dbTransactionInterceptionContext)
		{
			_connection = dbTransactionInterceptionContext.Connection;
		}
		Check.NotNull(copyFrom, "copyFrom");
	}

	public DbTransactionInterceptionContext WithConnection(DbConnection connection)
	{
		Check.NotNull(connection, "connection");
		DbTransactionInterceptionContext dbTransactionInterceptionContext = TypedClone();
		dbTransactionInterceptionContext._connection = connection;
		return dbTransactionInterceptionContext;
	}

	public new DbTransactionInterceptionContext AsAsync()
	{
		return (DbTransactionInterceptionContext)base.AsAsync();
	}

	public new DbTransactionInterceptionContext WithDbContext(DbContext context)
	{
		Check.NotNull(context, "context");
		return (DbTransactionInterceptionContext)base.WithDbContext(context);
	}

	public new DbTransactionInterceptionContext WithObjectContext(ObjectContext context)
	{
		Check.NotNull(context, "context");
		return (DbTransactionInterceptionContext)base.WithObjectContext(context);
	}

	private DbTransactionInterceptionContext TypedClone()
	{
		return (DbTransactionInterceptionContext)Clone();
	}

	protected override DbInterceptionContext Clone()
	{
		return new DbTransactionInterceptionContext(this);
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
public class DbTransactionInterceptionContext<TResult> : MutableInterceptionContext<TResult>
{
	public DbTransactionInterceptionContext()
	{
	}

	public DbTransactionInterceptionContext(DbInterceptionContext copyFrom)
		: base(copyFrom)
	{
		Check.NotNull(copyFrom, "copyFrom");
	}

	public new DbTransactionInterceptionContext<TResult> AsAsync()
	{
		return (DbTransactionInterceptionContext<TResult>)base.AsAsync();
	}

	public new DbTransactionInterceptionContext<TResult> WithDbContext(DbContext context)
	{
		Check.NotNull(context, "context");
		return (DbTransactionInterceptionContext<TResult>)base.WithDbContext(context);
	}

	public new DbTransactionInterceptionContext<TResult> WithObjectContext(ObjectContext context)
	{
		Check.NotNull(context, "context");
		return (DbTransactionInterceptionContext<TResult>)base.WithObjectContext(context);
	}

	protected override DbInterceptionContext Clone()
	{
		return new DbTransactionInterceptionContext<TResult>(this);
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
