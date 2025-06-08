using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Internal;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Data.Entity.Validation;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Infrastructure;

public class DbEntityEntry
{
	private readonly InternalEntityEntry _internalEntityEntry;

	public object Entity => _internalEntityEntry.Entity;

	public EntityState State
	{
		get
		{
			return _internalEntityEntry.State;
		}
		set
		{
			_internalEntityEntry.State = value;
		}
	}

	public DbPropertyValues CurrentValues => new DbPropertyValues(_internalEntityEntry.CurrentValues);

	public DbPropertyValues OriginalValues => new DbPropertyValues(_internalEntityEntry.OriginalValues);

	internal InternalEntityEntry InternalEntry => _internalEntityEntry;

	internal DbEntityEntry(InternalEntityEntry internalEntityEntry)
	{
		_internalEntityEntry = internalEntityEntry;
	}

	public DbPropertyValues GetDatabaseValues()
	{
		InternalPropertyValues databaseValues = _internalEntityEntry.GetDatabaseValues();
		if (databaseValues != null)
		{
			return new DbPropertyValues(databaseValues);
		}
		return null;
	}

	public Task<DbPropertyValues> GetDatabaseValuesAsync()
	{
		return GetDatabaseValuesAsync(CancellationToken.None);
	}

	public async Task<DbPropertyValues> GetDatabaseValuesAsync(CancellationToken cancellationToken)
	{
		InternalPropertyValues internalPropertyValues = await _internalEntityEntry.GetDatabaseValuesAsync(cancellationToken).WithCurrentCulture();
		return (internalPropertyValues == null) ? null : new DbPropertyValues(internalPropertyValues);
	}

	public void Reload()
	{
		_internalEntityEntry.Reload();
	}

	public Task ReloadAsync()
	{
		return _internalEntityEntry.ReloadAsync(CancellationToken.None);
	}

	public Task ReloadAsync(CancellationToken cancellationToken)
	{
		return _internalEntityEntry.ReloadAsync(cancellationToken);
	}

	public DbReferenceEntry Reference(string navigationProperty)
	{
		Check.NotEmpty(navigationProperty, "navigationProperty");
		return DbReferenceEntry.Create(_internalEntityEntry.Reference(navigationProperty));
	}

	public DbCollectionEntry Collection(string navigationProperty)
	{
		Check.NotEmpty(navigationProperty, "navigationProperty");
		return DbCollectionEntry.Create(_internalEntityEntry.Collection(navigationProperty));
	}

	public DbPropertyEntry Property(string propertyName)
	{
		Check.NotEmpty(propertyName, "propertyName");
		return DbPropertyEntry.Create(_internalEntityEntry.Property(propertyName));
	}

	public DbComplexPropertyEntry ComplexProperty(string propertyName)
	{
		Check.NotEmpty(propertyName, "propertyName");
		return DbComplexPropertyEntry.Create(_internalEntityEntry.Property(propertyName, null, requireComplex: true));
	}

	public DbMemberEntry Member(string propertyName)
	{
		Check.NotEmpty(propertyName, "propertyName");
		return DbMemberEntry.Create(_internalEntityEntry.Member(propertyName));
	}

	public DbEntityEntry<TEntity> Cast<TEntity>() where TEntity : class
	{
		if (!typeof(TEntity).IsAssignableFrom(_internalEntityEntry.EntityType))
		{
			throw Error.DbEntity_BadTypeForCast(typeof(DbEntityEntry).Name, typeof(TEntity).Name, _internalEntityEntry.EntityType.Name);
		}
		return new DbEntityEntry<TEntity>(_internalEntityEntry);
	}

	public DbEntityValidationResult GetValidationResult()
	{
		return _internalEntityEntry.InternalContext.Owner.CallValidateEntity(this);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override bool Equals(object obj)
	{
		if (obj == null || obj.GetType() != typeof(DbEntityEntry))
		{
			return false;
		}
		return Equals((DbEntityEntry)obj);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool Equals(DbEntityEntry other)
	{
		if (this == other)
		{
			return true;
		}
		if (other != null)
		{
			return _internalEntityEntry.Equals(other._internalEntityEntry);
		}
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override int GetHashCode()
	{
		return _internalEntityEntry.GetHashCode();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override string ToString()
	{
		return base.ToString();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public new Type GetType()
	{
		return base.GetType();
	}
}
public class DbEntityEntry<TEntity> where TEntity : class
{
	private readonly InternalEntityEntry _internalEntityEntry;

	public TEntity Entity => (TEntity)_internalEntityEntry.Entity;

	public EntityState State
	{
		get
		{
			return _internalEntityEntry.State;
		}
		set
		{
			_internalEntityEntry.State = value;
		}
	}

	public DbPropertyValues CurrentValues => new DbPropertyValues(_internalEntityEntry.CurrentValues);

	public DbPropertyValues OriginalValues => new DbPropertyValues(_internalEntityEntry.OriginalValues);

	internal DbEntityEntry(InternalEntityEntry internalEntityEntry)
	{
		_internalEntityEntry = internalEntityEntry;
	}

	public DbPropertyValues GetDatabaseValues()
	{
		InternalPropertyValues databaseValues = _internalEntityEntry.GetDatabaseValues();
		if (databaseValues != null)
		{
			return new DbPropertyValues(databaseValues);
		}
		return null;
	}

	public Task<DbPropertyValues> GetDatabaseValuesAsync()
	{
		return GetDatabaseValuesAsync(CancellationToken.None);
	}

	public async Task<DbPropertyValues> GetDatabaseValuesAsync(CancellationToken cancellationToken)
	{
		InternalPropertyValues internalPropertyValues = await _internalEntityEntry.GetDatabaseValuesAsync(cancellationToken).WithCurrentCulture();
		return (internalPropertyValues == null) ? null : new DbPropertyValues(internalPropertyValues);
	}

	public void Reload()
	{
		_internalEntityEntry.Reload();
	}

	public Task ReloadAsync()
	{
		return _internalEntityEntry.ReloadAsync(CancellationToken.None);
	}

	public Task ReloadAsync(CancellationToken cancellationToken)
	{
		return _internalEntityEntry.ReloadAsync(cancellationToken);
	}

	public DbReferenceEntry Reference(string navigationProperty)
	{
		Check.NotEmpty(navigationProperty, "navigationProperty");
		return DbReferenceEntry.Create(_internalEntityEntry.Reference(navigationProperty));
	}

	public DbReferenceEntry<TEntity, TProperty> Reference<TProperty>(string navigationProperty) where TProperty : class
	{
		Check.NotEmpty(navigationProperty, "navigationProperty");
		return DbReferenceEntry<TEntity, TProperty>.Create(_internalEntityEntry.Reference(navigationProperty, typeof(TProperty)));
	}

	public DbReferenceEntry<TEntity, TProperty> Reference<TProperty>(Expression<Func<TEntity, TProperty>> navigationProperty) where TProperty : class
	{
		Check.NotNull(navigationProperty, "navigationProperty");
		return DbReferenceEntry<TEntity, TProperty>.Create(_internalEntityEntry.Reference(DbHelpers.ParsePropertySelector(navigationProperty, "Reference", "navigationProperty"), typeof(TProperty)));
	}

	public DbCollectionEntry Collection(string navigationProperty)
	{
		Check.NotEmpty(navigationProperty, "navigationProperty");
		return DbCollectionEntry.Create(_internalEntityEntry.Collection(navigationProperty));
	}

	public DbCollectionEntry<TEntity, TElement> Collection<TElement>(string navigationProperty) where TElement : class
	{
		Check.NotEmpty(navigationProperty, "navigationProperty");
		return DbCollectionEntry<TEntity, TElement>.Create(_internalEntityEntry.Collection(navigationProperty, typeof(TElement)));
	}

	public DbCollectionEntry<TEntity, TElement> Collection<TElement>(Expression<Func<TEntity, ICollection<TElement>>> navigationProperty) where TElement : class
	{
		Check.NotNull(navigationProperty, "navigationProperty");
		return Collection<TElement>(DbHelpers.ParsePropertySelector(navigationProperty, "Collection", "navigationProperty"));
	}

	public DbPropertyEntry Property(string propertyName)
	{
		Check.NotEmpty(propertyName, "propertyName");
		return DbPropertyEntry.Create(_internalEntityEntry.Property(propertyName));
	}

	public DbPropertyEntry<TEntity, TProperty> Property<TProperty>(string propertyName)
	{
		Check.NotEmpty(propertyName, "propertyName");
		return DbPropertyEntry<TEntity, TProperty>.Create(_internalEntityEntry.Property(propertyName, typeof(TProperty)));
	}

	public DbPropertyEntry<TEntity, TProperty> Property<TProperty>(Expression<Func<TEntity, TProperty>> property)
	{
		Check.NotNull(property, "property");
		return Property<TProperty>(DbHelpers.ParsePropertySelector(property, "Property", "property"));
	}

	public DbComplexPropertyEntry ComplexProperty(string propertyName)
	{
		Check.NotEmpty(propertyName, "propertyName");
		return DbComplexPropertyEntry.Create(_internalEntityEntry.Property(propertyName, null, requireComplex: true));
	}

	public DbComplexPropertyEntry<TEntity, TComplexProperty> ComplexProperty<TComplexProperty>(string propertyName)
	{
		Check.NotEmpty(propertyName, "propertyName");
		return DbComplexPropertyEntry<TEntity, TComplexProperty>.Create(_internalEntityEntry.Property(propertyName, typeof(TComplexProperty), requireComplex: true));
	}

	public DbComplexPropertyEntry<TEntity, TComplexProperty> ComplexProperty<TComplexProperty>(Expression<Func<TEntity, TComplexProperty>> property)
	{
		Check.NotNull(property, "property");
		return ComplexProperty<TComplexProperty>(DbHelpers.ParsePropertySelector(property, "Property", "property"));
	}

	public DbMemberEntry Member(string propertyName)
	{
		Check.NotEmpty(propertyName, "propertyName");
		return DbMemberEntry.Create(_internalEntityEntry.Member(propertyName));
	}

	public DbMemberEntry<TEntity, TMember> Member<TMember>(string propertyName)
	{
		Check.NotEmpty(propertyName, "propertyName");
		return _internalEntityEntry.Member(propertyName, typeof(TMember)).CreateDbMemberEntry<TEntity, TMember>();
	}

	public static implicit operator DbEntityEntry(DbEntityEntry<TEntity> entry)
	{
		return new DbEntityEntry(entry._internalEntityEntry);
	}

	public DbEntityValidationResult GetValidationResult()
	{
		return _internalEntityEntry.InternalContext.Owner.CallValidateEntity(this);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override bool Equals(object obj)
	{
		if (obj == null || obj.GetType() != typeof(DbEntityEntry<TEntity>))
		{
			return false;
		}
		return Equals((DbEntityEntry<TEntity>)obj);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool Equals(DbEntityEntry<TEntity> other)
	{
		if (this == other)
		{
			return true;
		}
		if (other != null)
		{
			return _internalEntityEntry.Equals(other._internalEntityEntry);
		}
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override int GetHashCode()
	{
		return _internalEntityEntry.GetHashCode();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override string ToString()
	{
		return base.ToString();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public new Type GetType()
	{
		return base.GetType();
	}
}
