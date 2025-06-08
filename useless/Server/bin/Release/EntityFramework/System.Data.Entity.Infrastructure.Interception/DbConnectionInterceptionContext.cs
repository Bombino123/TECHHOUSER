using System.ComponentModel;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Infrastructure.Interception;

public class DbConnectionInterceptionContext : MutableInterceptionContext
{
	public DbConnectionInterceptionContext()
	{
	}

	public DbConnectionInterceptionContext(DbInterceptionContext copyFrom)
		: base(copyFrom)
	{
		Check.NotNull(copyFrom, "copyFrom");
	}

	public new DbConnectionInterceptionContext AsAsync()
	{
		return (DbConnectionInterceptionContext)base.AsAsync();
	}

	public new DbConnectionInterceptionContext WithDbContext(DbContext context)
	{
		Check.NotNull(context, "context");
		return (DbConnectionInterceptionContext)base.WithDbContext(context);
	}

	public new DbConnectionInterceptionContext WithObjectContext(ObjectContext context)
	{
		Check.NotNull(context, "context");
		return (DbConnectionInterceptionContext)base.WithObjectContext(context);
	}

	protected override DbInterceptionContext Clone()
	{
		return new DbConnectionInterceptionContext(this);
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
public class DbConnectionInterceptionContext<TResult> : MutableInterceptionContext<TResult>
{
	public DbConnectionInterceptionContext()
	{
	}

	public DbConnectionInterceptionContext(DbInterceptionContext copyFrom)
		: base(copyFrom)
	{
		Check.NotNull(copyFrom, "copyFrom");
	}

	public new DbConnectionInterceptionContext<TResult> AsAsync()
	{
		return (DbConnectionInterceptionContext<TResult>)base.AsAsync();
	}

	public new DbConnectionInterceptionContext<TResult> WithDbContext(DbContext context)
	{
		Check.NotNull(context, "context");
		return (DbConnectionInterceptionContext<TResult>)base.WithDbContext(context);
	}

	public new DbConnectionInterceptionContext<TResult> WithObjectContext(ObjectContext context)
	{
		Check.NotNull(context, "context");
		return (DbConnectionInterceptionContext<TResult>)base.WithObjectContext(context);
	}

	protected override DbInterceptionContext Clone()
	{
		return new DbConnectionInterceptionContext<TResult>(this);
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
