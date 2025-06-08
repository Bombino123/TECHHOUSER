using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Internal;
using System.Data.Entity.Utilities;

namespace System.Data.Entity;

public class DropCreateDatabaseAlways<TContext> : IDatabaseInitializer<TContext> where TContext : DbContext
{
	static DropCreateDatabaseAlways()
	{
		DbConfigurationManager.Instance.EnsureLoadedForContext(typeof(TContext));
	}

	public virtual void InitializeDatabase(TContext context)
	{
		Check.NotNull(context, "context");
		context.Database.Delete();
		context.Database.Create(DatabaseExistenceState.DoesNotExist);
		Seed(context);
		context.SaveChanges();
	}

	protected virtual void Seed(TContext context)
	{
	}
}
