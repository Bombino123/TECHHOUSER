using System.Data.Entity.Utilities;

namespace System.Data.Entity;

public class NullDatabaseInitializer<TContext> : IDatabaseInitializer<TContext> where TContext : DbContext
{
	public virtual void InitializeDatabase(TContext context)
	{
		Check.NotNull(context, "context");
	}
}
