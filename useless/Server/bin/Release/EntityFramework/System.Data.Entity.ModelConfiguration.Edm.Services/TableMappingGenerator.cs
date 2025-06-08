using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Edm.Services;

internal class TableMappingGenerator : StructuralTypeMappingGenerator
{
	public TableMappingGenerator(DbProviderManifest providerManifest)
		: base(providerManifest)
	{
	}

	public void Generate(EntityType entityType, DbDatabaseMapping databaseMapping)
	{
		EntitySet entitySet = databaseMapping.Model.GetEntitySet(entityType);
		EntitySetMapping entitySetMapping = databaseMapping.GetEntitySetMapping(entitySet) ?? databaseMapping.AddEntitySetMapping(entitySet);
		EntityTypeMapping entityTypeMapping = entitySetMapping.EntityTypeMappings.FirstOrDefault((EntityTypeMapping m) => m.EntityTypes.Contains(entitySet.ElementType)) ?? entitySetMapping.EntityTypeMappings.FirstOrDefault();
		EntityType entityType2 = ((entityTypeMapping != null) ? entityTypeMapping.MappingFragments.First().Table : databaseMapping.Database.AddTable(entityType.GetRootType().Name));
		entityTypeMapping = new EntityTypeMapping(null);
		MappingFragment mappingFragment = new MappingFragment(databaseMapping.Database.GetEntitySet(entityType2), entityTypeMapping, makeColumnsDistinct: false);
		entityTypeMapping.AddType(entityType);
		entityTypeMapping.AddFragment(mappingFragment);
		entityTypeMapping.SetClrType(entityType.GetClrType());
		entitySetMapping.AddTypeMapping(entityTypeMapping);
		new PropertyMappingGenerator(_providerManifest).Generate(entityType, entityType.Properties, entitySetMapping, mappingFragment, new List<EdmProperty>(), createNewColumn: false);
	}
}
