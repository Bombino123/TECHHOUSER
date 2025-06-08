using System.ComponentModel;
using System.Data.Common;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Infrastructure.Interception;

public class BeginTransactionInterceptionContext : DbConnectionInterceptionContext<DbTransaction>
{
	private IsolationLevel _isolationLevel = IsolationLevel.Unspecified;

	public IsolationLevel IsolationLevel => _isolationLevel;

	public BeginTransactionInterceptionContext()
	{
	}

	public BeginTransactionInterceptionContext(DbInterceptionContext copyFrom)
		: base(copyFrom)
	{
		Check.NotNull(copyFrom, "copyFrom");
		if (copyFrom is BeginTransactionInterceptionContext beginTransactionInterceptionContext)
		{
			_isolationLevel = beginTransactionInterceptionContext._isolationLevel;
		}
	}

	public new BeginTransactionInterceptionContext AsAsync()
	{
		return (BeginTransactionInterceptionContext)base.AsAsync();
	}

	public BeginTransactionInterceptionContext WithIsolationLevel(IsolationLevel isolationLevel)
	{
		BeginTransactionInterceptionContext beginTransactionInterceptionContext = TypedClone();
		beginTransactionInterceptionContext._isolationLevel = isolationLevel;
		return beginTransactionInterceptionContext;
	}

	private BeginTransactionInterceptionContext TypedClone()
	{
		return (BeginTransactionInterceptionContext)Clone();
	}

	protected override DbInterceptionContext Clone()
	{
		return new BeginTransactionInterceptionContext(this);
	}

	public new BeginTransactionInterceptionContext WithDbContext(DbContext context)
	{
		Check.NotNull(context, "context");
		return (BeginTransactionInterceptionContext)base.WithDbContext(context);
	}

	public new BeginTransactionInterceptionContext WithObjectContext(ObjectContext context)
	{
		Check.NotNull(context, "context");
		return (BeginTransactionInterceptionContext)base.WithObjectContext(context);
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
