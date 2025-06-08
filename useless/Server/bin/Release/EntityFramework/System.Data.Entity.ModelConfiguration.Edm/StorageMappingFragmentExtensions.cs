using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Edm;

internal static class StorageMappingFragmentExtensions
{
	private const string DefaultDiscriminatorAnnotation = "DefaultDiscriminator";

	private const string ConditionOnlyFragmentAnnotation = "ConditionOnlyFragment";

	private const string UnmappedPropertiesFragmentAnnotation = "UnmappedPropertiesFragment";

	public static EdmProperty GetDefaultDiscriminator(this MappingFragment entityTypeMapppingFragment)
	{
		return (EdmProperty)entityTypeMapppingFragment.Annotations.GetAnnotation("DefaultDiscriminator");
	}

	public static void SetDefaultDiscriminator(this MappingFragment entityTypeMappingFragment, EdmProperty discriminator)
	{
		entityTypeMappingFragment.Annotations.SetAnnotation("DefaultDiscriminator", discriminator);
	}

	public static void RemoveDefaultDiscriminatorAnnotation(this MappingFragment entityTypeMappingFragment)
	{
		entityTypeMappingFragment.Annotations.RemoveAnnotation("DefaultDiscriminator");
	}

	public static void RemoveDefaultDiscriminator(this MappingFragment entityTypeMappingFragment, EntitySetMapping entitySetMapping)
	{
		EdmProperty discriminatorColumn = entityTypeMappingFragment.RemoveDefaultDiscriminatorCondition();
		if (discriminatorColumn != null)
		{
			EntityType table = entityTypeMappingFragment.Table;
			table.Properties.Where((EdmProperty c) => c.Name.Equals(discriminatorColumn.Name, StringComparison.Ordinal)).ToList().Each(table.RemoveMember);
		}
		if (entitySetMapping != null && entityTypeMappingFragment.IsConditionOnlyFragment() && !entityTypeMappingFragment.ColumnConditions.Any())
		{
			EntityTypeMapping entityTypeMapping = entitySetMapping.EntityTypeMappings.Single((EntityTypeMapping etm) => etm.MappingFragments.Contains(entityTypeMappingFragment));
			entityTypeMapping.RemoveFragment(entityTypeMappingFragment);
			if (entityTypeMapping.MappingFragments.Count == 0)
			{
				entitySetMapping.RemoveTypeMapping(entityTypeMapping);
			}
		}
	}

	public static EdmProperty RemoveDefaultDiscriminatorCondition(this MappingFragment entityTypeMappingFragment)
	{
		EdmProperty defaultDiscriminator = entityTypeMappingFragment.GetDefaultDiscriminator();
		if (defaultDiscriminator != null && entityTypeMappingFragment.ColumnConditions.Any())
		{
			entityTypeMappingFragment.ClearConditions();
		}
		entityTypeMappingFragment.RemoveDefaultDiscriminatorAnnotation();
		return defaultDiscriminator;
	}

	public static void AddDiscriminatorCondition(this MappingFragment entityTypeMapppingFragment, EdmProperty discriminatorColumn, object value)
	{
		entityTypeMapppingFragment.AddConditionProperty(new ValueConditionMapping(discriminatorColumn, value));
	}

	public static void AddNullabilityCondition(this MappingFragment entityTypeMapppingFragment, EdmProperty column, bool isNull)
	{
		entityTypeMapppingFragment.AddConditionProperty(new IsNullConditionMapping(column, isNull));
	}

	public static bool IsConditionOnlyFragment(this MappingFragment entityTypeMapppingFragment)
	{
		object annotation = entityTypeMapppingFragment.Annotations.GetAnnotation("ConditionOnlyFragment");
		if (annotation != null)
		{
			return (bool)annotation;
		}
		return false;
	}

	public static void SetIsConditionOnlyFragment(this MappingFragment entityTypeMapppingFragment, bool isConditionOnlyFragment)
	{
		if (isConditionOnlyFragment)
		{
			entityTypeMapppingFragment.Annotations.SetAnnotation("ConditionOnlyFragment", isConditionOnlyFragment);
		}
		else
		{
			entityTypeMapppingFragment.Annotations.RemoveAnnotation("ConditionOnlyFragment");
		}
	}

	public static bool IsUnmappedPropertiesFragment(this MappingFragment entityTypeMapppingFragment)
	{
		object annotation = entityTypeMapppingFragment.Annotations.GetAnnotation("UnmappedPropertiesFragment");
		if (annotation != null)
		{
			return (bool)annotation;
		}
		return false;
	}

	public static void SetIsUnmappedPropertiesFragment(this MappingFragment entityTypeMapppingFragment, bool isUnmappedPropertiesFragment)
	{
		if (isUnmappedPropertiesFragment)
		{
			entityTypeMapppingFragment.Annotations.SetAnnotation("UnmappedPropertiesFragment", isUnmappedPropertiesFragment);
		}
		else
		{
			entityTypeMapppingFragment.Annotations.RemoveAnnotation("UnmappedPropertiesFragment");
		}
	}
}
