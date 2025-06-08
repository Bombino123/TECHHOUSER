using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Internal;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;

namespace System.Data.Entity;

public class CreateDatabaseIfNotExists<TContext> : IDatabaseInitializer<TContext> where TContext : DbContext
{
	static CreateDatabaseIfNotExists()
	{
		DbConfigurationManager.Instance.EnsureLoadedForContext(typeof(TContext));
	}

	public virtual void InitializeDatabase(TContext context)
	{
		Check.NotNull(context, "context");
		DatabaseExistenceState databaseExistenceState = new DatabaseTableChecker().AnyModelTableExists(context.InternalContext);
		if (databaseExistenceState == DatabaseExistenceState.Exists)
		{
			if (!context.Database.CompatibleWithModel(throwIfNoMetadata: false, databaseExistenceState))
			{
				throw Error.DatabaseInitializationStrategy_ModelMismatch(context.GetType().Name);
			}
		}
		else
		{
			context.Database.Create(databaseExistenceState);
			Seed(context);
			context.SaveChanges();
		}
	}

	protected virtual void Seed(TContext context)
	{
	}
}
