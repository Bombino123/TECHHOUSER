namespace System.Data.Entity;

public interface IDatabaseInitializer<in TContext> where TContext : DbContext
{
	void InitializeDatabase(TContext context);
}
