using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.ModelConfiguration.Configuration.Mapping;
using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.ModelConfiguration.Utilities;
using System.Data.Entity.Resources;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Configuration;

public class NotNullConditionConfiguration
{
	private readonly EntityMappingConfiguration _entityMappingConfiguration;

	internal PropertyPath PropertyPath { get; set; }

	internal NotNullConditionConfiguration(EntityMappingConfiguration entityMapConfiguration, PropertyPath propertyPath)
	{
		_entityMappingConfiguration = entityMapConfiguration;
		PropertyPath = propertyPath;
	}

	private NotNullConditionConfiguration(EntityMappingConfiguration owner, NotNullConditionConfiguration source)
	{
		_entityMappingConfiguration = owner;
		PropertyPath = source.PropertyPath;
	}

	internal virtual NotNullConditionConfiguration Clone(EntityMappingConfiguration owner)
	{
		return new NotNullConditionConfiguration(owner, this);
	}

	public void HasValue()
	{
		_entityMappingConfiguration.AddNullabilityCondition(this);
	}

	internal void Configure(DbDatabaseMapping databaseMapping, MappingFragment fragment, EntityType entityType)
	{
		IEnumerable<EdmPropertyPath> edmPropertyPath = EntityMappingConfiguration.PropertyPathToEdmPropertyPath(PropertyPath, entityType);
		if (edmPropertyPath.Count() > 1)
		{
			throw Error.InvalidNotNullCondition(PropertyPath.ToString(), entityType.Name);
		}
		EdmProperty edmProperty = (from pm in fragment.ColumnMappings
			where pm.PropertyPath.SequenceEqual(edmPropertyPath.Single())
			select pm.ColumnProperty).SingleOrDefault();
		if (edmProperty == null || !fragment.Table.Properties.Contains(edmProperty))
		{
			throw Error.InvalidNotNullCondition(PropertyPath.ToString(), entityType.Name);
		}
		if (ValueConditionConfiguration.AnyBaseTypeToTableWithoutColumnCondition(databaseMapping, entityType, fragment.Table, edmProperty))
		{
			edmProperty.Nullable = true;
		}
		System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration primitivePropertyConfiguration = new System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration();
		primitivePropertyConfiguration.IsNullable = false;
		primitivePropertyConfiguration.OverridableConfigurationParts = OverridableConfigurationParts.OverridableInSSpace;
		primitivePropertyConfiguration.Configure(edmPropertyPath.Single().Last());
		fragment.AddNullabilityCondition(edmProperty, isNull: false);
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
