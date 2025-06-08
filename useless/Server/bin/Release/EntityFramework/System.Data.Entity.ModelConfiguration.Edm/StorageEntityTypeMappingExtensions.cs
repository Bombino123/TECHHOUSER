using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Edm;

internal static class StorageEntityTypeMappingExtensions
{
	public static object GetConfiguration(this EntityTypeMapping entityTypeMapping)
	{
		return entityTypeMapping.Annotations.GetConfiguration();
	}

	public static void SetConfiguration(this EntityTypeMapping entityTypeMapping, object configuration)
	{
		entityTypeMapping.Annotations.SetConfiguration(configuration);
	}

	public static ColumnMappingBuilder GetPropertyMapping(this EntityTypeMapping entityTypeMapping, params EdmProperty[] propertyPath)
	{
		return entityTypeMapping.MappingFragments.SelectMany((MappingFragment f) => f.ColumnMappings).Single((ColumnMappingBuilder p) => p.PropertyPath.SequenceEqual(propertyPath));
	}

	public static EntityType GetPrimaryTable(this EntityTypeMapping entityTypeMapping)
	{
		return entityTypeMapping.MappingFragments.First().Table;
	}

	public static bool UsesOtherTables(this EntityTypeMapping entityTypeMapping, EntityType table)
	{
		return entityTypeMapping.MappingFragments.Any((MappingFragment f) => f.Table != table);
	}

	public static Type GetClrType(this EntityTypeMapping entityTypeMappping)
	{
		return entityTypeMappping.Annotations.GetClrType();
	}

	public static void SetClrType(this EntityTypeMapping entityTypeMapping, Type type)
	{
		entityTypeMapping.Annotations.SetClrType(type);
	}

	public static EntityTypeMapping Clone(this EntityTypeMapping entityTypeMapping)
	{
		EntityTypeMapping entityTypeMapping2 = new EntityTypeMapping(null);
		entityTypeMapping2.AddType(entityTypeMapping.EntityType);
		entityTypeMapping.Annotations.Copy(entityTypeMapping2.Annotations);
		return entityTypeMapping2;
	}
}
