using System.Data.Entity.Internal;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq.Expressions;

namespace System.Data.Entity.Infrastructure;

public class DbComplexPropertyEntry : DbPropertyEntry
{
	internal new static DbComplexPropertyEntry Create(InternalPropertyEntry internalPropertyEntry)
	{
		return (DbComplexPropertyEntry)internalPropertyEntry.CreateDbMemberEntry();
	}

	internal DbComplexPropertyEntry(InternalPropertyEntry internalPropertyEntry)
		: base(internalPropertyEntry)
	{
	}

	public DbPropertyEntry Property(string propertyName)
	{
		Check.NotEmpty(propertyName, "propertyName");
		return DbPropertyEntry.Create(((InternalPropertyEntry)InternalMemberEntry).Property(propertyName));
	}

	public DbComplexPropertyEntry ComplexProperty(string propertyName)
	{
		Check.NotEmpty(propertyName, "propertyName");
		return Create(((InternalPropertyEntry)InternalMemberEntry).Property(propertyName, null, requireComplex: true));
	}

	public new DbComplexPropertyEntry<TEntity, TComplexProperty> Cast<TEntity, TComplexProperty>() where TEntity : class
	{
		MemberEntryMetadata entryMetadata = InternalMemberEntry.EntryMetadata;
		if (!typeof(TEntity).IsAssignableFrom(entryMetadata.DeclaringType) || !typeof(TComplexProperty).IsAssignableFrom(entryMetadata.ElementType))
		{
			throw Error.DbMember_BadTypeForCast(typeof(DbComplexPropertyEntry).Name, typeof(TEntity).Name, typeof(TComplexProperty).Name, entryMetadata.DeclaringType.Name, entryMetadata.MemberType.Name);
		}
		return DbComplexPropertyEntry<TEntity, TComplexProperty>.Create((InternalPropertyEntry)InternalMemberEntry);
	}
}
public class DbComplexPropertyEntry<TEntity, TComplexProperty> : DbPropertyEntry<TEntity, TComplexProperty> where TEntity : class
{
	internal new static DbComplexPropertyEntry<TEntity, TComplexProperty> Create(InternalPropertyEntry internalPropertyEntry)
	{
		return (DbComplexPropertyEntry<TEntity, TComplexProperty>)internalPropertyEntry.CreateDbMemberEntry<TEntity, TComplexProperty>();
	}

	internal DbComplexPropertyEntry(InternalPropertyEntry internalPropertyEntry)
		: base(internalPropertyEntry)
	{
	}

	public static implicit operator DbComplexPropertyEntry(DbComplexPropertyEntry<TEntity, TComplexProperty> entry)
	{
		return DbComplexPropertyEntry.Create(entry.InternalPropertyEntry);
	}

	public DbPropertyEntry Property(string propertyName)
	{
		Check.NotEmpty(propertyName, "propertyName");
		return DbPropertyEntry.Create(base.InternalPropertyEntry.Property(propertyName));
	}

	public DbPropertyEntry<TEntity, TNestedProperty> Property<TNestedProperty>(string propertyName)
	{
		Check.NotEmpty(propertyName, "propertyName");
		return DbPropertyEntry<TEntity, TNestedProperty>.Create(base.InternalPropertyEntry.Property(propertyName, typeof(TNestedProperty)));
	}

	public DbPropertyEntry<TEntity, TNestedProperty> Property<TNestedProperty>(Expression<Func<TComplexProperty, TNestedProperty>> property)
	{
		Check.NotNull(property, "property");
		return Property<TNestedProperty>(DbHelpers.ParsePropertySelector(property, "Property", "property"));
	}

	public DbComplexPropertyEntry ComplexProperty(string propertyName)
	{
		Check.NotEmpty(propertyName, "propertyName");
		return DbComplexPropertyEntry.Create(base.InternalPropertyEntry.Property(propertyName, null, requireComplex: true));
	}

	public DbComplexPropertyEntry<TEntity, TNestedComplexProperty> ComplexProperty<TNestedComplexProperty>(string propertyName)
	{
		Check.NotEmpty(propertyName, "propertyName");
		return DbComplexPropertyEntry<TEntity, TNestedComplexProperty>.Create(base.InternalPropertyEntry.Property(propertyName, typeof(TNestedComplexProperty), requireComplex: true));
	}

	public DbComplexPropertyEntry<TEntity, TNestedComplexProperty> ComplexProperty<TNestedComplexProperty>(Expression<Func<TComplexProperty, TNestedComplexProperty>> property)
	{
		Check.NotNull(property, "property");
		return ComplexProperty<TNestedComplexProperty>(DbHelpers.ParsePropertySelector(property, "Property", "property"));
	}
}
