using System.ComponentModel;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Infrastructure.Interception;

public class DbConfigurationInterceptionContext : DbInterceptionContext
{
	public DbConfigurationInterceptionContext()
	{
	}

	public DbConfigurationInterceptionContext(DbInterceptionContext copyFrom)
		: base(copyFrom)
	{
		Check.NotNull(copyFrom, "copyFrom");
	}

	protected override DbInterceptionContext Clone()
	{
		return new DbConfigurationInterceptionContext(this);
	}

	public new DbConfigurationInterceptionContext WithDbContext(DbContext context)
	{
		Check.NotNull(context, "context");
		return (DbConfigurationInterceptionContext)base.WithDbContext(context);
	}

	public new DbConfigurationInterceptionContext WithObjectContext(ObjectContext context)
	{
		Check.NotNull(context, "context");
		return (DbConfigurationInterceptionContext)base.WithObjectContext(context);
	}

	public new DbConfigurationInterceptionContext AsAsync()
	{
		return (DbConfigurationInterceptionContext)base.AsAsync();
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
