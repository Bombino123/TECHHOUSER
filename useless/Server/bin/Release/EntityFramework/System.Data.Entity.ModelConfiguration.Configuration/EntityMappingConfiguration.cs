using System.ComponentModel;
using System.Data.Entity.ModelConfiguration.Configuration.Mapping;
using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
using System.Data.Entity.Spatial;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Linq.Expressions;

namespace System.Data.Entity.ModelConfiguration.Configuration;

public class EntityMappingConfiguration<TEntityType> where TEntityType : class
{
	private readonly EntityMappingConfiguration _entityMappingConfiguration;

	internal EntityMappingConfiguration EntityMappingConfigurationInstance => _entityMappingConfiguration;

	public EntityMappingConfiguration()
		: this(new EntityMappingConfiguration())
	{
	}

	internal EntityMappingConfiguration(EntityMappingConfiguration entityMappingConfiguration)
	{
		_entityMappingConfiguration = entityMappingConfiguration;
	}

	public void Properties<TObject>(Expression<Func<TEntityType, TObject>> propertiesExpression)
	{
		Check.NotNull(propertiesExpression, "propertiesExpression");
		_entityMappingConfiguration.Properties = propertiesExpression.GetComplexPropertyAccessList().ToList();
	}

	public PropertyMappingConfiguration Property<T>(Expression<Func<TEntityType, T>> propertyExpression) where T : struct
	{
		return new PropertyMappingConfiguration(Property<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration>(propertyExpression));
	}

	public PropertyMappingConfiguration Property<T>(Expression<Func<TEntityType, T?>> propertyExpression) where T : struct
	{
		return new PropertyMappingConfiguration(Property<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration>(propertyExpression));
	}

	public PropertyMappingConfiguration Property(Expression<Func<TEntityType, DbGeometry>> propertyExpression)
	{
		return new PropertyMappingConfiguration(Property<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration>(propertyExpression));
	}

	public PropertyMappingConfiguration Property(Expression<Func<TEntityType, DbGeography>> propertyExpression)
	{
		return new PropertyMappingConfiguration(Property<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration>(propertyExpression));
	}

	public PropertyMappingConfiguration Property(Expression<Func<TEntityType, string>> propertyExpression)
	{
		return new PropertyMappingConfiguration(Property<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.StringPropertyConfiguration>(propertyExpression));
	}

	public PropertyMappingConfiguration Property(Expression<Func<TEntityType, byte[]>> propertyExpression)
	{
		return new PropertyMappingConfiguration(Property<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.BinaryPropertyConfiguration>(propertyExpression));
	}

	public PropertyMappingConfiguration Property(Expression<Func<TEntityType, decimal>> propertyExpression)
	{
		return new PropertyMappingConfiguration(Property<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.DecimalPropertyConfiguration>(propertyExpression));
	}

	public PropertyMappingConfiguration Property(Expression<Func<TEntityType, decimal?>> propertyExpression)
	{
		return new PropertyMappingConfiguration(Property<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.DecimalPropertyConfiguration>(propertyExpression));
	}

	public PropertyMappingConfiguration Property(Expression<Func<TEntityType, DateTime>> propertyExpression)
	{
		return new PropertyMappingConfiguration(Property<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.DateTimePropertyConfiguration>(propertyExpression));
	}

	public PropertyMappingConfiguration Property(Expression<Func<TEntityType, DateTime?>> propertyExpression)
	{
		return new PropertyMappingConfiguration(Property<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.DateTimePropertyConfiguration>(propertyExpression));
	}

	public PropertyMappingConfiguration Property(Expression<Func<TEntityType, DateTimeOffset>> propertyExpression)
	{
		return new PropertyMappingConfiguration(Property<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.DateTimePropertyConfiguration>(propertyExpression));
	}

	public PropertyMappingConfiguration Property(Expression<Func<TEntityType, DateTimeOffset?>> propertyExpression)
	{
		return new PropertyMappingConfiguration(Property<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.DateTimePropertyConfiguration>(propertyExpression));
	}

	public PropertyMappingConfiguration Property(Expression<Func<TEntityType, TimeSpan>> propertyExpression)
	{
		return new PropertyMappingConfiguration(Property<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.DateTimePropertyConfiguration>(propertyExpression));
	}

	public PropertyMappingConfiguration Property(Expression<Func<TEntityType, TimeSpan?>> propertyExpression)
	{
		return new PropertyMappingConfiguration(Property<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.DateTimePropertyConfiguration>(propertyExpression));
	}

	internal TPrimitivePropertyConfiguration Property<TPrimitivePropertyConfiguration>(LambdaExpression lambdaExpression) where TPrimitivePropertyConfiguration : System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration, new()
	{
		return _entityMappingConfiguration.Property(lambdaExpression.GetComplexPropertyAccess(), () => new TPrimitivePropertyConfiguration
		{
			OverridableConfigurationParts = OverridableConfigurationParts.None
		});
	}

	public EntityMappingConfiguration<TEntityType> MapInheritedProperties()
	{
		_entityMappingConfiguration.MapInheritedProperties = true;
		return this;
	}

	public EntityMappingConfiguration<TEntityType> ToTable(string tableName)
	{
		Check.NotEmpty(tableName, "tableName");
		DatabaseName databaseName = DatabaseName.Parse(tableName);
		ToTable(databaseName.Name, databaseName.Schema);
		return this;
	}

	public EntityMappingConfiguration<TEntityType> ToTable(string tableName, string schemaName)
	{
		Check.NotEmpty(tableName, "tableName");
		_entityMappingConfiguration.TableName = new DatabaseName(tableName, schemaName);
		return this;
	}

	public EntityMappingConfiguration<TEntityType> HasTableAnnotation(string name, object value)
	{
		Check.NotEmpty(name, "name");
		_entityMappingConfiguration.SetAnnotation(name, value);
		return this;
	}

	public ValueConditionConfiguration Requires(string discriminator)
	{
		Check.NotEmpty(discriminator, "discriminator");
		return new ValueConditionConfiguration(_entityMappingConfiguration, discriminator);
	}

	public NotNullConditionConfiguration Requires<TProperty>(Expression<Func<TEntityType, TProperty>> property)
	{
		Check.NotNull(property, "property");
		return new NotNullConditionConfiguration(_entityMappingConfiguration, property.GetComplexPropertyAccess());
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
