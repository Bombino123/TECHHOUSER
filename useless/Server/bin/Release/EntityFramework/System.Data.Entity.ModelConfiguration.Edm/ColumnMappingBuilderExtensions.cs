using System.Collections.Generic;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Edm;

internal static class ColumnMappingBuilderExtensions
{
	public static void SyncNullabilityCSSpace(this ColumnMappingBuilder propertyMappingBuilder, DbDatabaseMapping databaseMapping, IEnumerable<EntitySet> entitySets, EntityType toTable)
	{
		EdmProperty edmProperty = propertyMappingBuilder.PropertyPath.Last();
		EntitySetMapping entitySetMapping = null;
		EntityType baseType = (EntityType)edmProperty.DeclaringType.BaseType;
		if (baseType != null)
		{
			entitySetMapping = GetEntitySetMapping(databaseMapping, baseType, entitySets);
		}
		while (baseType != null)
		{
			if (toTable == entitySetMapping.EntityTypeMappings.First((EntityTypeMapping m) => m.EntityType == baseType).GetPrimaryTable())
			{
				return;
			}
			baseType = (EntityType)baseType.BaseType;
		}
		propertyMappingBuilder.ColumnProperty.Nullable = edmProperty.Nullable;
	}

	private static EntitySetMapping GetEntitySetMapping(DbDatabaseMapping databaseMapping, EntityType cSpaceEntityType, IEnumerable<EntitySet> entitySets)
	{
		while (cSpaceEntityType.BaseType != null)
		{
			cSpaceEntityType = (EntityType)cSpaceEntityType.BaseType;
		}
		EntitySet cSpaceEntitySet = entitySets.First((EntitySet s) => s.ElementType == cSpaceEntityType);
		return databaseMapping.EntityContainerMappings.First().EntitySetMappings.First((EntitySetMapping m) => m.EntitySet == cSpaceEntitySet);
	}
}
