using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
using System.Data.Entity.ModelConfiguration.Configuration.Types;
using System.Data.Entity.ModelConfiguration.Utilities;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration;

public class EntityTypeConfiguration<TEntityType> : StructuralTypeConfiguration<TEntityType> where TEntityType : class
{
	private readonly EntityTypeConfiguration _entityTypeConfiguration;

	internal override StructuralTypeConfiguration Configuration => _entityTypeConfiguration;

	public EntityTypeConfiguration()
		: this(new EntityTypeConfiguration(typeof(TEntityType)))
	{
	}

	internal EntityTypeConfiguration(EntityTypeConfiguration entityTypeConfiguration)
	{
		_entityTypeConfiguration = entityTypeConfiguration;
	}

	internal override TPrimitivePropertyConfiguration Property<TPrimitivePropertyConfiguration>(LambdaExpression lambdaExpression)
	{
		return Configuration.Property(lambdaExpression.GetComplexPropertyAccess(), () => new TPrimitivePropertyConfiguration
		{
			OverridableConfigurationParts = OverridableConfigurationParts.None
		});
	}

	public EntityTypeConfiguration<TEntityType> HasKey<TKey>(Expression<Func<TEntityType, TKey>> keyExpression)
	{
		Check.NotNull(keyExpression, "keyExpression");
		_entityTypeConfiguration.Key(from p in keyExpression.GetSimplePropertyAccessList()
			select p.Single());
		return this;
	}

	public EntityTypeConfiguration<TEntityType> HasKey<TKey>(Expression<Func<TEntityType, TKey>> keyExpression, Action<PrimaryKeyIndexConfiguration> buildAction)
	{
		Check.NotNull(keyExpression, "keyExpression");
		Check.NotNull(buildAction, "buildAction");
		_entityTypeConfiguration.Key(from p in keyExpression.GetSimplePropertyAccessList()
			select p.Single());
		buildAction(new PrimaryKeyIndexConfiguration(_entityTypeConfiguration.ConfigureKey()));
		return this;
	}

	public IndexConfiguration HasIndex<TIndex>(Expression<Func<TEntityType, TIndex>> indexExpression)
	{
		Check.NotNull(indexExpression, "indexExpression");
		IEnumerable<PropertyInfo> components = from p in indexExpression.GetSimplePropertyAccessList()
			select p.Single();
		return new IndexConfiguration(_entityTypeConfiguration.Index(new PropertyPath(components)));
	}

	public EntityTypeConfiguration<TEntityType> HasEntitySetName(string entitySetName)
	{
		Check.NotEmpty(entitySetName, "entitySetName");
		_entityTypeConfiguration.EntitySetName = entitySetName;
		return this;
	}

	public EntityTypeConfiguration<TEntityType> Ignore<TProperty>(Expression<Func<TEntityType, TProperty>> propertyExpression)
	{
		Check.NotNull(propertyExpression, "propertyExpression");
		Configuration.Ignore(propertyExpression.GetSimplePropertyAccess().Single());
		return this;
	}

	public EntityTypeConfiguration<TEntityType> ToTable(string tableName)
	{
		Check.NotEmpty(tableName, "tableName");
		DatabaseName databaseName = DatabaseName.Parse(tableName);
		_entityTypeConfiguration.ToTable(databaseName.Name, databaseName.Schema);
		return this;
	}

	public EntityTypeConfiguration<TEntityType> ToTable(string tableName, string schemaName)
	{
		Check.NotEmpty(tableName, "tableName");
		_entityTypeConfiguration.ToTable(tableName, schemaName);
		return this;
	}

	public EntityTypeConfiguration<TEntityType> HasTableAnnotation(string name, object value)
	{
		Check.NotEmpty(name, "name");
		_entityTypeConfiguration.SetAnnotation(name, value);
		return this;
	}

	public EntityTypeConfiguration<TEntityType> MapToStoredProcedures()
	{
		_entityTypeConfiguration.MapToStoredProcedures();
		return this;
	}

	public EntityTypeConfiguration<TEntityType> MapToStoredProcedures(Action<ModificationStoredProceduresConfiguration<TEntityType>> modificationStoredProcedureMappingConfigurationAction)
	{
		Check.NotNull(modificationStoredProcedureMappingConfigurationAction, "modificationStoredProcedureMappingConfigurationAction");
		ModificationStoredProceduresConfiguration<TEntityType> modificationStoredProceduresConfiguration = new ModificationStoredProceduresConfiguration<TEntityType>();
		modificationStoredProcedureMappingConfigurationAction(modificationStoredProceduresConfiguration);
		_entityTypeConfiguration.MapToStoredProcedures(modificationStoredProceduresConfiguration.Configuration, allowOverride: true);
		return this;
	}

	public EntityTypeConfiguration<TEntityType> Map(Action<EntityMappingConfiguration<TEntityType>> entityMappingConfigurationAction)
	{
		Check.NotNull(entityMappingConfigurationAction, "entityMappingConfigurationAction");
		EntityMappingConfiguration<TEntityType> entityMappingConfiguration = new EntityMappingConfiguration<TEntityType>();
		entityMappingConfigurationAction(entityMappingConfiguration);
		_entityTypeConfiguration.AddMappingConfiguration(entityMappingConfiguration.EntityMappingConfigurationInstance);
		return this;
	}

	public EntityTypeConfiguration<TEntityType> Map<TDerived>(Action<EntityMappingConfiguration<TDerived>> derivedTypeMapConfigurationAction) where TDerived : class, TEntityType
	{
		Check.NotNull(derivedTypeMapConfigurationAction, "derivedTypeMapConfigurationAction");
		EntityMappingConfiguration<TDerived> entityMappingConfiguration = new EntityMappingConfiguration<TDerived>();
		DatabaseName tableName = _entityTypeConfiguration.GetTableName();
		if (tableName != null)
		{
			entityMappingConfiguration.EntityMappingConfigurationInstance.TableName = tableName;
		}
		derivedTypeMapConfigurationAction(entityMappingConfiguration);
		if (typeof(TDerived) == typeof(TEntityType))
		{
			_entityTypeConfiguration.AddMappingConfiguration(entityMappingConfiguration.EntityMappingConfigurationInstance);
		}
		else
		{
			_entityTypeConfiguration.AddSubTypeMappingConfiguration(typeof(TDerived), entityMappingConfiguration.EntityMappingConfigurationInstance);
		}
		return this;
	}

	public OptionalNavigationPropertyConfiguration<TEntityType, TTargetEntity> HasOptional<TTargetEntity>(Expression<Func<TEntityType, TTargetEntity>> navigationPropertyExpression) where TTargetEntity : class
	{
		Check.NotNull(navigationPropertyExpression, "navigationPropertyExpression");
		return new OptionalNavigationPropertyConfiguration<TEntityType, TTargetEntity>(_entityTypeConfiguration.Navigation(navigationPropertyExpression.GetSimplePropertyAccess().Single()));
	}

	public RequiredNavigationPropertyConfiguration<TEntityType, TTargetEntity> HasRequired<TTargetEntity>(Expression<Func<TEntityType, TTargetEntity>> navigationPropertyExpression) where TTargetEntity : class
	{
		Check.NotNull(navigationPropertyExpression, "navigationPropertyExpression");
		return new RequiredNavigationPropertyConfiguration<TEntityType, TTargetEntity>(_entityTypeConfiguration.Navigation(navigationPropertyExpression.GetSimplePropertyAccess().Single()));
	}

	public ManyNavigationPropertyConfiguration<TEntityType, TTargetEntity> HasMany<TTargetEntity>(Expression<Func<TEntityType, ICollection<TTargetEntity>>> navigationPropertyExpression) where TTargetEntity : class
	{
		Check.NotNull(navigationPropertyExpression, "navigationPropertyExpression");
		return new ManyNavigationPropertyConfiguration<TEntityType, TTargetEntity>(_entityTypeConfiguration.Navigation(navigationPropertyExpression.GetSimplePropertyAccess().Single()));
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
