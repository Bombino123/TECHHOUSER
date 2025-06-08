using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public class MappingInheritedPropertiesSupportConvention : IDbMappingConvention, IConvention
{
	void IDbMappingConvention.Apply(DbDatabaseMapping databaseMapping)
	{
		Check.NotNull(databaseMapping, "databaseMapping");
		databaseMapping.EntityContainerMappings.SelectMany((EntityContainerMapping ecm) => ecm.EntitySetMappings).Each(delegate(EntitySetMapping esm)
		{
			foreach (EntityTypeMapping entityTypeMapping in esm.EntityTypeMappings)
			{
				if (RemapsInheritedProperties(databaseMapping, entityTypeMapping) && HasBaseWithIsTypeOf(esm, entityTypeMapping.EntityType))
				{
					throw Error.UnsupportedHybridInheritanceMapping(entityTypeMapping.EntityType.Name);
				}
			}
		});
	}

	private static bool RemapsInheritedProperties(DbDatabaseMapping databaseMapping, EntityTypeMapping entityTypeMapping)
	{
		foreach (EdmProperty property in entityTypeMapping.EntityType.Properties.Except(entityTypeMapping.EntityType.DeclaredProperties).Except(entityTypeMapping.EntityType.GetKeyProperties()))
		{
			MappingFragment fragment = GetFragmentForPropertyMapping(entityTypeMapping, property);
			if (fragment == null)
			{
				continue;
			}
			for (EntityType entityType = (EntityType)entityTypeMapping.EntityType.BaseType; entityType != null; entityType = (EntityType)entityType.BaseType)
			{
				if ((from baseTypeMapping in databaseMapping.GetEntityTypeMappings(entityType)
					select GetFragmentForPropertyMapping(baseTypeMapping, property)).Any((MappingFragment baseFragment) => baseFragment != null && baseFragment.Table != fragment.Table))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static MappingFragment GetFragmentForPropertyMapping(EntityTypeMapping entityTypeMapping, EdmProperty property)
	{
		return entityTypeMapping.MappingFragments.SingleOrDefault((MappingFragment tmf) => tmf.ColumnMappings.Any((ColumnMappingBuilder pm) => pm.PropertyPath.Last() == property));
	}

	private static bool HasBaseWithIsTypeOf(EntitySetMapping entitySetMapping, EntityType entityType)
	{
		EdmType baseType;
		for (baseType = entityType.BaseType; baseType != null; baseType = baseType.BaseType)
		{
			if (entitySetMapping.EntityTypeMappings.Where((EntityTypeMapping etm) => etm.EntityType == baseType).Any((EntityTypeMapping etm) => etm.IsHierarchyMapping))
			{
				return true;
			}
		}
		return false;
	}
}
