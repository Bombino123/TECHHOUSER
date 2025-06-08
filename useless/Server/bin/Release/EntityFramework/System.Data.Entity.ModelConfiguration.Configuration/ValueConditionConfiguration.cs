using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.ModelConfiguration.Configuration.Mapping;
using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.ModelConfiguration.Edm.Services;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Diagnostics;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Configuration;

[DebuggerDisplay("{Discriminator}")]
public class ValueConditionConfiguration
{
	private readonly EntityMappingConfiguration _entityMappingConfiguration;

	private System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration _configuration;

	internal string Discriminator { get; set; }

	internal object Value { get; set; }

	internal ValueConditionConfiguration(EntityMappingConfiguration entityMapConfiguration, string discriminator)
	{
		_entityMappingConfiguration = entityMapConfiguration;
		Discriminator = discriminator;
	}

	private ValueConditionConfiguration(EntityMappingConfiguration owner, ValueConditionConfiguration source)
	{
		_entityMappingConfiguration = owner;
		Discriminator = source.Discriminator;
		Value = source.Value;
		_configuration = ((source._configuration == null) ? null : source._configuration.Clone());
	}

	internal virtual ValueConditionConfiguration Clone(EntityMappingConfiguration owner)
	{
		return new ValueConditionConfiguration(owner, this);
	}

	private T GetOrCreateConfiguration<T>() where T : System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration, new()
	{
		if (_configuration == null)
		{
			_configuration = new T();
		}
		else if (!(_configuration is T))
		{
			T val = new T();
			val.CopyFrom(_configuration);
			_configuration = val;
		}
		_configuration.OverridableConfigurationParts = OverridableConfigurationParts.None;
		return (T)_configuration;
	}

	public PrimitiveColumnConfiguration HasValue<T>(T value) where T : struct
	{
		ValidateValueType(value);
		Value = value;
		_entityMappingConfiguration.AddValueCondition(this);
		return new PrimitiveColumnConfiguration(GetOrCreateConfiguration<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration>());
	}

	public PrimitiveColumnConfiguration HasValue<T>(T? value) where T : struct
	{
		ValidateValueType(value);
		Value = value;
		_entityMappingConfiguration.AddValueCondition(this);
		return new PrimitiveColumnConfiguration(GetOrCreateConfiguration<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration>());
	}

	public StringColumnConfiguration HasValue(string value)
	{
		Value = value;
		_entityMappingConfiguration.AddValueCondition(this);
		return new StringColumnConfiguration(GetOrCreateConfiguration<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.StringPropertyConfiguration>());
	}

	private static void ValidateValueType(object value)
	{
		if (value != null && !value.GetType().IsPrimitiveType(out var _))
		{
			throw Error.InvalidDiscriminatorType(value.GetType().Name);
		}
	}

	internal static IEnumerable<MappingFragment> GetMappingFragmentsWithColumnAsDefaultDiscriminator(DbDatabaseMapping databaseMapping, EntityType table, EdmProperty column)
	{
		return from tmf in databaseMapping.EntityContainerMappings.SelectMany((EntityContainerMapping ecm) => ecm.EntitySetMappings).SelectMany((EntitySetMapping esm) => esm.EntityTypeMappings).SelectMany((EntityTypeMapping etm) => etm.MappingFragments)
			where tmf.Table == table && tmf.GetDefaultDiscriminator() == column
			select tmf;
	}

	internal static bool AnyBaseTypeToTableWithoutColumnCondition(DbDatabaseMapping databaseMapping, EntityType entityType, EntityType table, EdmProperty column)
	{
		for (EdmType baseType = entityType.BaseType; baseType != null; baseType = baseType.BaseType)
		{
			if (!baseType.Abstract)
			{
				List<MappingFragment> source = (from tmf in databaseMapping.GetEntityTypeMappings((EntityType)baseType).SelectMany((EntityTypeMapping etm) => etm.MappingFragments)
					where tmf.Table == table
					select tmf).ToList();
				if (source.Any() && source.SelectMany((MappingFragment etmf) => etmf.ColumnConditions).All((ConditionPropertyMapping cc) => cc.Column != column))
				{
					return true;
				}
			}
		}
		return false;
	}

	internal void Configure(DbDatabaseMapping databaseMapping, MappingFragment fragment, EntityType entityType, DbProviderManifest providerManifest)
	{
		EdmProperty edmProperty = fragment.Table.Properties.SingleOrDefault((EdmProperty c) => string.Equals(c.Name, Discriminator, StringComparison.Ordinal));
		if (edmProperty != null && GetMappingFragmentsWithColumnAsDefaultDiscriminator(databaseMapping, fragment.Table, edmProperty).Any())
		{
			edmProperty.Name = fragment.Table.Properties.Select((EdmProperty p) => p.Name).Uniquify(edmProperty.Name);
			edmProperty = null;
		}
		if (edmProperty == null)
		{
			TypeUsage storeType = providerManifest.GetStoreType(DatabaseMappingGenerator.DiscriminatorTypeUsage);
			edmProperty = new EdmProperty(Discriminator, storeType)
			{
				Nullable = false
			};
			TablePrimitiveOperations.AddColumn(fragment.Table, edmProperty);
		}
		if (AnyBaseTypeToTableWithoutColumnCondition(databaseMapping, entityType, fragment.Table, edmProperty))
		{
			edmProperty.Nullable = true;
		}
		System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration primitivePropertyConfiguration = edmProperty.GetConfiguration() as System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration;
		if (Value != null)
		{
			ConfigureColumnType(providerManifest, primitivePropertyConfiguration, edmProperty);
			fragment.AddDiscriminatorCondition(edmProperty, Value);
		}
		else
		{
			if (string.IsNullOrWhiteSpace(edmProperty.TypeName))
			{
				TypeUsage storeType2 = providerManifest.GetStoreType(DatabaseMappingGenerator.DiscriminatorTypeUsage);
				edmProperty.PrimitiveType = (PrimitiveType)storeType2.EdmType;
				edmProperty.MaxLength = 128;
				edmProperty.Nullable = false;
			}
			GetOrCreateConfiguration<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration>().IsNullable = true;
			fragment.AddNullabilityCondition(edmProperty, isNull: true);
		}
		if (_configuration != null)
		{
			if (primitivePropertyConfiguration != null && (primitivePropertyConfiguration.OverridableConfigurationParts & OverridableConfigurationParts.OverridableInCSpace) != OverridableConfigurationParts.OverridableInCSpace && !primitivePropertyConfiguration.IsCompatible(_configuration, inCSpace: true, out var errorMessage))
			{
				throw Error.ConflictingColumnConfiguration(edmProperty, fragment.Table, errorMessage);
			}
			if (_configuration.IsNullable.HasValue)
			{
				edmProperty.Nullable = _configuration.IsNullable.Value;
			}
			_configuration.Configure(edmProperty, fragment.Table, providerManifest);
		}
	}

	private void ConfigureColumnType(DbProviderManifest providerManifest, System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration existingConfiguration, EdmProperty discriminatorColumn)
	{
		if ((existingConfiguration == null || existingConfiguration.ColumnType == null) && (_configuration == null || _configuration.ColumnType == null))
		{
			Value.GetType().IsPrimitiveType(out var primitiveType);
			PrimitiveType primitiveType2 = (PrimitiveType)providerManifest.GetStoreType((primitiveType == PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)) ? DatabaseMappingGenerator.DiscriminatorTypeUsage : TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(primitiveType.PrimitiveTypeKind))).EdmType;
			if (existingConfiguration != null && !discriminatorColumn.TypeName.Equals(primitiveType2.Name, StringComparison.OrdinalIgnoreCase))
			{
				throw Error.ConflictingInferredColumnType(discriminatorColumn.Name, discriminatorColumn.TypeName, primitiveType2.Name);
			}
			discriminatorColumn.PrimitiveType = primitiveType2;
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
