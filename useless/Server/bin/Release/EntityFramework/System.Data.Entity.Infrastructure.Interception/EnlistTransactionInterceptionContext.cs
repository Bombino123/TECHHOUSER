using System.ComponentModel;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Utilities;
using System.Transactions;

namespace System.Data.Entity.Infrastructure.Interception;

public class EnlistTransactionInterceptionContext : DbConnectionInterceptionContext
{
	private Transaction _transaction;

	public Transaction Transaction => _transaction;

	public EnlistTransactionInterceptionContext()
	{
	}

	public EnlistTransactionInterceptionContext(DbInterceptionContext copyFrom)
		: base(copyFrom)
	{
		Check.NotNull(copyFrom, "copyFrom");
		if (copyFrom is EnlistTransactionInterceptionContext enlistTransactionInterceptionContext)
		{
			_transaction = enlistTransactionInterceptionContext._transaction;
		}
	}

	public new EnlistTransactionInterceptionContext AsAsync()
	{
		return (EnlistTransactionInterceptionContext)base.AsAsync();
	}

	public EnlistTransactionInterceptionContext WithTransaction(Transaction transaction)
	{
		EnlistTransactionInterceptionContext enlistTransactionInterceptionContext = TypedClone();
		enlistTransactionInterceptionContext._transaction = transaction;
		return enlistTransactionInterceptionContext;
	}

	private EnlistTransactionInterceptionContext TypedClone()
	{
		return (EnlistTransactionInterceptionContext)Clone();
	}

	protected override DbInterceptionContext Clone()
	{
		return new EnlistTransactionInterceptionContext(this);
	}

	public new EnlistTransactionInterceptionContext WithDbContext(DbContext context)
	{
		Check.NotNull(context, "context");
		return (EnlistTransactionInterceptionContext)base.WithDbContext(context);
	}

	public new EnlistTransactionInterceptionContext WithObjectContext(ObjectContext context)
	{
		Check.NotNull(context, "context");
		return (EnlistTransactionInterceptionContext)base.WithObjectContext(context);
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
