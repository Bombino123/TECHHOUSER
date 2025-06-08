using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Core;
using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
using System.Data.Entity.ModelConfiguration.Configuration.Types;
using System.Data.Entity.ModelConfiguration.Utilities;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Configuration;

public class ConventionTypeConfiguration
{
	[Flags]
	private enum ConfigurationAspect : uint
	{
		None = 0u,
		HasEntitySetName = 1u,
		HasKey = 2u,
		IgnoreType = 4u,
		Ignore = 8u,
		IsComplexType = 0x10u,
		MapToStoredProcedures = 0x20u,
		Property = 0x40u,
		NavigationProperty = 0x80u,
		ToTable = 0x100u,
		HasTableAnnotation = 0x200u
	}

	private readonly Type _type;

	private readonly Func<EntityTypeConfiguration> _entityTypeConfiguration;

	private readonly ModelConfiguration _modelConfiguration;

	private readonly Func<ComplexTypeConfiguration> _complexTypeConfiguration;

	private ConfigurationAspect _currentConfigurationAspect;

	private static readonly List<ConfigurationAspect> ConfigurationAspectsConflictingWithIgnoreType = new List<ConfigurationAspect>
	{
		ConfigurationAspect.IsComplexType,
		ConfigurationAspect.HasEntitySetName,
		ConfigurationAspect.Ignore,
		ConfigurationAspect.HasKey,
		ConfigurationAspect.MapToStoredProcedures,
		ConfigurationAspect.NavigationProperty,
		ConfigurationAspect.Property,
		ConfigurationAspect.ToTable,
		ConfigurationAspect.HasTableAnnotation
	};

	private static readonly List<ConfigurationAspect> ConfigurationAspectsConflictingWithComplexType = new List<ConfigurationAspect>
	{
		ConfigurationAspect.HasEntitySetName,
		ConfigurationAspect.HasKey,
		ConfigurationAspect.MapToStoredProcedures,
		ConfigurationAspect.NavigationProperty,
		ConfigurationAspect.ToTable,
		ConfigurationAspect.HasTableAnnotation
	};

	public Type ClrType => _type;

	internal ConventionTypeConfiguration(Type type, ModelConfiguration modelConfiguration)
		: this(type, null, null, modelConfiguration)
	{
	}

	internal ConventionTypeConfiguration(Type type, Func<EntityTypeConfiguration> entityTypeConfiguration, ModelConfiguration modelConfiguration)
		: this(type, entityTypeConfiguration, null, modelConfiguration)
	{
	}

	internal ConventionTypeConfiguration(Type type, Func<ComplexTypeConfiguration> complexTypeConfiguration, ModelConfiguration modelConfiguration)
		: this(type, null, complexTypeConfiguration, modelConfiguration)
	{
	}

	private ConventionTypeConfiguration(Type type, Func<EntityTypeConfiguration> entityTypeConfiguration, Func<ComplexTypeConfiguration> complexTypeConfiguration, ModelConfiguration modelConfiguration)
	{
		_type = type;
		_entityTypeConfiguration = entityTypeConfiguration;
		_complexTypeConfiguration = complexTypeConfiguration;
		_modelConfiguration = modelConfiguration;
	}

	public ConventionTypeConfiguration HasEntitySetName(string entitySetName)
	{
		Check.NotEmpty(entitySetName, "entitySetName");
		ValidateConfiguration(ConfigurationAspect.HasEntitySetName);
		if (_entityTypeConfiguration != null && _entityTypeConfiguration().EntitySetName == null)
		{
			_entityTypeConfiguration().EntitySetName = entitySetName;
		}
		return this;
	}

	public ConventionTypeConfiguration Ignore()
	{
		ValidateConfiguration(ConfigurationAspect.IgnoreType);
		if (_entityTypeConfiguration == null && _complexTypeConfiguration == null)
		{
			_modelConfiguration.Ignore(_type);
		}
		return this;
	}

	public ConventionTypeConfiguration IsComplexType()
	{
		ValidateConfiguration(ConfigurationAspect.IsComplexType);
		if (_entityTypeConfiguration == null && _complexTypeConfiguration == null)
		{
			_modelConfiguration.ComplexType(_type);
		}
		return this;
	}

	public ConventionTypeConfiguration Ignore(string propertyName)
	{
		Check.NotEmpty(propertyName, "propertyName");
		PropertyInfo instanceProperty = _type.GetInstanceProperty(propertyName);
		if (instanceProperty == null)
		{
			throw new InvalidOperationException(Strings.NoSuchProperty(propertyName, _type.Name));
		}
		Ignore(instanceProperty);
		return this;
	}

	public ConventionTypeConfiguration Ignore(PropertyInfo propertyInfo)
	{
		Check.NotNull(propertyInfo, "propertyInfo");
		ValidateConfiguration(ConfigurationAspect.Ignore);
		if (propertyInfo != null)
		{
			if (_entityTypeConfiguration != null)
			{
				_entityTypeConfiguration().Ignore(propertyInfo);
			}
			if (_complexTypeConfiguration != null)
			{
				_complexTypeConfiguration().Ignore(propertyInfo);
			}
		}
		return this;
	}

	public ConventionPrimitivePropertyConfiguration Property(string propertyName)
	{
		Check.NotEmpty(propertyName, "propertyName");
		PropertyInfo instanceProperty = _type.GetInstanceProperty(propertyName);
		if (instanceProperty == null)
		{
			throw new InvalidOperationException(Strings.NoSuchProperty(propertyName, _type.Name));
		}
		return Property(instanceProperty);
	}

	public ConventionPrimitivePropertyConfiguration Property(PropertyInfo propertyInfo)
	{
		Check.NotNull(propertyInfo, "propertyInfo");
		return Property(new PropertyPath(propertyInfo));
	}

	internal ConventionPrimitivePropertyConfiguration Property(PropertyPath propertyPath)
	{
		ValidateConfiguration(ConfigurationAspect.Property);
		PropertyInfo propertyInfo = propertyPath.Last();
		if (!propertyInfo.IsValidEdmScalarProperty())
		{
			throw new InvalidOperationException(Strings.LightweightEntityConfiguration_NonScalarProperty(propertyPath));
		}
		System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration propertyConfiguration = ((_entityTypeConfiguration != null) ? _entityTypeConfiguration().Property(propertyPath) : ((_complexTypeConfiguration != null) ? _complexTypeConfiguration().Property(propertyPath) : null));
		return new ConventionPrimitivePropertyConfiguration(propertyInfo, () => propertyConfiguration);
	}

	internal ConventionNavigationPropertyConfiguration NavigationProperty(string propertyName)
	{
		Check.NotEmpty(propertyName, "propertyName");
		PropertyInfo instanceProperty = _type.GetInstanceProperty(propertyName);
		if (instanceProperty == null)
		{
			throw new InvalidOperationException(Strings.NoSuchProperty(propertyName, _type.Name));
		}
		return NavigationProperty(instanceProperty);
	}

	internal ConventionNavigationPropertyConfiguration NavigationProperty(PropertyInfo propertyInfo)
	{
		Check.NotNull(propertyInfo, "propertyInfo");
		return NavigationProperty(new PropertyPath(propertyInfo));
	}

	internal ConventionNavigationPropertyConfiguration NavigationProperty(PropertyPath propertyPath)
	{
		ValidateConfiguration(ConfigurationAspect.NavigationProperty);
		PropertyInfo propertyInfo = propertyPath.Last();
		if (!propertyInfo.IsValidEdmNavigationProperty())
		{
			throw new InvalidOperationException(Strings.LightweightEntityConfiguration_InvalidNavigationProperty(propertyPath));
		}
		return new ConventionNavigationPropertyConfiguration((_entityTypeConfiguration != null) ? _entityTypeConfiguration().Navigation(propertyInfo) : null, _modelConfiguration);
	}

	public ConventionTypeConfiguration HasKey(string propertyName)
	{
		Check.NotEmpty(propertyName, "propertyName");
		PropertyInfo instanceProperty = _type.GetInstanceProperty(propertyName);
		if (instanceProperty == null)
		{
			throw new InvalidOperationException(Strings.NoSuchProperty(propertyName, _type.Name));
		}
		return HasKey(instanceProperty);
	}

	public ConventionTypeConfiguration HasKey(PropertyInfo propertyInfo)
	{
		Check.NotNull(propertyInfo, "propertyInfo");
		ValidateConfiguration(ConfigurationAspect.HasKey);
		if (_entityTypeConfiguration != null && !_entityTypeConfiguration().IsKeyConfigured)
		{
			_entityTypeConfiguration().Key(propertyInfo);
		}
		return this;
	}

	public ConventionTypeConfiguration HasKey(IEnumerable<string> propertyNames)
	{
		Check.NotNull(propertyNames, "propertyNames");
		PropertyInfo[] keyProperties = propertyNames.Select(delegate(string n)
		{
			PropertyInfo instanceProperty = _type.GetInstanceProperty(n);
			if (instanceProperty == null)
			{
				throw new InvalidOperationException(Strings.NoSuchProperty(n, _type.Name));
			}
			return instanceProperty;
		}).ToArray();
		return HasKey(keyProperties);
	}

	public ConventionTypeConfiguration HasKey(IEnumerable<PropertyInfo> keyProperties)
	{
		Check.NotNull(keyProperties, "keyProperties");
		EntityUtil.CheckArgumentContainsNull(ref keyProperties, "keyProperties");
		EntityUtil.CheckArgumentEmpty(ref keyProperties, (string p) => Strings.CollectionEmpty(p, "HasKey"), "keyProperties");
		ValidateConfiguration(ConfigurationAspect.HasKey);
		if (_entityTypeConfiguration != null && !_entityTypeConfiguration().IsKeyConfigured)
		{
			_entityTypeConfiguration().Key(keyProperties);
		}
		return this;
	}

	public ConventionTypeConfiguration ToTable(string tableName)
	{
		Check.NotEmpty(tableName, "tableName");
		ValidateConfiguration(ConfigurationAspect.ToTable);
		if (_entityTypeConfiguration != null && !_entityTypeConfiguration().IsTableNameConfigured)
		{
			DatabaseName databaseName = DatabaseName.Parse(tableName);
			_entityTypeConfiguration().ToTable(databaseName.Name, databaseName.Schema);
		}
		return this;
	}

	public ConventionTypeConfiguration ToTable(string tableName, string schemaName)
	{
		Check.NotEmpty(tableName, "tableName");
		ValidateConfiguration(ConfigurationAspect.ToTable);
		if (_entityTypeConfiguration != null && !_entityTypeConfiguration().IsTableNameConfigured)
		{
			_entityTypeConfiguration().ToTable(tableName, schemaName);
		}
		return this;
	}

	public ConventionTypeConfiguration HasTableAnnotation(string name, object value)
	{
		Check.NotEmpty(name, "name");
		ValidateConfiguration(ConfigurationAspect.HasTableAnnotation);
		if (_entityTypeConfiguration != null && !_entityTypeConfiguration().Annotations.ContainsKey(name))
		{
			_entityTypeConfiguration().SetAnnotation(name, value);
		}
		return this;
	}

	public ConventionTypeConfiguration MapToStoredProcedures()
	{
		ValidateConfiguration(ConfigurationAspect.MapToStoredProcedures);
		if (_entityTypeConfiguration != null)
		{
			_entityTypeConfiguration().MapToStoredProcedures();
		}
		return this;
	}

	public ConventionTypeConfiguration MapToStoredProcedures(Action<ConventionModificationStoredProceduresConfiguration> modificationStoredProceduresConfigurationAction)
	{
		Check.NotNull(modificationStoredProceduresConfigurationAction, "modificationStoredProceduresConfigurationAction");
		ValidateConfiguration(ConfigurationAspect.MapToStoredProcedures);
		ConventionModificationStoredProceduresConfiguration conventionModificationStoredProceduresConfiguration = new ConventionModificationStoredProceduresConfiguration(_type);
		modificationStoredProceduresConfigurationAction(conventionModificationStoredProceduresConfiguration);
		MapToStoredProcedures(conventionModificationStoredProceduresConfiguration.Configuration);
		return this;
	}

	internal void MapToStoredProcedures(ModificationStoredProceduresConfiguration modificationStoredProceduresConfiguration)
	{
		if (_entityTypeConfiguration != null)
		{
			_entityTypeConfiguration().MapToStoredProcedures(modificationStoredProceduresConfiguration, allowOverride: false);
		}
	}

	private void ValidateConfiguration(ConfigurationAspect aspect)
	{
		_currentConfigurationAspect |= aspect;
		if (_currentConfigurationAspect.HasFlag(ConfigurationAspect.IgnoreType) && ConfigurationAspectsConflictingWithIgnoreType.Any((ConfigurationAspect ca) => _currentConfigurationAspect.HasFlag(ca)))
		{
			throw new InvalidOperationException(Strings.LightweightEntityConfiguration_ConfigurationConflict_IgnoreType(ConfigurationAspectsConflictingWithIgnoreType.First((ConfigurationAspect ca) => _currentConfigurationAspect.HasFlag(ca)), _type.Name));
		}
		if (_currentConfigurationAspect.HasFlag(ConfigurationAspect.IsComplexType) && ConfigurationAspectsConflictingWithComplexType.Any((ConfigurationAspect ca) => _currentConfigurationAspect.HasFlag(ca)))
		{
			throw new InvalidOperationException(Strings.LightweightEntityConfiguration_ConfigurationConflict_ComplexType(ConfigurationAspectsConflictingWithComplexType.First((ConfigurationAspect ca) => _currentConfigurationAspect.HasFlag(ca)), _type.Name));
		}
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
public class ConventionTypeConfiguration<T> where T : class
{
	private readonly ConventionTypeConfiguration _configuration;

	public Type ClrType => _configuration.ClrType;

	internal ConventionTypeConfiguration(Type type, Func<EntityTypeConfiguration> entityTypeConfiguration, ModelConfiguration modelConfiguration)
	{
		_configuration = new ConventionTypeConfiguration(type, entityTypeConfiguration, modelConfiguration);
	}

	internal ConventionTypeConfiguration(Type type, Func<ComplexTypeConfiguration> complexTypeConfiguration, ModelConfiguration modelConfiguration)
	{
		_configuration = new ConventionTypeConfiguration(type, complexTypeConfiguration, modelConfiguration);
	}

	internal ConventionTypeConfiguration(Type type, ModelConfiguration modelConfiguration)
	{
		_configuration = new ConventionTypeConfiguration(type, modelConfiguration);
	}

	[Conditional("DEBUG")]
	private static void VerifyType(Type type)
	{
	}

	public ConventionTypeConfiguration<T> HasEntitySetName(string entitySetName)
	{
		_configuration.HasEntitySetName(entitySetName);
		return this;
	}

	public ConventionTypeConfiguration<T> Ignore()
	{
		_configuration.Ignore();
		return this;
	}

	public ConventionTypeConfiguration<T> IsComplexType()
	{
		_configuration.IsComplexType();
		return this;
	}

	public ConventionTypeConfiguration<T> Ignore<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
	{
		Check.NotNull(propertyExpression, "propertyExpression");
		_configuration.Ignore(propertyExpression.GetSimplePropertyAccess().Single());
		return this;
	}

	public ConventionPrimitivePropertyConfiguration Property<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
	{
		Check.NotNull(propertyExpression, "propertyExpression");
		return _configuration.Property(propertyExpression.GetComplexPropertyAccess());
	}

	internal ConventionNavigationPropertyConfiguration NavigationProperty<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
	{
		Check.NotNull(propertyExpression, "propertyExpression");
		return _configuration.NavigationProperty(propertyExpression.GetComplexPropertyAccess());
	}

	public ConventionTypeConfiguration<T> HasKey<TProperty>(Expression<Func<T, TProperty>> keyExpression)
	{
		Check.NotNull(keyExpression, "keyExpression");
		_configuration.HasKey(from p in keyExpression.GetSimplePropertyAccessList()
			select p.Single());
		return this;
	}

	public ConventionTypeConfiguration<T> ToTable(string tableName)
	{
		Check.NotEmpty(tableName, "tableName");
		_configuration.ToTable(tableName);
		return this;
	}

	public ConventionTypeConfiguration<T> ToTable(string tableName, string schemaName)
	{
		Check.NotEmpty(tableName, "tableName");
		_configuration.ToTable(tableName, schemaName);
		return this;
	}

	public ConventionTypeConfiguration<T> HasTableAnnotation(string name, object value)
	{
		Check.NotEmpty(name, "name");
		_configuration.HasTableAnnotation(name, value);
		return this;
	}

	public ConventionTypeConfiguration<T> MapToStoredProcedures()
	{
		_configuration.MapToStoredProcedures();
		return this;
	}

	public ConventionTypeConfiguration<T> MapToStoredProcedures(Action<ModificationStoredProceduresConfiguration<T>> modificationStoredProceduresConfigurationAction)
	{
		Check.NotNull(modificationStoredProceduresConfigurationAction, "modificationStoredProceduresConfigurationAction");
		ModificationStoredProceduresConfiguration<T> modificationStoredProceduresConfiguration = new ModificationStoredProceduresConfiguration<T>();
		modificationStoredProceduresConfigurationAction(modificationStoredProceduresConfiguration);
		_configuration.MapToStoredProcedures(modificationStoredProceduresConfiguration.Configuration);
		return this;
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
