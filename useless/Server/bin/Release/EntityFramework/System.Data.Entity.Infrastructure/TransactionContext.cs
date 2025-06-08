using System.Data.Common;

namespace System.Data.Entity.Infrastructure;

public class TransactionContext : DbContext
{
	private const string _defaultTableName = "__TransactionHistory";

	public virtual IDbSet<TransactionRow> Transactions { get; set; }

	public TransactionContext(DbConnection existingConnection)
		: base(existingConnection, contextOwnsConnection: false)
	{
		base.Configuration.ValidateOnSaveEnabled = false;
	}

	protected override void OnModelCreating(DbModelBuilder modelBuilder)
	{
		modelBuilder.Entity<TransactionRow>().ToTable("__TransactionHistory");
	}
}
