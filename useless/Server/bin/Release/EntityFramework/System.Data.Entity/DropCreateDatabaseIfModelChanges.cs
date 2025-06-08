using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Internal;
using System.Data.Entity.Utilities;

namespace System.Data.Entity;

public class DropCreateDatabaseIfModelChanges<TContext> : IDatabaseInitializer<TContext> where TContext : DbContext
{
	static DropCreateDatabaseIfModelChanges()
	{
		DbConfigurationManager.Instance.EnsureLoadedForContext(typeof(TContext));
	}

	public virtual void InitializeDatabase(TContext context)
	{
		Check.NotNull(context, "context");
		DatabaseExistenceState databaseExistenceState = new DatabaseTableChecker().AnyModelTableExists(context.InternalContext);
		if (databaseExistenceState == DatabaseExistenceState.Exists)
		{
			if (context.Database.CompatibleWithModel(throwIfNoMetadata: true))
			{
				return;
			}
			context.Database.Delete();
			databaseExistenceState = DatabaseExistenceState.DoesNotExist;
		}
		context.Database.Create(databaseExistenceState);
		Seed(context);
		context.SaveChanges();
	}

	protected virtual void Seed(TContext context)
	{
	}
}
