namespace System.Data.Entity.Infrastructure;

public interface IDbContextFactory<out TContext> where TContext : DbContext
{
	TContext Create();
}
