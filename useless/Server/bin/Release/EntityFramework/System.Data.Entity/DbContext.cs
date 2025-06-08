using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Internal;
using System.Data.Entity.ModelConfiguration.Utilities;
using System.Data.Entity.Utilities;
using System.Data.Entity.Validation;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity;

public class DbContext : IDisposable, IObjectContextAdapter
{
	private InternalContext _internalContext;

	private Database _database;

	public Database Database
	{
		get
		{
			if (_database == null)
			{
				_database = new Database(InternalContext);
			}
			return _database;
		}
	}

	ObjectContext IObjectContextAdapter.ObjectContext
	{
		get
		{
			InternalContext.ForceOSpaceLoadingForKnownEntityTypes();
			return InternalContext.ObjectContext;
		}
	}

	public DbChangeTracker ChangeTracker => new DbChangeTracker(InternalContext);

	public DbContextConfiguration Configuration => new DbContextConfiguration(InternalContext);

	internal virtual InternalContext InternalContext => _internalContext;

	protected DbContext()
	{
		InitializeLazyInternalContext(new LazyInternalConnection(this, GetType().DatabaseName()));
	}

	protected DbContext(DbCompiledModel model)
	{
		Check.NotNull(model, "model");
		InitializeLazyInternalContext(new LazyInternalConnection(this, GetType().DatabaseName()), model);
	}

	public DbContext(string nameOrConnectionString)
	{
		Check.NotEmpty(nameOrConnectionString, "nameOrConnectionString");
		InitializeLazyInternalContext(new LazyInternalConnection(this, nameOrConnectionString));
	}

	public DbContext(string nameOrConnectionString, DbCompiledModel model)
	{
		Check.NotEmpty(nameOrConnectionString, "nameOrConnectionString");
		Check.NotNull(model, "model");
		InitializeLazyInternalContext(new LazyInternalConnection(this, nameOrConnectionString), model);
	}

	public DbContext(DbConnection existingConnection, bool contextOwnsConnection)
	{
		Check.NotNull(existingConnection, "existingConnection");
		InitializeLazyInternalContext(new EagerInternalConnection(this, existingConnection, contextOwnsConnection));
	}

	public DbContext(DbConnection existingConnection, DbCompiledModel model, bool contextOwnsConnection)
	{
		Check.NotNull(existingConnection, "existingConnection");
		Check.NotNull(model, "model");
		InitializeLazyInternalContext(new EagerInternalConnection(this, existingConnection, contextOwnsConnection), model);
	}

	public DbContext(ObjectContext objectContext, bool dbContextOwnsObjectContext)
	{
		Check.NotNull(objectContext, "objectContext");
		DbConfigurationManager.Instance.EnsureLoadedForContext(GetType());
		_internalContext = new EagerInternalContext(this, objectContext, dbContextOwnsObjectContext);
		DiscoverAndInitializeSets();
	}

	internal virtual void InitializeLazyInternalContext(IInternalConnection internalConnection, DbCompiledModel model = null)
	{
		DbConfigurationManager.Instance.EnsureLoadedForContext(GetType());
		_internalContext = new LazyInternalContext(this, internalConnection, model, DbConfiguration.DependencyResolver.GetService<Func<DbContext, IDbModelCacheKey>>(), DbConfiguration.DependencyResolver.GetService<AttributeProvider>());
		DiscoverAndInitializeSets();
	}

	private void DiscoverAndInitializeSets()
	{
		new DbSetDiscoveryService(this).InitializeSets();
	}

	protected virtual void OnModelCreating(DbModelBuilder modelBuilder)
	{
	}

	internal void CallOnModelCreating(DbModelBuilder modelBuilder)
	{
		OnModelCreating(modelBuilder);
	}

	public virtual DbSet<TEntity> Set<TEntity>() where TEntity : class
	{
		return (DbSet<TEntity>)InternalContext.Set<TEntity>();
	}

	public virtual DbSet Set(Type entityType)
	{
		Check.NotNull(entityType, "entityType");
		return (DbSet)InternalContext.Set(entityType);
	}

	public virtual int SaveChanges()
	{
		return InternalContext.SaveChanges();
	}

	public virtual Task<int> SaveChangesAsync()
	{
		return SaveChangesAsync(CancellationToken.None);
	}

	public virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken)
	{
		return InternalContext.SaveChangesAsync(cancellationToken);
	}

	public IEnumerable<DbEntityValidationResult> GetValidationErrors()
	{
		List<DbEntityValidationResult> list = new List<DbEntityValidationResult>();
		foreach (DbEntityEntry item in ChangeTracker.Entries())
		{
			if (item.InternalEntry.EntityType != typeof(EdmMetadata) && ShouldValidateEntity(item))
			{
				DbEntityValidationResult dbEntityValidationResult = ValidateEntity(item, new Dictionary<object, object>());
				if (dbEntityValidationResult != null && !dbEntityValidationResult.IsValid)
				{
					list.Add(dbEntityValidationResult);
				}
			}
		}
		return list;
	}

	protected virtual bool ShouldValidateEntity(DbEntityEntry entityEntry)
	{
		Check.NotNull(entityEntry, "entityEntry");
		return (entityEntry.State & (EntityState.Added | EntityState.Modified)) != 0;
	}

	protected virtual DbEntityValidationResult ValidateEntity(DbEntityEntry entityEntry, IDictionary<object, object> items)
	{
		Check.NotNull(entityEntry, "entityEntry");
		return entityEntry.InternalEntry.GetValidationResult(items);
	}

	internal virtual DbEntityValidationResult CallValidateEntity(DbEntityEntry entityEntry)
	{
		return ValidateEntity(entityEntry, new Dictionary<object, object>());
	}

	public DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class
	{
		Check.NotNull(entity, "entity");
		return new DbEntityEntry<TEntity>(new InternalEntityEntry(InternalContext, entity));
	}

	public DbEntityEntry Entry(object entity)
	{
		Check.NotNull(entity, "entity");
		return new DbEntityEntry(new InternalEntityEntry(InternalContext, entity));
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		_internalContext.Dispose();
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
