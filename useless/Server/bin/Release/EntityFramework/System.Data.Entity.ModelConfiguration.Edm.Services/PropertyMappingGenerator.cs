using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Edm.Services;

internal class PropertyMappingGenerator : StructuralTypeMappingGenerator
{
	public PropertyMappingGenerator(DbProviderManifest providerManifest)
		: base(providerManifest)
	{
	}

	public void Generate(EntityType entityType, IEnumerable<EdmProperty> properties, EntitySetMapping entitySetMapping, MappingFragment entityTypeMappingFragment, IList<EdmProperty> propertyPath, bool createNewColumn)
	{
		ReadOnlyMetadataCollection<EdmProperty> declaredProperties = entityType.GetRootType().DeclaredProperties;
		foreach (EdmProperty property in properties)
		{
			if (property.IsComplexType && propertyPath.Any((EdmProperty p) => p.IsComplexType && p.ComplexType == property.ComplexType))
			{
				throw Error.CircularComplexTypeHierarchy();
			}
			propertyPath.Add(property);
			if (property.IsComplexType)
			{
				Generate(entityType, property.ComplexType.Properties, entitySetMapping, entityTypeMappingFragment, propertyPath, createNewColumn);
			}
			else
			{
				EdmProperty edmProperty = (from pm in entitySetMapping.EntityTypeMappings.SelectMany((EntityTypeMapping etm) => etm.MappingFragments).SelectMany((MappingFragment etmf) => etmf.ColumnMappings)
					where pm.PropertyPath.SequenceEqual(propertyPath)
					select pm.ColumnProperty).FirstOrDefault();
				if (edmProperty == null || createNewColumn)
				{
					string columnName = string.Join("_", propertyPath.Select((EdmProperty p) => p.Name));
					edmProperty = MapTableColumn(property, columnName, !declaredProperties.Contains(propertyPath.First()));
					entityTypeMappingFragment.Table.AddColumn(edmProperty);
					if (entityType.KeyProperties().Contains(property))
					{
						entityTypeMappingFragment.Table.AddKeyMember(edmProperty);
					}
				}
				entityTypeMappingFragment.AddColumnMapping(new ColumnMappingBuilder(edmProperty, propertyPath.ToList()));
			}
			propertyPath.Remove(property);
		}
	}
}
