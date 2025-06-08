using System.ComponentModel;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Infrastructure.Interception;

public class DbConnectionPropertyInterceptionContext<TValue> : PropertyInterceptionContext<TValue>
{
	public DbConnectionPropertyInterceptionContext()
	{
	}

	public DbConnectionPropertyInterceptionContext(DbInterceptionContext copyFrom)
		: base(copyFrom)
	{
		Check.NotNull(copyFrom, "copyFrom");
	}

	public new DbConnectionPropertyInterceptionContext<TValue> WithValue(TValue value)
	{
		return (DbConnectionPropertyInterceptionContext<TValue>)base.WithValue(value);
	}

	protected override DbInterceptionContext Clone()
	{
		return new DbConnectionPropertyInterceptionContext<TValue>(this);
	}

	public new DbConnectionPropertyInterceptionContext<TValue> AsAsync()
	{
		return (DbConnectionPropertyInterceptionContext<TValue>)base.AsAsync();
	}

	public new DbConnectionPropertyInterceptionContext<TValue> WithDbContext(DbContext context)
	{
		Check.NotNull(context, "context");
		return (DbConnectionPropertyInterceptionContext<TValue>)base.WithDbContext(context);
	}

	public new DbConnectionPropertyInterceptionContext<TValue> WithObjectContext(ObjectContext context)
	{
		Check.NotNull(context, "context");
		return (DbConnectionPropertyInterceptionContext<TValue>)base.WithObjectContext(context);
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
